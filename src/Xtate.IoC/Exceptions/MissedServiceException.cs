// Copyright © 2019-2024 Sergii Artemenko
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

[Serializable]
public class MissedServiceException : DependencyInjectionException
{
	public MissedServiceException() { }

	public MissedServiceException(string? message) : base(message) { }

	public MissedServiceException(string? message, Exception? innerException) : base(message, innerException) { }

	public required Type Service { get; init; }

	public Type? Argument { get; init; }

	public static MissedServiceException Create<T>() => new(Res.Format(Resources.Exception_ServiceMissedInContainer, typeof(T))) { Service = typeof(T) };

	public static MissedServiceException Create<T, TArg>()
	{
		var argumentType = ArgumentType.TypeOf<TArg>();
		var isEmptyArg = argumentType.IsEmpty;

		var message = isEmptyArg
			? Res.Format(Resources.Exception_ServiceMissedInContainer, typeof(T))
			: Res.Format(Resources.Exception_ServiceArgMissedInContainer, typeof(T), argumentType);

		return new MissedServiceException(message) { Service = typeof(T), Argument = isEmptyArg ? null : typeof(TArg) };
	}
}