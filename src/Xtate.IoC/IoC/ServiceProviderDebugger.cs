// Copyright © 2019-2025 Sergii Artemenko
// 
// This file is part of the Xtate project. <https://xtate.net/>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Concurrent;
using System.IO;

namespace Xtate.IoC;

public class ServiceProviderDebugger(TextWriter writer, bool leaveOpen = false) : IServiceProviderActions, IServiceProviderDataActions, IDisposable
{
    private readonly AsyncLocal<ServiceLogger> _logger = new();

    private readonly ConcurrentQueue<ServiceLogger> _loggersToDump = new();

    private readonly ConcurrentDictionary<TypeKey, Stat> _stats = new();

    private readonly TextWriter _writer = writer;

    private int _writerOwned;

#region Interface IDisposable

    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

#endregion

#region Interface IServiceProviderActions

    public IServiceProviderDataActions RegisterServices() => this;

    public IServiceProviderDataActions? ServiceRequesting(TypeKey typeKey)
    {
        GetLogger().ServiceRequesting(typeKey);

        return null;
    }

    public IServiceProviderDataActions? FactoryCalling(TypeKey typeKey)
    {
        var stat = GetStat(typeKey);
        stat.FactoryCalling();

        GetLogger().FactoryCalling(stat.CallsCount);

        return null;
    }

    public IServiceProviderDataActions? FactoryCalled(TypeKey typeKey)
    {
        GetStat(typeKey).FactoryCalled();

        return null;
    }

    public IServiceProviderDataActions? ServiceRequested(TypeKey typeKey)
    {
        GetLogger().ServiceRequested();

        return null;
    }

#endregion

#region Interface IServiceProviderDataActions

    public void RegisterService(ServiceEntry serviceEntry)
    {
        _writer.Write(@"REG: ");

        var scope = serviceEntry.InstanceScope.ToString();
        _writer.Write(scope);

        for (var i = 0; i < 18 - scope.Length; i ++)
        {
            _writer.Write(' ');
        }

        _writer.Write(@" | ");
        _writer.WriteLine(serviceEntry.Key.ToString());
    }

    [ExcludeFromCodeCoverage]
    public void ServiceRequesting<T, TArg>(TArg argument) => throw new NotSupportedException();

    [ExcludeFromCodeCoverage]
    public void ServiceRequested<T, TArg>(T? instance) => throw new NotSupportedException();

    [ExcludeFromCodeCoverage]
    public void FactoryCalling<T, TArg>(TArg argument) => throw new NotSupportedException();

    [ExcludeFromCodeCoverage]
    public void FactoryCalled<T, TArg>(T? instance) => throw new NotSupportedException();

#endregion

    private bool TryOwnWriter()
    {
        // Fast-fail check to avoid unnecessary Interlocked operation if already owned
        if (_writerOwned != 0)
        {
            return false;
        }

        return Interlocked.CompareExchange(ref _writerOwned, value: 1, comparand: 0) == 0;
    }

    private void ReleaseWriter()
    {
        DumpLoggersQueue();

        _writerOwned = 0;
    }

    private ServiceLogger GetLogger() => _logger.Value ??= new ServiceLogger(this);

    private Stat GetStat(TypeKey serviceKey) => _stats.GetOrAdd(serviceKey, key => new Stat(key));

    private void DumpStatistics()
    {
        var maxServiceLength = _stats.Max(p => p.Value.ServiceName.Length);

        var list = from pair in _stats
                   let nc = (pair.Value.ServiceName, pair.Value.CallsCount)
                   orderby nc.CallsCount descending, nc.ServiceName
                   select nc;

        foreach (var (serviceName, callsCount) in list)
        {
            _writer.Write(@"STAT: ");
            _writer.Write(serviceName);

            for (var i = 0; i < maxServiceLength - serviceName.Length; i ++)
            {
                _writer.Write(' ');
            }

            _writer.Write(@" | ");
            _writer.Write(callsCount.ToString());
            _writer.WriteLine(callsCount > 1 ? @" calls" : @" call");
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            try
            {
                DumpLoggersQueue();

                DumpStatistics();

                _writer.Flush();

                if (!leaveOpen)
                {
                    _writer.Close();
                }
            }
            catch (ObjectDisposedException)
            {
                // ignore
            }
        }
    }

    private void DumpLoggersQueue()
    {
        while (_loggersToDump.TryDequeue(out var logger))
        {
            logger.DumpLocalWriter();
        }
    }

    private class ServiceLogger(ServiceProviderDebugger parent)
    {
        private bool _factoryCalled;

        private int _level;

        private StringWriter? _localWriter;

        private bool _noFactory;

        private bool _parentWriterOwner;

        private int _previousLevel;

        private TextWriter Writer
        {
            get
            {
                if (TryGetParentWriterOwnership())
                {
                    return parent._writer;
                }

                if (_localWriter is null)
                {
                    _localWriter = new StringWriter();

                    parent._loggersToDump.Enqueue(this);
                }

                return _localWriter;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ServiceRequesting(TypeKey typeKey)
        {
            lock (this)
            {
                _level ++;

                if (_factoryCalled)
                {
                    Writer.WriteLine();

                    _factoryCalled = false;
                }

                WriteIdent();

                Writer.Write(typeKey.ToString());

                _noFactory = true;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void FactoryCalling(int callsCount)
        {
            Writer.Write($@" {{#{callsCount}}}");

            _factoryCalled = true;
            _noFactory = false;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ServiceRequested()
        {
            if (_noFactory)
            {
                Writer.WriteLine(@" {CACHED}");
            }
            else
            {
                if (_factoryCalled)
                {
                    Writer.WriteLine();
                    _factoryCalled = false;
                }
                else
                {
                    WriteIdent();
                    Writer.WriteLine(@"__");
                }
            }

            _noFactory = false;

            if (-- _level == 0 && _parentWriterOwner)
            {
                parent.ReleaseWriter();

                _parentWriterOwner = false;
            }
        }

        private void WriteIdent()
        {
            var padding = false;

            for (var i = 1; i < _level; i ++)
            {
                WriteWithPadding(@"|");
            }

            WriteWithPadding(_level < _previousLevel ? @"\" : @">> ");

            _previousLevel = _level;

            return;

            void WriteWithPadding(string str)
            {
                if (padding)
                {
                    Writer.Write(@"  ");
                }

                padding = true;
                Writer.Write(str);
            }
        }

        private bool TryGetParentWriterOwnership()
        {
            if (_parentWriterOwner)
            {
                return true;
            }

            if (!parent.TryOwnWriter())
            {
                return false;
            }

            _parentWriterOwner = true;

            DumpLocalWriter();

            return true;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void DumpLocalWriter()
        {
            parent._writer.Write(_localWriter?.ToString());

            _localWriter?.Close();

            _localWriter = null;
        }
    }

    private class Stat(TypeKey typeKey)
    {
        private int _deepLevel;

        public string ServiceName => typeKey.ToString() ?? string.Empty;

        public int CallsCount { get; private set; }

        public void FactoryCalling()
        {
            CallsCount ++;

            if (++ _deepLevel > 100)
            {
                throw new DependencyInjectionException(Resources.Exception_CycleReferenceDetectedInContainerConfiguration);
            }
        }

        public void FactoryCalled() => _deepLevel --;
    }
}