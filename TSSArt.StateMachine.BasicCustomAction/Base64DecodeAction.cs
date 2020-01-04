﻿using System;
using System.Buffers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace TSSArt.StateMachine
{
	public class Base64DecodeAction : CustomActionBase
	{
		private readonly string _destination;
		private readonly string _source;

		public Base64DecodeAction(XmlReader xmlReader)
		{
			if (xmlReader == null) throw new ArgumentNullException(nameof(xmlReader));

			_source = xmlReader.GetAttribute("source");
			_destination = xmlReader.GetAttribute("destination");
		}

		public override ValueTask Action(IExecutionContext context, CancellationToken token)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var source = context.DataModel[_source].AsString();

#if NETSTANDARD2_1
			var result = OptimizedDecode(source);

			static string OptimizedDecode(string str)
			{
				var bytes = ArrayPool<byte>.Shared.Rent(str.Length);
				try
				{
					if (!Convert.TryFromBase64String(str, bytes, out var length))
					{
						throw new InvalidOperationException("Can't parse Base64 string");
					}

					return Encoding.UTF8.GetString(bytes.AsSpan(start: 0, length));
				}
				finally
				{
					ArrayPool<byte>.Shared.Return(bytes);
				}
			}
#else
			var result = Encoding.UTF8.GetString(Convert.FromBase64String(source));
#endif
			context.DataModel[_destination] = new DataModelValue(result);

			return default;
		}
	}
}