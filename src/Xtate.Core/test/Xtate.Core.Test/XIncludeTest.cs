﻿#region Copyright © 2019-2023 Sergii Artemenko

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

<<<<<<< Updated upstream
using System;
=======
>>>>>>> Stashed changes
using System.Text;
using System.Xml;
<<<<<<< Updated upstream
using Microsoft.VisualStudio.TestTools.UnitTesting;
=======
>>>>>>> Stashed changes
using Xtate.IoC;
using Xtate.Scxml;
using Xtate.XInclude;
using IServiceProvider = Xtate.IoC.IServiceProvider;

namespace Xtate.Core.Test;

[TestClass]
public class XIncludeTest
{
	[TestMethod]
	public async Task CreateStateMachineWithXInclude()
	{
		var services = new ServiceCollection();
		services.RegisterStateMachineHost();
		services.AddImplementationSync<XIncludeOptions>().For<IXIncludeOptions>();
		var serviceProvider = services.BuildProvider();
		var host = await serviceProvider.GetRequiredService<StateMachineHost>();

		await host.StartHostAsync();

		_ = await host.ExecuteStateMachineAsync(new Uri("res://Xtate.Core.Test/Xtate.Core.Test/Scxml/XInclude/SingleIncludeSource.scxml"));

		await host.StopHostAsync();
	}

	[TestMethod]
	public async Task DtdReaderTest()
	{
		var uri = new Uri("res://Xtate.Core.Test/Xtate.Core.Test/Scxml/XInclude/DtdSingleIncludeSource.scxml");

		var services = new ServiceCollection();
		services.RegisterScxml();
		var serviceProvider = services.BuildProvider();

		var resourceLoaderService = await serviceProvider.GetRequiredService<IResourceLoader>();
		var resource = await resourceLoaderService.Request(uri);
		var resolver = await serviceProvider.GetRequiredService<XmlResolver>();

		var xmlReaderSettings = new XmlReaderSettings { Async = true, XmlResolver = resolver, DtdProcessing = DtdProcessing.Parse };
		var xmlReader = XmlReader.Create(await resource.GetStream(doNotCache: true), xmlReaderSettings, uri.ToString());

		var xIncludeReader = await serviceProvider.GetRequiredService<XIncludeReader, XmlReader>(xmlReader);

		var builder = new StringBuilder();
		var xmlWriter = XmlWriter.Create(builder);
		while (await xIncludeReader.ReadAsync())
		{
<<<<<<< Updated upstream
			var host2 = new StateMachineHostBuilder()
					   //TODO:
					   //.AddResourceLoaderFactory(ResxResourceLoaderFactory.Instance)
					   .Build(ServiceLocator.Default);

			var services = new ServiceCollection();
			services.RegisterStateMachineHost();
			services.AddImplementationSync<XIncludeOptions>().For<IXIncludeOptions>();
			var serviceProvider = services.BuildProvider();
			var host = await serviceProvider.GetRequiredService<StateMachineHost>();


			await host.StartHostAsync();

			var _ = await host.ExecuteStateMachineAsync(new Uri("res://Xtate.Core.Test/Xtate.Core.Test/Scxml/XInclude/SingleIncludeSource.scxml"));

			await host.StopHostAsync();
=======
			// ReSharper disable once MethodHasAsyncOverload
			xmlWriter.WriteNode(xmlReader, defattr: false);
>>>>>>> Stashed changes
		}

		xmlWriter.Close();

		Console.Write(builder.ToString());
	}

	[TestMethod]
	public async Task XIncludeReaderTest()
	{
		var uri = new Uri("res://Xtate.Core.Test/Xtate.Core.Test/Scxml/XInclude/SingleIncludeSource.scxml");

		var services = new ServiceCollection();
		services.RegisterScxml();
		var serviceProvider = services.BuildProvider();

		var resourceLoaderService = await serviceProvider.GetRequiredService<IResourceLoader>();
		var resource = await resourceLoaderService.Request(uri);
		var resolver = await serviceProvider.GetRequiredService<XmlResolver>();

		var xmlReaderSettings = new XmlReaderSettings { Async = true, XmlResolver = resolver };
		var xmlReader = XmlReader.Create(await resource.GetStream(doNotCache: true), xmlReaderSettings, uri.ToString());

		var xIncludeReader = await serviceProvider.GetRequiredService<XIncludeReader, XmlReader>(xmlReader);

		var builder = new StringBuilder();
		var xmlWriter = XmlWriter.Create(builder);
		while (await xIncludeReader.ReadAsync())
		{
<<<<<<< Updated upstream
			var uri = new Uri("res://Xtate.Core.Test/Xtate.Core.Test/Scxml/XInclude/DtdSingleIncludeSource.scxml");

			var services = new ServiceCollection();
			services.RegisterScxml();
			var serviceProvider = services.BuildProvider();

			var resourceLoaderService = await serviceProvider.GetRequiredService<IResourceLoader>();
			var resource = await resourceLoaderService.Request(uri);
			var resolver = await serviceProvider.GetRequiredService<XmlResolver>();

			var xmlReaderSettings = new XmlReaderSettings { Async = true, XmlResolver = resolver, DtdProcessing = DtdProcessing.Parse };
			var xmlReader = XmlReader.Create(await resource.GetStream(doNotCache: true), xmlReaderSettings, uri.ToString());

			var xIncludeReader = await serviceProvider.GetRequiredService<XIncludeReader, XmlReader>(xmlReader);

			var builder = new StringBuilder();
			var xmlWriter = XmlWriter.Create(builder);
			while (await xIncludeReader.ReadAsync())
			{
				// ReSharper disable once MethodHasAsyncOverload
				xmlWriter.WriteNode(xmlReader, defattr: false);
			}

			xmlWriter.Close();

			Console.Write(builder.ToString());
=======
			// ReSharper disable once MethodHasAsyncOverload
			xmlWriter.WriteNode(xIncludeReader, defattr: false);
>>>>>>> Stashed changes
		}

		xmlWriter.Close();

<<<<<<< Updated upstream
			var services = new ServiceCollection();
			services.RegisterScxml();
			var serviceProvider = services.BuildProvider();

			var resourceLoaderService = await serviceProvider.GetRequiredService<IResourceLoader>();
			var resource = await resourceLoaderService.Request(uri);
			var resolver = await serviceProvider.GetRequiredService<XmlResolver>();

			var xmlReaderSettings = new XmlReaderSettings { Async = true, XmlResolver = resolver };
			var xmlReader = XmlReader.Create(await resource.GetStream(doNotCache: true), xmlReaderSettings, uri.ToString());

			var xIncludeReader = await serviceProvider.GetRequiredService<XIncludeReader, XmlReader>(xmlReader);

			var builder = new StringBuilder();
			var xmlWriter = XmlWriter.Create(builder);
			while (await xIncludeReader.ReadAsync())
			{
				// ReSharper disable once MethodHasAsyncOverload
				xmlWriter.WriteNode(xIncludeReader, defattr: false);
			}

			xmlWriter.Close();

			Console.Write(builder.ToString());
		}
=======
		Console.Write(builder.ToString());
>>>>>>> Stashed changes
	}
}