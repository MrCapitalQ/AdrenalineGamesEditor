using System.Text.Json;
using System.Text.Json.Serialization;

namespace MrCapitalQ.AdrenalineGamesEditor.Core.Json;

internal class StringBooleanJsonConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => "TRUE".Equals(reader.GetString(), StringComparison.OrdinalIgnoreCase);

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString().ToUpperInvariant());
}
