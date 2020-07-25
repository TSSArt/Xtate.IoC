﻿using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xtate.DataModel;
using Xtate.Persistence;

namespace Xtate.Core.Test.Legacy
{
	public class Evaluator : IExternalScriptExpression, IIntegerEvaluator, IStringEvaluator, IExecEvaluator, IArrayEvaluator, IObjectEvaluator, IBooleanEvaluator, ILocationEvaluator, IValueExpression,
							 ILocationExpression, IConditionExpression, IScriptExpression
	{
		public Evaluator(string? expression) => Expression = expression;

		public Evaluator(Uri? entityUri) => Uri = entityUri;

	#region Interface IArrayEvaluator

		public ValueTask<IObject[]> EvaluateArray(IExecutionContext executionContext, CancellationToken token) => new ValueTask<IObject[]>(new IObject[0]);

	#endregion

	#region Interface IBooleanEvaluator

		public ValueTask<bool> EvaluateBoolean(IExecutionContext executionContext, CancellationToken token) => new ValueTask<bool>(false);

	#endregion

	#region Interface IExecEvaluator

		public ValueTask Execute(IExecutionContext executionContext, CancellationToken token) => default;

	#endregion

	#region Interface IExternalScriptExpression

		public Uri? Uri { get; }

	#endregion

	#region Interface IIntegerEvaluator

		public ValueTask<int> EvaluateInteger(IExecutionContext executionContext, CancellationToken token) => new ValueTask<int>(0);

	#endregion

	#region Interface ILocationEvaluator

		public void DeclareLocalVariable(IExecutionContext executionContext) { }

		public ValueTask SetValue(IObject value, IExecutionContext executionContext, CancellationToken token) => default;

		public ValueTask<IObject> GetValue(IExecutionContext executionContext, CancellationToken token) => new ValueTask<IObject>((IObject) null!);

		public string GetName(IExecutionContext executionContext) => "?";

	#endregion

	#region Interface IObjectEvaluator

		public ValueTask<IObject> EvaluateObject(IExecutionContext executionContext, CancellationToken token) => new ValueTask<IObject>((IObject) null!);

	#endregion

	#region Interface IStringEvaluator

		public ValueTask<string> EvaluateString(IExecutionContext executionContext, CancellationToken token) => new ValueTask<string>("");

	#endregion

	#region Interface IValueExpression

		public string? Expression { get; }

	#endregion
	}

	public class TestDataModelHandler : DataModelHandlerBase
	{
		public TestDataModelHandler() : base(DefaultErrorProcessor.Instance) { }

		protected override void Visit(ref IValueExpression expression)
		{
			expression = new Evaluator(expression.Expression);
		}

		protected override void Visit(ref ILocationExpression expression)
		{
			expression = new Evaluator(expression.Expression);
		}

		protected override void Visit(ref IConditionExpression entity)
		{
			entity = new Evaluator(entity.Expression);
		}

		protected override void Visit(ref IScriptExpression entity)
		{
			entity = new Evaluator(entity.Expression);
		}

		protected override void Visit(ref IExternalScriptExpression entity)
		{
			entity = new Evaluator(entity.Uri);
		}
	}

	[TestClass]
	public class InterpreterModelPersistenceTest
	{
		private IStateMachine     _allStateMachine  = default!;
		private IDataModelHandler _dataModelHandler = default!;

		[TestInitialize]
		public void Initialize()
		{
			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Xtate.Core.Test.Legacy.test.scxml");

			var xmlReader = XmlReader.Create(stream);

			var director = new ScxmlDirector(xmlReader, BuilderFactory.Instance, DefaultErrorProcessor.Instance, namespaceResolver: null);

			_allStateMachine = director.ConstructStateMachine(StateMachineValidator.Instance);

			_dataModelHandler = new TestDataModelHandler();
		}

		[TestMethod]
		public void SaveInterpreterModelTest()
		{
			var model = new InterpreterModelBuilder(_allStateMachine, _dataModelHandler, customActionProviders: default, DefaultErrorProcessor.Instance).Build();
			var storeSupport = model.Root.As<IStoreSupport>();

			var storage = new InMemoryStorage(false);
			storeSupport.Store(new Bucket(storage));

			new StateMachineReader().Build(new Bucket(storage));
		}

		[TestMethod]
		public void SaveRestoreInterpreterModelWithStorageRecreateTest()
		{
			var model = new InterpreterModelBuilder(_allStateMachine, _dataModelHandler, customActionProviders: default, DefaultErrorProcessor.Instance).Build();
			var storeSupport = model.Root.As<IStoreSupport>();

			byte[] transactionLog;
			using (var storage = new InMemoryStorage(false))
			{
				storeSupport.Store(new Bucket(storage));
				transactionLog = new byte[storage.GetTransactionLogSize()];
				storage.WriteTransactionLogToSpan(new Span<byte>(transactionLog));

				Assert.AreEqual(expected: 0, storage.GetTransactionLogSize());
			}

			IStateMachine restoredStateMachine;
			using (var newStorage = new InMemoryStorage(transactionLog))
			{
				restoredStateMachine = new StateMachineReader().Build(new Bucket(newStorage));
			}

			new InterpreterModelBuilder(restoredStateMachine, _dataModelHandler, customActionProviders: default, DefaultErrorProcessor.Instance).Build();
		}

		[TestMethod]
		public void SaveRestoreInterpreterModelRuntimeModelTest()
		{
			var _ = new StateMachineFluentBuilder(BuilderFactory.Instance)
					.BeginState((Identifier) "a")
					.AddTransition(context => true, (Identifier) "a")
					.AddOnEntry(context => Console.WriteLine(@"OnEntry"))
					.EndState()
					.Build();

			var model = new InterpreterModelBuilder(_allStateMachine, _dataModelHandler, customActionProviders: default, DefaultErrorProcessor.Instance).Build();
			var storeSupport = model.Root.As<IStoreSupport>();

			byte[] transactionLog;
			using (var storage = new InMemoryStorage(false))
			{
				storeSupport.Store(new Bucket(storage));
				transactionLog = new byte[storage.GetTransactionLogSize()];
				storage.WriteTransactionLogToSpan(new Span<byte>(transactionLog));
			}

			IStateMachine restoredStateMachine;
			using (var newStorage = new InMemoryStorage(transactionLog))
			{
				restoredStateMachine = new StateMachineReader().Build(new Bucket(newStorage), model.EntityMap);
			}

			new InterpreterModelBuilder(restoredStateMachine, _dataModelHandler, customActionProviders: default, DefaultErrorProcessor.Instance).Build();
		}
	}
}