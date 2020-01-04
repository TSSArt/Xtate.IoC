﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using TSSArt.StateMachine.EcmaScript;
using TSSArt.StateMachine.Services;

namespace TSSArt.StateMachine.IntegrationTest
{
	internal class Program
	{
		private static async Task Main(string[] args)
		{
			Trace.Listeners.Add(new ConsoleTraceListener());

			var baseUri = new Uri(args.Length > 0 ? args[0] : "http://localhost:5001/");
			using var handler = new HttpEventProcessorHandler(baseUri);
			var httpEventProcessor = handler.CreateEventProcessor(baseUri.AbsolutePath);
			var options = new IoProcessorOptions
						  {
								  EventProcessors = new[] { httpEventProcessor },
								  ServiceFactories = new[] { HttpClientService.Factory, SmtpClientService.Factory },
								  DataModelHandlerFactories = new[] { EcmaScriptDataModelHandler.Factory },
								  CustomActionProviders = new[] { BasicCustomActionProvider.Instance, MimeCustomActionProvider.Instance, MidCustomActionProvider.Instance },
								  StateMachineProvider = new ResourceProvider(),
								  Configuration = new Dictionary<string, string>
												  {
														  { "uiEndpoint", "http://localhost:5000/dialog" },
														  { "mailEndpoint", "http://mid.dev.tssart.com/MailServer/Web2/api/Mail/" }
												  }
						  };

			await using var ioProcessor = new IoProcessor(options);

			var prms = new DataModelObject
					   {
							   ["loginUrl"] = new DataModelValue("https://test.tssart.com/wp-login.php"),
							   ["username"] = new DataModelValue("tadex1"),
							   ["password"] = new DataModelValue("123456")
					   };

			var task = ioProcessor.Execute(new Uri(uriString: "login", UriKind.Relative), new DataModelValue(prms));

			var webHost = new WebHostBuilder()
						  .Configure(builder => builder.Run(handler.ProcessRequest))
						  .UseKestrel(serverOptions => serverOptions.ListenAnyIP(baseUri.Port))
						  .Build();

			await webHost.StartAsync().ConfigureAwait(false);

			dynamic result = await task.ConfigureAwait(false);

			var prms2 = new DataModelObject
					   {
							   ["profileUrl"] = new DataModelValue("https://test.tssart.com/wp-admin/profile.php"),
							   ["cookies"] = new DataModelValue(result.data.cookies)
					   };
			var task2 = ioProcessor.Execute(new Uri(uriString: "captureEmail", UriKind.Relative), new DataModelValue(prms2));

			dynamic result2 = await task2.ConfigureAwait(false);

			Console.WriteLine(DataModelConverter.ToJson(result));

			await webHost.StopAsync().ConfigureAwait(false);
		}
	}

	internal class ResourceProvider : IStateMachineProvider
	{
		public ValueTask<IStateMachine> GetStateMachine(Uri source)
		{
			using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TSSArt.StateMachine.IntegrationTest." + source + ".xml");
			using var xmlReader = XmlReader.Create(stream);
			var stateMachine = new ScxmlDirector(xmlReader, new BuilderFactory()).ConstructStateMachine();
			return new ValueTask<IStateMachine>(stateMachine);
		}

		public ValueTask<IStateMachine> GetStateMachine(string scxml)
		{
			using var reader = new StringReader(scxml);
			using var xmlReader = XmlReader.Create(reader);
			var stateMachine = new ScxmlDirector(xmlReader, new BuilderFactory()).ConstructStateMachine();
			return new ValueTask<IStateMachine>(stateMachine);
		}
	}
}