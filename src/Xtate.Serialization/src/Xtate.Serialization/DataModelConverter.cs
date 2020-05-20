﻿using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using TSSArt.StateMachine.Annotations;

namespace TSSArt.StateMachine
{
	[Flags]
	[PublicAPI]
	public enum DataModelConverterOptions
	{
		/// <summary>
		///     Serialization of undefined value will cause an exception. Array with undefined element will cause an exception.
		///     Properties with undefined values in object will be skipped.
		/// </summary>
		UndefinedNotAllowed = 0,

		/// <summary>
		///     Undefined value will serialize to empty string. Undefined elements in array will be skipped. Properties with
		///     undefined values in object will be skipped.
		/// </summary>
		UndefinedToSkip = 1,

		/// <summary>
		///     Undefined value will serialize to (null) value. Undefined elements in array will be serialized as (null).
		///     Properties with undefined values in object will be serialized as properties with (null).
		/// </summary>
		UndefinedToNull = 2,

		/// <summary>
		///     Undefined value will serialize to (null) value. Undefined elements in array will be serialized as (null).
		///     Properties with undefined values in object will be skipped.
		/// </summary>
		UndefinedToSkipOrNull = UndefinedToSkip | UndefinedToNull,

		WriteIndented = 4
	}

	[PublicAPI]
	public static class DataModelConverter
	{
		private static readonly JsonSerializerOptions DefaultOptions = CreateOptions(DataModelConverterOptions.UndefinedNotAllowed);
		private static JsonSerializerOptions GetOptions(DataModelConverterOptions options) => options == DataModelConverterOptions.UndefinedNotAllowed ? DefaultOptions : CreateOptions(options);

		private static JsonSerializerOptions CreateOptions(DataModelConverterOptions options) =>
				new JsonSerializerOptions
				{
						Converters =
						{
								new JsonValueConverter(options),
								new JsonObjectConverter(options),
								new JsonArrayConverter(options)
						},
						WriteIndented = (options & DataModelConverterOptions.WriteIndented) != 0,
						MaxDepth = 64
				};

		public static string ToJson(DataModelValue value, DataModelConverterOptions options = default) => JsonSerializer.Serialize(value, GetOptions(options));

		public static byte[] ToJsonUtf8Bytes(DataModelValue value, DataModelConverterOptions options = default) => JsonSerializer.SerializeToUtf8Bytes(value, GetOptions(options));

		public static Task ToJsonAsync(Stream stream, DataModelValue value, DataModelConverterOptions options = default, CancellationToken token = default) =>
				JsonSerializer.SerializeAsync(stream, value, GetOptions(options), token);

		public static DataModelValue FromJson(string json) => JsonSerializer.Deserialize<DataModelValue>(json, DefaultOptions);

		public static DataModelValue FromJson(ReadOnlySpan<byte> utf8Json) => JsonSerializer.Deserialize<DataModelValue>(utf8Json, DefaultOptions);

		public static ValueTask<DataModelValue> FromJsonAsync(Stream stream, CancellationToken token = default) => JsonSerializer.DeserializeAsync<DataModelValue>(stream, DefaultOptions, token);

		private class JsonValueConverter : JsonConverter<DataModelValue>
		{
			private readonly DataModelConverterOptions _options;

			public JsonValueConverter(DataModelConverterOptions options) => _options = options;

			public override DataModelValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
					reader.TokenType switch
					{
							JsonTokenType.True => true,
							JsonTokenType.False => false,
							JsonTokenType.Null => DataModelValue.Null,
							JsonTokenType.String when reader.TryGetDateTimeOffset(out var datetime) => datetime,
							JsonTokenType.String => reader.GetString(),
							JsonTokenType.Number => reader.GetDouble(),
							JsonTokenType.StartObject => JsonSerializer.Deserialize<DataModelObject>(ref reader, options),
							JsonTokenType.StartArray => JsonSerializer.Deserialize<DataModelArray>(ref reader, options),
							_ => Infrastructure.UnexpectedValue<DataModelValue>(Resources.Exception_Not_expected_token_type)
					};

			public override void Write(Utf8JsonWriter writer, DataModelValue value, JsonSerializerOptions options)
			{
				switch (value.Type)
				{
					case DataModelValueType.Undefined when (_options & DataModelConverterOptions.UndefinedToNull) != 0:
						writer.WriteNullValue();
						break;

					case DataModelValueType.Undefined when (_options & DataModelConverterOptions.UndefinedToSkip) == 0:
						throw new JsonException(Resources.Exception_Undefined_value_not_allowed);

					case DataModelValueType.Undefined:
						break;

					case DataModelValueType.Null:
						writer.WriteNullValue();
						break;

					case DataModelValueType.String:
						writer.WriteStringValue(value.AsString());
						break;

					case DataModelValueType.Number:
						writer.WriteNumberValue(value.AsNumber());
						break;

					case DataModelValueType.DateTime:
						writer.WriteStringValue(value.AsDateTimeOffset());
						break;

					case DataModelValueType.Boolean:
						writer.WriteBooleanValue(value.AsBoolean());
						break;

					case DataModelValueType.Object:
						JsonSerializer.Serialize(writer, value.AsObject(), options);
						break;

					case DataModelValueType.Array:
						JsonSerializer.Serialize(writer, value.AsArray(), options);
						break;

					default:
						Infrastructure.UnexpectedValue(Resources.Exception_Unknown_type_for_serialization);
						break;
				}
			}
		}

		private class JsonObjectConverter : JsonConverter<DataModelObject>
		{
			private readonly DataModelConverterOptions _options;

			public JsonObjectConverter(DataModelConverterOptions options) => _options = options;

			public override DataModelObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				var obj = new DataModelObject();

				Infrastructure.Assert(reader.TokenType == JsonTokenType.StartObject);

				reader.Read();

				while (reader.TokenType != JsonTokenType.EndObject)
				{
					var name = reader.GetString();
					var value = JsonSerializer.Deserialize<DataModelValue>(ref reader, options);

					obj[name] = value;

					reader.Read();
				}

				reader.Read();

				return obj;
			}

			public override void Write(Utf8JsonWriter writer, DataModelObject obj, JsonSerializerOptions options)
			{
				if (writer.CurrentDepth > options.MaxDepth)
				{
					throw new JsonException(Resources.Exception_Cycle_reference_detected);
				}

				writer.WriteStartObject();

				foreach (var name in obj.Properties)
				{
					var value = obj[name];
					if (!value.IsUndefined())
					{
						writer.WritePropertyName(name);
						JsonSerializer.Serialize(writer, value, options);
					}
					else if ((_options & DataModelConverterOptions.UndefinedToSkipOrNull) == DataModelConverterOptions.UndefinedToNull)
					{
						writer.WritePropertyName(name);
						writer.WriteNullValue();
					}
				}

				writer.WriteEndObject();
			}
		}

		private class JsonArrayConverter : JsonConverter<DataModelArray>
		{
			private readonly DataModelConverterOptions _options;

			public JsonArrayConverter(DataModelConverterOptions options) => _options = options;

			public override DataModelArray Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				var array = new DataModelArray();

				Infrastructure.Assert(reader.TokenType == JsonTokenType.StartArray);

				reader.Read();

				while (reader.TokenType != JsonTokenType.EndArray)
				{
					var value = JsonSerializer.Deserialize<DataModelValue>(ref reader, options);

					array.Add(value);

					reader.Read();
				}

				reader.Read();

				return array;
			}

			public override void Write(Utf8JsonWriter writer, DataModelArray array, JsonSerializerOptions options)
			{
				writer.WriteStartArray();

				var arrayLength = array.Length;
				for (var i = 0; i < arrayLength; i ++)
				{
					var value = array[i];
					if (!value.IsUndefined())
					{
						JsonSerializer.Serialize(writer, value, options);
					}
					else if ((_options & DataModelConverterOptions.UndefinedToNull) != 0)
					{
						writer.WriteNullValue();
					}
					else if ((_options & DataModelConverterOptions.UndefinedToSkip) == 0)
					{
						throw new JsonException(Resources.Exception_Undefined_value_not_allowed);
					}
				}

				writer.WriteEndArray();
			}
		}
	}
}