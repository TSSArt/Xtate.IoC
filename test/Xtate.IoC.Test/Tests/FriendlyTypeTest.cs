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

namespace Xtate.IoC.Test;

public enum SomeEnumName;

public class NonGeneric
{
	public enum NestedEnum;

	public class NestedNonGeneric;

	public class NestedGeneric<[UsedImplicitly] TN>;
}

public class Generic<[UsedImplicitly] T>
{
	public enum NestedEnum;

	public class NestedNonGeneric;

	public class NestedGeneric<[UsedImplicitly] TN>;
}

[TestClass]
public class FriendlyTypeTest
{
	[TestMethod]
	[DataRow("bool", typeof(bool))]
	[DataRow("byte", typeof(byte))]
	[DataRow("char", typeof(char))]
	[DataRow("decimal", typeof(decimal))]
	[DataRow("double", typeof(double))]
	[DataRow("short", typeof(short))]
	[DataRow("int", typeof(int))]
	[DataRow("long", typeof(long))]
	[DataRow("sbyte", typeof(sbyte))]
	[DataRow("float", typeof(float))]
	[DataRow("string", typeof(string))]
	[DataRow("ushort", typeof(ushort))]
	[DataRow("uint", typeof(uint))]
	[DataRow("ulong", typeof(ulong))]
	[DataRow("object", typeof(object))]
	[DataRow("void", typeof(void))]
	[DataRow("List<int>", typeof(List<int>))]
	[DataRow("(int, long)", typeof((int, long)))]
	[DataRow("(int, int, int)", typeof((int, int, int)))]
	[DataRow("(int, int, int, int)", typeof((int, int, int, int)))]
	[DataRow("(int, int, int, int, int)", typeof((int, int, int, int, int)))]
	[DataRow("(int, int, int, int, int, int)", typeof((int, int, int, int, int, int)))]
	[DataRow("(int, int, int, int, int, int, int)", typeof((int, int, int, int, int, int, int)))]
	[DataRow("(int, int, int, int, int, int, int, int)", typeof((int, int, int, int, int, int, int, int)))]
	[DataRow("(int, int, int, int, int, int, int, int, int)", typeof((int, int, int, int, int, int, int, int, int)))]
	[DataRow("(int, int, int, int, int, int, int, int, int, int)", typeof((int, int, int, int, int, int, int, int, int, int)))]
	[DataRow("SomeEnumName", typeof(SomeEnumName))]
	[DataRow("NonGeneric", typeof(NonGeneric))]
	[DataRow("Generic<int>", typeof(Generic<int>))]
	[DataRow("Generic<Generic<int>>", typeof(Generic<Generic<int>>))]
	[DataRow("NonGeneric.NestedNonGeneric", typeof(NonGeneric.NestedNonGeneric))]
	[DataRow("NonGeneric.NestedGeneric<int>", typeof(NonGeneric.NestedGeneric<int>))]
	[DataRow("Generic<long>.NestedNonGeneric", typeof(Generic<long>.NestedNonGeneric))]
	[DataRow("Generic<long>.NestedGeneric<object>", typeof(Generic<long>.NestedGeneric<object>))]
	[DataRow("Generic<long>.NestedEnum", typeof(Generic<long>.NestedEnum))]
	[DataRow("NonGeneric.NestedEnum", typeof(NonGeneric.NestedEnum))]
	public void FriendlyName_ShouldReturnExpectedName_WhenGivenType(string expectedName, Type type)
	{
		// Arrange & Act
		var actualName = type.FriendlyName();

		// Assert
		Assert.AreEqual(expectedName, actualName);
	}
}