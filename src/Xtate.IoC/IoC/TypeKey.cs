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

internal interface ITypeKeyAction
{
	void TypedAction<T, TArg>(TypeKey typeKey);
}

internal abstract class SimpleTypeKey : TypeKey;

internal abstract class GenericTypeKey : TypeKey
{
	public abstract SimpleTypeKey DefinitionKey { get; }
}

public abstract class TypeKey
{
	public static TypeKey ServiceKey<T, TArg>() => Infra.TypeInitHandle(() => Service<T, TArg>.Value);

	public static TypeKey ImplementationKey<T, TArg>() => Infra.TypeInitHandle(() => Implementation<T, TArg>.Value);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static TypeKey ServiceKeyFast<T, TArg>() => Service<T, TArg>.Value;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static TypeKey ImplementationKeyFast<T, TArg>() => Implementation<T, TArg>.Value;

	[ExcludeFromCodeCoverage]
	internal virtual void DoTypedAction(ITypeKeyAction typeKeyAction) => Infra.Assert(false);

	public virtual bool IsEmptyArg => true;

	private static class Service<T, TArg>
	{
		private static readonly string ToStringValue = ArgumentType.TypeOf<TArg>().IsEmpty
			? @$"SRV:{ServiceType.TypeOf<T>()}"
			: @$"SRV:{ServiceType.TypeOf<T>()}({ArgumentType.TypeOf<TArg>()})";

		public static readonly TypeKey Value = ServiceType.TypeOf<T>().IsGeneric ? new Generic() : new Simple();

		private class Simple : SimpleTypeKey
		{
			internal override void DoTypedAction(ITypeKeyAction typeKeyAction) => typeKeyAction.TypedAction<T, TArg>(this);

			public override string ToString() => ToStringValue;

			public override bool IsEmptyArg => ArgumentType.TypeOf<TArg>().IsEmpty;
		}

		private class Generic : GenericTypeKey
		{
			public override SimpleTypeKey DefinitionKey { get; } = new DefinitionServiceTypeKey(ServiceType.TypeOf<T>().Definition);

			internal override void DoTypedAction(ITypeKeyAction typeKeyAction) => typeKeyAction.TypedAction<T, TArg>(this);

			public override string ToString() => ToStringValue;

			public override bool IsEmptyArg => ArgumentType.TypeOf<TArg>().IsEmpty;
		}
	}

	private static class Implementation<T, TArg>
	{
		private static readonly string ToStringValue = ArgumentType.TypeOf<TArg>().IsEmpty
			? @$"IMP:{ImplementationType.TypeOf<T>()}"
			: @$"IMP:{ImplementationType.TypeOf<T>()}({ArgumentType.TypeOf<TArg>()})";

		public static readonly TypeKey Value = ImplementationType.TypeOf<T>().IsGeneric ? new Generic() : new Simple();

		private class Simple : SimpleTypeKey
		{
			internal override void DoTypedAction(ITypeKeyAction typeKeyAction) => typeKeyAction.TypedAction<T, TArg>(this);

			public override string ToString() => ToStringValue;

			public override bool IsEmptyArg => ArgumentType.TypeOf<TArg>().IsEmpty;
		}

		private class Generic : GenericTypeKey
		{
			public override SimpleTypeKey DefinitionKey { get; } = new DefinitionImplementationTypeKey(ImplementationType.TypeOf<T>().Definition);

			internal override void DoTypedAction(ITypeKeyAction typeKeyAction) => typeKeyAction.TypedAction<T, TArg>(this);

			public override string ToString() => ToStringValue;

			public override bool IsEmptyArg => ArgumentType.TypeOf<TArg>().IsEmpty;
		}
	}

	private class DefinitionServiceTypeKey(in ServiceType openGeneric) : SimpleTypeKey
	{
		private readonly ServiceType _openGeneric = openGeneric;

		public override int GetHashCode() => _openGeneric.GetHashCode();

		public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is DefinitionServiceTypeKey other && _openGeneric.Equals(other._openGeneric));

		public override string ToString() => @$"SRV:{_openGeneric}";
	}

	private class DefinitionImplementationTypeKey(in ImplementationType openGeneric) : SimpleTypeKey
	{
		private readonly ImplementationType _openGeneric = openGeneric;

		public override int GetHashCode() => _openGeneric.GetHashCode();

		public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is DefinitionImplementationTypeKey other && _openGeneric.Equals(other._openGeneric));

		public override string ToString() => @$"IMP:{_openGeneric}";
	}
}