using MrCapitalQ.AdrenalineGamesEditor.Core.Json;
using System.Buffers;
using System.Text;
using System.Text.Json;

namespace MrCapitalQ.AdrenalineGamesEditor.Core.Tests.Json;

public class StringBooleanJsonConverterTests
{
    private readonly JsonSerializerOptions _options = new();

    private readonly StringBooleanJsonConverter _converter = new();

    [InlineData("\"TRUE\"", true)]
    [InlineData("\"true\"", true)]
    [InlineData("\"FALSE\"", false)]
    [InlineData("\"False\"", false)]
    [InlineData("\"Non-boolean\"", false)]
    [Theory]
    public void Read_ConvertsFromJsonValue(string jsonValue, bool expected)
    {
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonValue));
        reader.Read();

        var actual = _converter.Read(ref reader, typeof(bool), _options);

        Assert.Equal(expected, actual);
    }

    [InlineData(true, "\"TRUE\"")]
    [InlineData(false, "\"FALSE\"")]
    [Theory]
    public void Write_ConvertsToJsonValue(bool value, string expected)
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new Utf8JsonWriter(buffer);

        _converter.Write(writer, value, _options);
        writer.Flush();

        var actual = Encoding.UTF8.GetString(buffer.WrittenSpan);

        Assert.Equal(expected, actual);
    }
}
