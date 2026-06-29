using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnityEngine;

namespace ColorsOfJoy.Converters;

public class Color32JsonConverter : JsonConverter<Color32>
{
    public override Color32 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject token for Color32.");

        byte r = 0, g = 0, b = 0, a = 255;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return new Color32(r, g, b, a);

            if (reader.TokenType != JsonTokenType.PropertyName)
                continue;

            string propertyName = reader.GetString();
            reader.Read();

            switch (propertyName)
            {
                case "r": r = reader.GetByte(); break;
                case "g": g = reader.GetByte(); break;
                case "b": b = reader.GetByte(); break;
                case "a": a = reader.GetByte(); break;
            }
        }

        throw new JsonException("Unexpected end when reading Color32.");
    }

    public override void Write(Utf8JsonWriter writer, Color32 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("r", value.r);
        writer.WriteNumber("g", value.g);
        writer.WriteNumber("b", value.b);
        writer.WriteNumber("a", value.a);
        writer.WriteEndObject();
    }
}