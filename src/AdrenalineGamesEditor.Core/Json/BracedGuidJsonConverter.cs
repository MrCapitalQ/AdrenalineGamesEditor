using System.Text.Json;
using System.Text.Json.Serialization;

namespace MrCapitalQ.AdrenalineGamesEditor.Core.Json;

internal class BracedGuidJsonConverter : JsonConverter<Guid>
{
    public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (!(reader.GetString() is { } value))
            throw new JsonException();

        return Guid.Parse(value);
    }

    public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString("B"));
}
