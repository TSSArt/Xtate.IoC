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
using System.Threading;

namespace Xtate.IoC.Test;

[TestClass]
public class AncestorTest
{
    [TestMethod]
    public async Task AncestorSimpleTest_ShouldResolveDependenciesCorrectly()
    {
        // Arrange
        await using var container = Container.Create(s =>
                                                     {
                                                         s.AddType<ChildPost>();
                                                         s.AddType<GrandParent>();
                                                         s.AddType<Parent>();
                                                         s.AddType<Child>();
                                                         s.AddType<Child, int>();
                                                         s.AddImplementation<LazyImpl<Any>>().For<ILazy<Any>>();
                                                         s.AddSharedImplementationSync<ServiceProviderActions>(SharedWithin.Scope).For<ServiceProviderActions>().For<IServiceProviderActions>();
                                                         s.AddFactory<LazyImpl<Any>>().For<Lazy<Any>>();
                                                     });

        // Act
        var grandParent = await container.GetRequiredService<GrandParent>();
        var childPost = await container.GetRequiredService<ChildPost>();
        var child = await container.GetRequiredService<Child, int>(5);

        // Assert
        Assert.AreEqual(typeof(Parent), grandParent.ParentInstance.ChildInstance.ParentInstance1.Value!.GetType());
        Assert.AreEqual(typeof(Parent), grandParent.ParentInstance.ChildInstance.ParentInstance2.Value!.GetType());
        Assert.AreEqual(typeof(Parent), grandParent.ParentInstance.ChildInstance.ParentInstance3.Value!.GetType());
        Assert.AreEqual(typeof(GrandParent), grandParent.ParentInstance.GrandParentInstance.Value!.GetType());
        Assert.AreEqual(typeof(Parent), childPost.ParentInstance.Value!.GetType());
        Assert.AreEqual(typeof(Parent), childPost.ParentInstanceGetter().GetType());
        Assert.IsNotNull(child);
    }

    [TestMethod]
    public void AncestorOtherTest_ShouldValidateTypeKeyArguments()
    {
        // Arrange
        var k1 = TypeKey.ImplementationKey<object, ValueTuple>();
        var k2 = TypeKey.ImplementationKey<object, int>();
        var k3 = TypeKey.ImplementationKey<List<int>, int>();

        // Act & Assert
        Assert.IsTrue(k1.IsEmptyArg);
        Assert.IsFalse(k2.IsEmptyArg);
        Assert.IsFalse(k3.IsEmptyArg);
        Assert.IsTrue(((GenericTypeKey)k3).DefinitionKey.IsEmptyArg);
    }

    private delegate T Lazy<out T>();

    private interface ILazy<out T>
    {
        T? Value { get; }
    }

    [UsedImplicitly]
    private class LazyImpl<T> : ILazy<T>, IAsyncInitialization
    {
        private ValueTask<T?> _task;

        public LazyImpl(ServiceProviderActions actions, Func<ValueTask<T?>> factory) => _task = actions.TryCapture(this) ? default : factory().Preserve();

    #region Interface IAsyncInitialization

        Task IAsyncInitialization.Initialization => _task.IsCompletedSuccessfully ? Task.CompletedTask : _task.AsTask();

    #endregion

    #region Interface ILazy<T>

        T? ILazy<T>.Value => GetValue();

    #endregion

        [UsedImplicitly]
        public Lazy<T?> GetValueFunc() => GetValue;

        private T? GetValue() => _task.Result;

        internal void SetValue(T? instance) => _task = new ValueTask<T?>(instance);
    }

    [UsedImplicitly]
    private class ServiceProviderActions : IServiceProviderActions, IServiceProviderDataActions
    {
        private static readonly ConcurrentBag<Container> ContainerPool = [];

        private readonly AsyncLocal<Container?> _local = new();

    #region Interface IServiceProviderActions

        public IServiceProviderDataActions? RegisterServices() => null;

        public IServiceProviderDataActions? ServiceRequesting(TypeKey typeKey) => null;

        public IServiceProviderDataActions? ServiceRequested(TypeKey typeKey) => null;

        public IServiceProviderDataActions? FactoryCalling(TypeKey typeKey) => typeKey.IsEmptyArg ? this : null;

        public IServiceProviderDataActions? FactoryCalled(TypeKey typeKey) => typeKey.IsEmptyArg ? this : null;

    #endregion

    #region Interface IServiceProviderDataActions

        [ExcludeFromCodeCoverage]
        public void RegisterService(ServiceEntry serviceEntry) { }

        [ExcludeFromCodeCoverage]
        public void ServiceRequesting<T, TArg>(TArg argument) { }

        [ExcludeFromCodeCoverage]
        public void ServiceRequested<T, TArg>(T? instance) { }

        public void FactoryCalling<T, TArg>(TArg argument) => GetCurrentContainer().Add((typeof(T), null));

        public void FactoryCalled<T, TArg>(T? instance)
        {
            var container = GetCurrentContainer();

            for (var i = 0; i < container.Count; i ++)
            {
                var (type, lazy) = container[i];

                if (type == typeof(T))
                {
                    container[i] = default;

                    if (lazy is LazyImpl<T> lazyImpl)
                    {
                        lazyImpl.SetValue(instance);
                    }
                }
            }

            container.RemoveAll(static p => p.Type is null);

            if (container.Count == 0)
            {
                _local.Value = null!;

                ContainerPool.Add(container);
            }
        }

    #endregion

        private Container GetCurrentContainer()
        {
            if (_local.Value is { } container)
            {
                return container;
            }

            if (!ContainerPool.TryTake(out container))
            {
                container = [];
            }

            return _local.Value = container;
        }

        public bool TryCapture<T>(LazyImpl<T> lazy)
        {
            var container = GetCurrentContainer();

            for (var i = 0; i < container.Count; i ++)
            {
                var (type, itemLazy) = container[i];

                if (type == typeof(T))
                {
                    if (itemLazy is null)
                    {
                        container[i] = (type, lazy);
                    }
                    else
                    {
                        container.Add((type, lazy));
                    }

                    return true;
                }
            }

            return false;
        }

        private class Container : List<(Type Type, object? Lazy)>;
    }

    private class GrandParent
    {
        [UsedImplicitly]
        public required Parent ParentInstance = null!;
    }

    private class Parent
    {
        [UsedImplicitly]
        public required Child ChildInstance = null!;

        [UsedImplicitly]
        public required ILazy<GrandParent> GrandParentInstance = null!;
    }

    private class Child
    {
        [UsedImplicitly]
        public required ILazy<Parent> ParentInstance1 = null!;

        [UsedImplicitly]
        public required ILazy<Parent> ParentInstance2 = null!;

        [UsedImplicitly]
        public required ILazy<Parent> ParentInstance3 = null!;
    }

    private class ChildPost
    {
        [UsedImplicitly]
        public required ILazy<Parent> ParentInstance = null!;

        [UsedImplicitly]
        public required Lazy<Parent> ParentInstanceGetter = null!;
    }
}