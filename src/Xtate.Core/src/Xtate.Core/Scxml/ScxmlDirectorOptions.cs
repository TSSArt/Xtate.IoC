﻿#region Copyright © 2019-2021 Sergii Artemenko

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

#endregion

using System;
using System.Xml;
using Xtate.Core;

namespace Xtate.Scxml
{
	[PublicAPI]
	public record ScxmlDirectorOptions
	{
		private int _maxNestingLevel;

		public ScxmlDirectorOptions(ServiceLocator serviceLocator)
		{
			ServiceLocator = serviceLocator;
			ErrorProcessorService = serviceLocator.GetService<IErrorProcessorService<ScxmlDirector>>();
			StateMachineValidator = serviceLocator.GetService<IStateMachineValidator>();
		}

		public ServiceLocator ServiceLocator { get; init; }

		public IErrorProcessorService1  ErrorProcessorService { get; init; }
		public IXmlNamespaceResolver?  NamespaceResolver     { get; init; }
		public XmlReaderSettings?      XmlReaderSettings     { get; init; }
		public ScxmlXmlResolver?       XmlResolver           { get; init; }
		public IStateMachineValidator? StateMachineValidator { get; init; }
		public bool                    LineInfoRequired   { get; init; }
		public bool                    Async                 { get; init; }
		public bool                    XIncludeAllowed       { get; init; }

		public int MaxNestingLevel
		{
			get => _maxNestingLevel;
			init
			{
				if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), Resources.Exception_ValueMustBeNonNegativeInteger);

				_maxNestingLevel = value;
			}
		}
	}
}