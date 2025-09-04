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

namespace Xtate.IoC.Test;

[TestClass]
public class ScopeTest
{
    private static async ValueTask<string[]> GetList<T>(IAsyncEnumerable<T> asyncEnumerable)
    {
        var list = new List<string>();

        await foreach (var svc in asyncEnumerable)
        {
            list.Add(((ISomeValue)svc!).Value);
        }

        return [.. list];
    }

    [TestMethod]
    public async Task NoScope_AnyType_ReturnsExpectedValues()
    {
        // Arrange
        await using var container = Container.Create(s =>
                                                     {
                                                         s.AddImplementation<NonGeneric>().For<INonGeneric>();
                                                         s.AddImplementation<Generic<Any>>().For<IGeneric<Any>>();
                                                     });

        // Act
        var nonGenServicesList = await GetList(container.GetServices<INonGeneric>());
        var gen1ServicesList = await GetList(container.GetServices<IGeneric<object>>());
        var gen2ServicesList = await GetList(container.GetServices<IGeneric<int>>());
        var gen3ServicesList = await GetList(container.GetServices<IGeneric<Version>>());

        // Assert
        Assert.IsTrue(nonGenServicesList is [""]);
        Assert.IsTrue(gen1ServicesList is ["Object"]);
        Assert.IsTrue(gen2ServicesList is ["Int32"]);
        Assert.IsTrue(gen3ServicesList is ["Version"]);
    }

    [TestMethod]
    public async Task NoScope_SpecificTypes_ReturnsExpectedValues()
    {
        // Arrange
        await using var container = Container.Create(s =>
                                                     {
                                                         s.AddImplementation<NonGeneric>().For<INonGeneric>();
                                                         s.AddImplementation<Generic<object>>().For<IGeneric<object>>();
                                                         s.AddImplementation<Generic<int>>().For<IGeneric<int>>();
                                                     });

        // Act
        var nonGenServicesList = await GetList(container.GetServices<INonGeneric>());
        var gen1ServicesList = await GetList(container.GetServices<IGeneric<object>>());
        var gen2ServicesList = await GetList(container.GetServices<IGeneric<int>>());
        var gen3ServicesList = await GetList(container.GetServices<IGeneric<Version>>());

        // Assert
        Assert.IsTrue(nonGenServicesList is [""]);
        Assert.IsTrue(gen1ServicesList is ["Object"]);
        Assert.IsTrue(gen2ServicesList is ["Int32"]);
        Assert.IsTrue(gen3ServicesList is []);
    }

    [TestMethod]
    public async Task Scoped_AnyType_ReturnsExpectedValues()
    {
        // Arrange
        await using var parentContainer = Container.Create(s =>
                                                           {
                                                               s.AddImplementation<NonGeneric>().For<INonGeneric>();
                                                               s.AddImplementation<Generic<Any>>().For<IGeneric<Any>>();
                                                           });

        var serviceScopeFactory = await parentContainer.GetRequiredService<IServiceScopeFactory>();
        var container = serviceScopeFactory.CreateScope().ServiceProvider;

        // Act
        var nonGenServicesList = await GetList(container.GetServices<INonGeneric>());
        var gen1ServicesList = await GetList(container.GetServices<IGeneric<object>>());
        var gen2ServicesList = await GetList(container.GetServices<IGeneric<int>>());
        var gen3ServicesList = await GetList(container.GetServices<IGeneric<Version>>());

        // Assert
        Assert.IsTrue(nonGenServicesList is [""]);
        Assert.IsTrue(gen1ServicesList is ["Object"]);
        Assert.IsTrue(gen2ServicesList is ["Int32"]);
        Assert.IsTrue(gen3ServicesList is ["Version"]);
    }

    [TestMethod]
    public async Task Scoped_SpecificTypes_ReturnsExpectedValues()
    {
        // Arrange
        await using var parentContainer = Container.Create(s =>
                                                           {
                                                               s.AddImplementation<NonGeneric>().For<INonGeneric>();
                                                               s.AddImplementation<Generic<object>>().For<IGeneric<object>>();
                                                               s.AddImplementation<Generic<int>>().For<IGeneric<int>>();
                                                           });

        var serviceScopeFactory = await parentContainer.GetRequiredService<IServiceScopeFactory>();
        var container = serviceScopeFactory.CreateScope().ServiceProvider;

        // Act
        var nonGenServicesList = await GetList(container.GetServices<INonGeneric>());
        var gen1ServicesList = await GetList(container.GetServices<IGeneric<object>>());
        var gen2ServicesList = await GetList(container.GetServices<IGeneric<int>>());
        var gen3ServicesList = await GetList(container.GetServices<IGeneric<Version>>());

        // Assert
        Assert.IsTrue(nonGenServicesList is [""]);
        Assert.IsTrue(gen1ServicesList is ["Object"]);
        Assert.IsTrue(gen2ServicesList is ["Int32"]);
        Assert.IsTrue(gen3ServicesList is []);
    }

    [TestMethod]
    public async Task Scoped_AnyTypeInScope_ReturnsExpectedValues()
    {
        // Arrange
        await using var parentContainer = Container.Create(s =>
                                                           {
                                                               s.AddImplementation<NonGeneric>().For<INonGeneric>();
                                                               s.AddImplementation<Generic<Any>>().For<IGeneric<Any>>();
                                                           });

        var serviceScopeFactory = await parentContainer.GetRequiredService<IServiceScopeFactory>();
        var container = serviceScopeFactory.CreateScope(s => { s.AddImplementation<Generic<Any>>().For<IGeneric<Any>>(); })
                                           .ServiceProvider;

        // Act
        var nonGenServicesList = await GetList(container.GetServices<INonGeneric>());
        var gen1ServicesList = await GetList(container.GetServices<IGeneric<object>>());
        var gen2ServicesList = await GetList(container.GetServices<IGeneric<int>>());
        var gen3ServicesList = await GetList(container.GetServices<IGeneric<Version>>());

        // Assert
        Assert.IsTrue(nonGenServicesList is [""]);
        Assert.IsTrue(gen1ServicesList is ["Object", "Object"]);
        Assert.IsTrue(gen2ServicesList is ["Int32", "Int32"]);
        Assert.IsTrue(gen3ServicesList is ["Version", "Version"]);
    }

    [TestMethod]
    public async Task Scoped_SpecificTypesInScope_ReturnsExpectedValues()
    {
        // Arrange
        await using var parentContainer = Container.Create(s =>
                                                           {
                                                               s.AddImplementation<NonGeneric>().For<INonGeneric>();
                                                               s.AddImplementation<Generic<object>>().For<IGeneric<object>>();
                                                               s.AddImplementation<Generic<int>>().For<IGeneric<int>>();
                                                           });

        var serviceScopeFactory = await parentContainer.GetRequiredService<IServiceScopeFactory>();
        var container = serviceScopeFactory.CreateScope(s => { s.AddImplementation<Generic<Any>>().For<IGeneric<Any>>(); })
                                           .ServiceProvider;

        // Act
        var nonGenServicesList = await GetList(container.GetServices<INonGeneric>());
        var gen1ServicesList = await GetList(container.GetServices<IGeneric<object>>());
        var gen2ServicesList = await GetList(container.GetServices<IGeneric<int>>());
        var gen3ServicesList = await GetList(container.GetServices<IGeneric<Version>>());

        // Assert
        Assert.IsTrue(nonGenServicesList is [""]);
        Assert.IsTrue(gen1ServicesList is ["Object", "Object"]);
        Assert.IsTrue(gen2ServicesList is ["Int32", "Int32"]);
        Assert.IsTrue(gen3ServicesList is ["Version"]);
    }

    [TestMethod]
    public async Task Scoped_AnyTypeWithSpecificTypesInScope_ReturnsExpectedValues()
    {
        // Arrange
        await using var parentContainer = Container.Create(s =>
                                                           {
                                                               s.AddImplementation<NonGeneric>().For<INonGeneric>();
                                                               s.AddImplementation<Generic<Any>>().For<IGeneric<Any>>();
                                                           });

        var serviceScopeFactory = await parentContainer.GetRequiredService<IServiceScopeFactory>();
        var container = serviceScopeFactory.CreateScope(s =>
                                                        {
                                                            s.AddImplementation<Generic<object>>().For<IGeneric<object>>();
                                                            s.AddImplementation<Generic<int>>().For<IGeneric<int>>();
                                                        })
                                           .ServiceProvider;

        // Act
        var nonGenServicesList = await GetList(container.GetServices<INonGeneric>());
        var gen1ServicesList = await GetList(container.GetServices<IGeneric<object>>());
        var gen2ServicesList = await GetList(container.GetServices<IGeneric<int>>());
        var gen3ServicesList = await GetList(container.GetServices<IGeneric<Version>>());

        // Assert
        Assert.IsTrue(nonGenServicesList is [""]);
        Assert.IsTrue(gen1ServicesList is ["Object", "Object"]);
        Assert.IsTrue(gen2ServicesList is ["Int32", "Int32"]);
        Assert.IsTrue(gen3ServicesList is ["Version"]);
    }

    private interface ISomeValue
    {
        string Value { get; }
    }

    private interface INonGeneric;

    private interface IGeneric<[UsedImplicitly] T>;

    [UsedImplicitly]
    private class NonGeneric : INonGeneric, ISomeValue
    {
    #region Interface ISomeValue

        public string Value => "";

    #endregion
    }

    [UsedImplicitly]
    private class Generic<T> : IGeneric<T>, ISomeValue
    {
    #region Interface ISomeValue

        public string Value => typeof(T).Name;

    #endregion
    }
}