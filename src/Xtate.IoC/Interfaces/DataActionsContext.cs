// Copyright © 2019-2026 Sergii Artemenko
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

namespace Xtate.IoC;

public struct DataActionsContext<T, TArg>
{
	internal ActionsContext ActionsContext;

	internal DataActionsContext(TArg argument)
	{
		ActionsContext = new ActionsContext(TypeKey.ServiceKeyFast<T, TArg>(), typeof(T), typeof(TArg));
		Argument = argument;
	}

	public TypeKey TypeKey => ActionsContext.TypeKey;

	public TArg Argument { get; }

	public T? Instance { get; internal set; }

	public Exception? Exception
	{
		get => ActionsContext.Exception;
		internal set => ActionsContext.Exception = value;
	}

	public int UserDataInt32
	{
		get => ActionsContext.UserDataInt32;
		set => ActionsContext.UserDataInt32 = value;
	}

	public object? UserDataObject
	{
		get => ActionsContext.UserDataObject;
		set => ActionsContext.UserDataObject = value;
	}

	internal class Container
	{
		public DataActionsContext<T, TArg> Context;
	}
}