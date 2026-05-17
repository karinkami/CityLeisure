using System.Text.Json;
using System.Text.Json.Serialization;

namespace CityLeisure.Api.Serialization;

public sealed class SafeNullableDoubleJsonConverter : JsonConverter<double?>
{
    public override double? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;
        var v = reader.GetDouble();
        return double.IsFinite(v) ? v : null;
    }

    public override void Write(Utf8JsonWriter writer, double? value, JsonSerializerOptions options)
    {
        if (value is null || !double.IsFinite(value.Value))
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteNumberValue(value.Value);
    }
}
