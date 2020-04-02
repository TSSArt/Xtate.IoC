﻿using System;
using System.Collections.Immutable;
using JetBrains.Annotations;

namespace TSSArt.StateMachine
{
	[PublicAPI]
	public class IoProcessorOptions
	{
		public ImmutableArray<IEventProcessorFactory>   EventProcessorFactories   { get; set; }
		public ImmutableArray<IServiceFactory>          ServiceFactories          { get; set; }
		public ImmutableArray<IDataModelHandlerFactory> DataModelHandlerFactories { get; set; }
		public ImmutableArray<ICustomActionFactory>     CustomActionFactories     { get; set; }
		public ImmutableArray<IResourceLoader>          ResourceLoaders           { get; set; }
		public ImmutableDictionary<string, string>?     Configuration             { get; set; }
		public ILogger?                                 Logger                    { get; set; }
		public PersistenceLevel                         PersistenceLevel          { get; set; }
		public IStorageProvider?                        StorageProvider           { get; set; }
		public TimeSpan                                 SuspendIdlePeriod         { get; set; }
		public bool                                     VerboseValidation         { get; set; }
	}
}