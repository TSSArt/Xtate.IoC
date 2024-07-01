﻿// Copyright © 2019-2023 Sergii Artemenko
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

#if !NET8_0

<<<<<<< Updated upstream:src/Xtate.Core/src/Xtate.Core/DataModel/Abstractions/Interfaces/IDataModelHandlerFactory.cs
using System.Threading;
using System.Threading.Tasks;
=======
namespace Xtate.Core;
>>>>>>> Stashed changes:src/Xtate.Core/src/Xtate.Core/Helpers/CancellationTokenSourceExtensions.cs

public static class CancellationTokenSourceExtensions
{
<<<<<<< Updated upstream:src/Xtate.Core/src/Xtate.Core/DataModel/Abstractions/Interfaces/IDataModelHandlerFactory.cs
	//TODO:delete
	public interface IDataModelHandlerFactory1
	{
		ValueTask<IDataModelHandlerFactoryActivator?> TryGetActivator(ServiceLocator serviceLocator, string dataModelType, CancellationToken token);
	}
}
=======
	public static Task CancelAsync(this CancellationTokenSource cancellationTokenSource) =>
		!cancellationTokenSource.IsCancellationRequested
			? Task.Run(cancellationTokenSource.Cancel)
			: Task.CompletedTask;
}

#endif
>>>>>>> Stashed changes:src/Xtate.Core/src/Xtate.Core/Helpers/CancellationTokenSourceExtensions.cs