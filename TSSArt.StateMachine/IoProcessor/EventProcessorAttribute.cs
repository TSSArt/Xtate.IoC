﻿using System;

namespace TSSArt.StateMachine
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class EventProcessorAttribute : Attribute
	{
		public EventProcessorAttribute(string type)
		{
			if (string.IsNullOrEmpty(type)) throw new ArgumentException(message: "Value cannot be null or empty.", nameof(type));

			Type = type;
		}

		public string Type { get; }

		public string Alias { get; set; }
	}
}