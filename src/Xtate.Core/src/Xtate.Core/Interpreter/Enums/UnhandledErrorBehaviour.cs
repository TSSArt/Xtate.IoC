﻿namespace TSSArt.StateMachine
{
	public enum UnhandledErrorBehaviour
	{
		DestroyStateMachine = 0,
		HaltStateMachine    = 1,
		IgnoreError         = 2
	}
}