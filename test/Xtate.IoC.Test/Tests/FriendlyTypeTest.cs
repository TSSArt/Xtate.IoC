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

[TestClass]
public class FriendlyTypeTest
{
	[TestMethod]
	[DataRow("int", typeof(int))]
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
	[DataRow("(int,long)", typeof((int, long)))]
	[DataRow("(int,int,int)", typeof((int, int, int)))]
	[DataRow("(int,int,int,int)", typeof((int, int, int, int)))]
	[DataRow("(int,int,int,int,int)", typeof((int, int, int, int, int)))]
	[DataRow("(int,int,int,int,int,int)", typeof((int, int, int, int, int, int)))]
	[DataRow("(int,int,int,int,int,int,int)", typeof((int, int, int, int, int, int, int)))]
	[DataRow("(int,int,int,int,int,int,int,int)", typeof((int, int, int, int, int, int, int, int)))]
	[DataRow("(int,int,int,int,int,int,int,int,int)", typeof((int, int, int, int, int, int, int, int, int)))]
	[DataRow("(int,int,int,int,int,int,int,int,int,int)", typeof((int, int, int, int, int, int, int, int, int, int)))]
	public void ClassNameTest(string name, Type type)
	{
		Assert.AreEqual(name, type.FriendlyName());
	}
}