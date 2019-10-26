﻿namespace TSSArt.StateMachine
{
	public interface ILocationEvaluator
	{
		void DeclareLocalVariable(IExecutionContext executionContext);

		void SetValue(IObject value, IExecutionContext executionContext);

		IObject GetValue(IExecutionContext executionContext);

		string GetName(IExecutionContext executionContext);
	}
}