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

public struct ActionsContext(TypeKey typeKey, Type serviceType, Type argumentType)
{
	public TypeKey TypeKey { get; } = typeKey;

	public Type ServiceType { get; } = serviceType;

	public Type ArgumentType { get; } = argumentType;

	public Exception? Exception { get; internal set; }

	public int UserDataInt32 { get; set; }

	public object? UserDataObject { get; set; }

	public static ActionsContext Create<T, TArg>() => new(TypeKey.ServiceKeyFast<T, TArg>(), typeof(T), typeof(TArg));
}