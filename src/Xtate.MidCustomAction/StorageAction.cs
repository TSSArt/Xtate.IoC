﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace TSSArt.StateMachine
{
	public class StorageAction : CustomActionBase
	{
		private const    string  Location  = "location";
		private const    string  Operation = "operation";
		private const    string  Template  = "template";
		private const    string  Rule      = "rule";
		private readonly string? _operation;
		private readonly string? _rule;
		private readonly string? _template;

		public StorageAction(XmlReader xmlReader, ICustomActionContext access) : base(access)
		{
			if (xmlReader == null) throw new ArgumentNullException(nameof(xmlReader));

			RegisterArgument(Location, xmlReader.GetAttribute(Location));
			RegisterResultLocation(xmlReader.GetAttribute(Location));

			_operation = xmlReader.GetAttribute(Operation)?.ToUpperInvariant();
			_template = xmlReader.GetAttribute(Template)?.ToUpperInvariant();
			_rule = xmlReader.GetAttribute(Rule);

			//<storage xmlns="http://tssart.com/scxml/customaction/mid" location="username"
			//operation="create" template="userid" rule="[a-z]{1,20}" />
			//<mid:storage location="username" operation="get" variable="username" />
			//<mid:storage location="password" operation="set" variable="password" />
		}

		internal static void FillXmlNameTable(XmlNameTable xmlNameTable)
		{
			xmlNameTable.Add(Location);
			xmlNameTable.Add(Operation);
			xmlNameTable.Add(Template);
			xmlNameTable.Add(Rule);
		}

		protected override DataModelValue Evaluate(IReadOnlyDictionary<string, DataModelValue> arguments)
		{
			if (arguments == null) throw new ArgumentNullException(nameof(arguments));


			if (_operation == "CREATE")
			{
				var locationValue = arguments[Location];
				var lastValue = locationValue.AsStringOrDefault() ?? string.Empty;
				return CreateValue(lastValue);
			}

			throw new NotSupportedException($"Unknown operation [{_operation}]");
		}

		private string CreateValue(string lastValue)
		{
			var query = EnumeratePredefinedValues();

			if (lastValue.Length > 0)
			{
				query = query.SkipWhile(v => v != lastValue).Skip(1);
			}

			return query.FirstOrDefault() ?? CreateRandomValue();
		}

		private string CreateRandomValue()
		{
			string result;

			foreach (var value in GetPredefinedValues())
			{
				if (TryGenerate(value, index: 0, random: true, out result))
				{
					return result;
				}
			}

			if (TryGenerate(string.Empty, index: 0, random: true, out result))
			{
				return result;
			}

			throw new InvalidOperationException(@"Can't generate value.");
		}

		private IEnumerable<string> EnumeratePredefinedValues()
		{
			foreach (var value in GetPredefinedValues())
			{
				if (TryGenerate(value, index: 0, random: false, out var result))
				{
					yield return result;
				}
			}

			foreach (var value in GetPredefinedValues())
			{
				for (var i = 1; i < 9; i ++)
				{
					if (TryGenerate(value, i, random: false, out var result))
					{
						yield return result;
					}
				}
			}
		}

		private string[] GetPredefinedValues()
		{
			return _template switch
			{
					"USERID" => new[] { "tadex", "xtadex" },
					_ => Array.Empty<string>()
			};
		}

		private bool TryGenerate(string value, int index, bool random, out string result)
		{
			result = value;

			if (index > 0)
			{
				result += index;
			}

			if (random)
			{
				var length = _template == "PASSWORD" ? 16 : 2;
				for (var i = 0; i < length; i ++)
				{
					result += new Random().Next(minValue: 0, maxValue: 9);
				}
			}

			return Regex.IsMatch(result, _rule);
		}
	}
}