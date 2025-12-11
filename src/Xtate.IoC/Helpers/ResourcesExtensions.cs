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

using System.Globalization;

namespace Xtate.IoC;

/// <summary>
///     Provides methods for formatting resource strings using the specified culture and strongly-typed arguments.
/// </summary>
internal static class ResourcesExtensions
{
	/// <summary>
	///     Formats a string using the specified format and a single argument.
	/// </summary>
	/// <param name="format">The format string.</param>
	/// <param name="arg">The argument to format.</param>
	/// <returns>A formatted string.</returns>
	private static string Format(string format, object? arg) => string.Format(Resources.Culture ?? CultureInfo.CurrentUICulture, format, arg);

	/// <summary>
	///     Formats a string using the specified format and two arguments.
	/// </summary>
	/// <param name="format">The format string.</param>
	/// <param name="arg0">The first argument to format.</param>
	/// <param name="arg1">The second argument to format.</param>
	/// <returns>A formatted string.</returns>
	private static string Format(string format, object? arg0, object? arg1) => string.Format(Resources.Culture ?? CultureInfo.CurrentUICulture, format, arg0, arg1);

	extension(Resources)
	{
		/// <summary>
		///     Creates an error message indicating that an assertion did not match the expected value for a specified
		///     <paramref name="type" />.
		/// </summary>
		/// <param name="type">The type being asserted.</param>
		/// <param name="value">The actual value that failed the assertion.</param>
		/// <returns>A localized error message.</returns>
		public static string Exception_AssertUnmatched(Type type, object? value) => Format(Resources.Exception_AssertUnmatched, type.FriendlyName, value);

		/// <summary>
		///     Creates an error message indicating that a required service of the specified <paramref name="type" /> is missing in
		///     the container.
		/// </summary>
		/// <param name="type">The service type that was not found.</param>
		/// <returns>A localized error message.</returns>
		public static string Exception_ServiceMissedInContainer(Type type) => Format(Resources.Exception_ServiceMissedInContainer, type.FriendlyName);

		/// <summary>
		///     Creates an error message indicating that a required argument of <paramref name="argType" /> for service
		///     <paramref name="type" /> is missing in the container.
		/// </summary>
		/// <param name="type">The service type that requires the argument.</param>
		/// <param name="argType">The type of the missing argument.</param>
		/// <returns>A localized error message.</returns>
		public static string Exception_ServiceArgMissedInContainer(Type type, ArgumentType argType) => Format(Resources.Exception_ServiceMissedInContainer, type.FriendlyName, argType);

		/// <summary>
		///     Creates an error message indicating that a value of type <paramref name="fromType" /> cannot be cast to
		///     <paramref name="toType" />.
		/// </summary>
		/// <param name="fromType">The source type.</param>
		/// <param name="toType">The target type.</param>
		/// <returns>A localized error message.</returns>
		public static string Exception_TypeCantBeCastedTo(Type fromType, Type toType) => Format(Resources.Exception_TypeCantBeCastedTo, fromType.FriendlyName, toType.FriendlyName);

		/// <summary>
		///     Creates an error message indicating that a type <paramref name="fromType" /> cannot be constructed based on the
		///     service type <paramref name="toType" />.
		/// </summary>
		/// <param name="fromType">The type to construct.</param>
		/// <param name="toType">The service type used as a base.</param>
		/// <returns>A localized error message.</returns>
		public static string Exception_TypeCantBeConstructedBasedOnServiceType(Type fromType, Type toType) =>
			Format(Resources.Exception_TypeCantBeConstructedBasedOnServiceType, fromType.FriendlyName, toType.FriendlyName);

		/// <summary>
		///     Creates an error message indicating that a factory for the specified <paramref name="type" /> could not be found.
		/// </summary>
		/// <param name="type">The type for which the factory is required.</param>
		/// <returns>A localized error message.</returns>
		public static string Exception_CantFindFactoryForType(Type type) => Format(Resources.Exception_CantFindFactoryForType, type.FriendlyName);

		/// <summary>
		///     Creates an error message indicating that the factory for the specified <paramref name="type" /> raised an
		///     exception.
		/// </summary>
		/// <param name="type">The type whose factory caused the exception.</param>
		/// <returns>A localized error message.</returns>
		public static string Exception_FactoryOfRaisedException(Type type) => Format(Resources.Exception_FactoryOfRaisedException, type.FriendlyName);

		/// <summary>
		///     Creates an error message indicating that the specified <paramref name="type" /> is invalid in the current context.
		/// </summary>
		/// <param name="type">The invalid type.</param>
		/// <returns>A localized error message.</returns>
		public static string Exception_InvalidType(Type type) => Format(Resources.Exception_InvalidType, type.FriendlyName);

		/// <summary>
		///     Creates an error message indicating that the specified <paramref name="type" /> was used in a synchronous
		///     instantiation path where it is not allowed.
		/// </summary>
		/// <param name="type">The type involved in the synchronous instantiation.</param>
		/// <returns>A localized error message.</returns>
		public static string Exception_TypeUsedInSynchronousInstantiation(Type type) => Format(Resources.Exception_TypeUsedInSynchronousInstantiation, type.FriendlyName);

		/// <summary>
		///     Creates an error message indicating that the specified service <paramref name="type" /> is not available in a
		///     synchronous context.
		/// </summary>
		/// <param name="type">The service type that is unavailable.</param>
		/// <returns>A localized error message.</returns>
		public static string Exception_ServiceNotAvailableInSynchronousContext(Type type) => Format(Resources.Exception_ServiceNotAvailableInSynchronousContext, type.FriendlyName);

		/// <summary>
		///     Creates an error message indicating that a forward delegate from <paramref name="fromType" /> cannot be cast to
		///     <paramref name="toType" />.
		/// </summary>
		/// <param name="fromType">The source delegate type.</param>
		/// <param name="toType">The target delegate type.</param>
		/// <returns>A localized error message.</returns>
		public static string Exception_CantCastForwardDelegate(Type fromType, Type toType) => Format(Resources.Exception_CantCastForwardDelegate, fromType.FriendlyName, toType.FriendlyName);

		/// <summary>
		///     Creates an error message indicating that the specified <paramref name="option" /> is not supported.
		/// </summary>
		/// <param name="option">The unsupported option.</param>
		/// <returns>A localized error message.</returns>
		public static string Exception_OptionDoesNotSupport(Option option) => Format(Resources.Exception_OptionDoesNotSupport, option);

		/// <summary>
		///     Creates an error message indicating that the type <paramref name="type" /> does not contain a synchronous method
		///     with the signature <c>Method(CancellationToken)</c> returning <paramref name="returnType" />.
		/// </summary>
		/// <param name="type">The type to inspect.</param>
		/// <param name="returnType">The expected method return type.</param>
		/// <returns>A localized error message.</returns>
		public static string Exception_TypeDoesNotContainSyncMethodWithSignatureMethodCancellationToken(Type type, Type returnType) =>
			Format(Resources.Exception_TypeDoesNotContainSyncMethodWithSignatureMethodCancellationToken, type.FriendlyName, returnType.FriendlyName);

		/// <summary>
		///     Creates an error message indicating that the type <paramref name="type" /> does not contain an asynchronous method
		///     with the signature <c>Method(CancellationToken)</c> returning <paramref name="returnType" />.
		/// </summary>
		/// <param name="type">The type to inspect.</param>
		/// <param name="returnType">The expected method return type.</param>
		/// <returns>A localized error message.</returns>
		public static string Exception_TypeDoesNotContainAsyncMethodWithSignatureMethodCancellationToken(Type type, Type returnType) =>
			Format(Resources.Exception_TypeDoesNotContainAsyncMethodWithSignatureMethodCancellationToken, type.FriendlyName, returnType.FriendlyName);

		/// <summary>
		///     Formats a service descriptor with no arguments.
		/// </summary>
		/// <param name="serviceType">The service type.</param>
		/// <returns>A localized formatted string.</returns>
		public static string Format_ServiceNoArgs(ServiceType serviceType) => Format(Resources.Format_ServiceNoArgs, serviceType);

		/// <summary>
		///     Formats a service descriptor with a specific <paramref name="argumentType" />.
		/// </summary>
		/// <param name="serviceType">The service type.</param>
		/// <param name="argumentType">The argument type used in formatting.</param>
		/// <returns>A localized formatted string.</returns>
		public static string Format_ServiceWithArgs(ServiceType serviceType, ArgumentType argumentType) => Format(Resources.Format_ServiceWithArgs, serviceType, argumentType);

		/// <summary>
		///     Formats an implementation descriptor with no arguments.
		/// </summary>
		/// <param name="implementationType">The implementation type.</param>
		/// <returns>A localized formatted string.</returns>
		public static string Format_ImplementationNoArgs(ImplementationType implementationType) => Format(Resources.Format_ImplementationNoArgs, implementationType);

		/// <summary>
		///     Formats an implementation descriptor with a specific <paramref name="argumentType" />.
		/// </summary>
		/// <param name="implementationType">The implementation type.</param>
		/// <param name="argumentType">The argument type used in formatting.</param>
		/// <returns>A localized formatted string.</returns>
		public static string Format_ImplementationWithArgs(ImplementationType implementationType, ArgumentType argumentType) =>
			Format(Resources.Format_ImplementationWithArgs, implementationType, argumentType);
	}
}