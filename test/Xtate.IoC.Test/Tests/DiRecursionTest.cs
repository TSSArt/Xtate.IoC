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

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global

namespace Xtate.IoC.Test;

[TestClass]
public class DiRecursionTest
{
    [TestMethod]
    public async Task Recursion_ShouldThrowDependencyInjectionException_WhenCircularDependencyDetected()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddType<R1>();
        serviceCollection.AddType<R2>();
        serviceCollection.AddImplementationSync<RecursionDetector>().For<IServiceProviderActions>();
        var serviceProvider = serviceCollection.BuildProvider();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<DependencyInjectionException>(async () => await serviceProvider.GetRequiredService<R1>());
    }

    private class RecursionDetector : IServiceProviderActions
    {
        private int _level;

    #region Interface IServiceProviderActions

        public IServiceProviderDataActions? RegisterServices() => null;

        public IServiceProviderDataActions? ServiceRequesting(TypeKey typeKey) => null;

        [ExcludeFromCodeCoverage]
        public IServiceProviderDataActions ServiceRequested(TypeKey typeKey) => throw new NotSupportedException(typeKey?.ToString());

        public IServiceProviderDataActions? FactoryCalling(TypeKey typeKey)
        {
            if (_level ++ > 20)
            {
                throw new DependencyInjectionException();
            }

            return null;
        }

        [ExcludeFromCodeCoverage]
        public IServiceProviderDataActions? FactoryCalled(TypeKey typeKey)
        {
            _level --;

            return null;
        }

    #endregion
    }

    [ExcludeFromCodeCoverage]
    public class R1(R2 r2)
    {
        public R2 Unknown { get; } = r2;
    }

    [ExcludeFromCodeCoverage]
    public class R2(R1 r1)
    {
        public R1 Unknown { get; } = r1;
    }
}