﻿using System;

namespace TSSArt.StateMachine
{
	public class StateMachineResult
	{
		public StateMachineResult(StateMachineExitStatus status, DataModelValue result = default)
		{
			Result = result;
			Status = status;
		}

		public StateMachineResult(StateMachineExitStatus status, Exception exception)
		{
			Status = status;
			Exception = exception;
		}

		public DataModelValue Result { get; }

		public StateMachineExitStatus Status { get; }

		public Exception Exception { get; }
	}
}