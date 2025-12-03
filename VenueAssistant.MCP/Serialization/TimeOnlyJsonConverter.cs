using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VenueAssistant.MCP.Serialization;

/// <summary>
/// Serializes TimeOnly to ISO-like HH:mm:ss strings and parses them back.
/// </summary>
public sealed class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    private const string Format = "HH:mm:ss";

    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new JsonException("TimeOnly value was null or empty.");
        }

        return TimeOnly.Parse(value, CultureInfo.InvariantCulture);
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
    }
}
