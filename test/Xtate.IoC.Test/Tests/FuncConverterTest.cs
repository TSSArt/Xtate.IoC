﻿// Copyright © 2019-2024 Sergii Artemenko
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
public class FuncConverterTest
{
	[TestMethod]
	public void CastWrongType1Test()
	{
		Assert.ThrowsException<InvalidCastException>([ExcludeFromCodeCoverage]() => { FuncConverter.Cast<EventHandler>(new Func<ValueTuple, bool>(MyFunc)); });
		return;

		[ExcludeFromCodeCoverage]
		static bool MyFunc(ValueTuple _) => false;
	}

	[TestMethod]
	public void CastWrongType2Test()
	{
		Assert.ThrowsException<InvalidCastException>([ExcludeFromCodeCoverage]() => { FuncConverter.Cast<Predicate<string>>(new Func<ValueTuple, bool>(MyFunc)); });
		return;

		[ExcludeFromCodeCoverage]
		static bool MyFunc(ValueTuple _) => false;
	}

	[TestMethod]
	public void Cast1Test()
	{
		var f = FuncConverter.Cast<Func<string>>(new Func<ValueTuple, string>(MyFunc));

		Assert.AreEqual(expected: "test", f());
		return;

		static string MyFunc(ValueTuple _) => "test";
	}

	[TestMethod]
	public void Cast2Test()
	{
		var f = FuncConverter.Cast<Func<string, string>>(new Func<string, string>(MyFunc));

		Assert.AreEqual(expected: "test", f("test"));
		return;

		static string MyFunc(string v) => v;
	}

	[TestMethod]
	public void Cast2ATest()
	{
		Assert.ThrowsException<ArgumentException>([ExcludeFromCodeCoverage]() => FuncConverter.Cast<Func<object, string>>(new Func<string, string>(MyFunc)));
		return;

		[ExcludeFromCodeCoverage]
		static string MyFunc(string v) => v;
	}

	[TestMethod]
	public void Cast3Test()
	{
		var f = FuncConverter.Cast<Func<string, string, string>>(new Func<(string, string), string>(MyFunc));

		Assert.AreEqual(expected: "ab", f(arg1: "a", arg2: "b"));
		return;

		static string MyFunc((string v1, string v2) arg) => arg.v1 + arg.v2;
	}

	[TestMethod]
	public void Cast1_9Test()
	{
		var f = FuncConverter.Cast<Func<string, string, string, string, string, string, string, string, string, string>>(
			new Func<(string, string, string, string, string, string, string, string, string), string>(MyFunc));

		Assert.AreEqual(expected: "123456789", f(arg1: "1", arg2: "2", arg3: "3", arg4: "4", arg5: "5", arg6: "6", arg7: "7", arg8: "8", arg9: "9"));
		return;

		static string MyFunc((string v1, string v2, string v3, string v4, string v5, string v6, string v7, string v8, string v9) arg) =>
			arg.v1 + arg.v2 + arg.v3 + arg.v4 + arg.v5 + arg.v6 + arg.v7 + arg.v8 + arg.v9;
	}
}