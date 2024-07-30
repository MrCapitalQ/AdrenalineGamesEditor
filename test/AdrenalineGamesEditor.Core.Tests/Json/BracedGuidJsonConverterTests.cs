using MrCapitalQ.AdrenalineGamesEditor.Core.Json;
using System.Buffers;
using System.Text;
using System.Text.Json;

namespace MrCapitalQ.AdrenalineGamesEditor.Core.Tests.Json;

public class BracedGuidJsonConverterTests
{
    private readonly JsonSerializerOptions _options = new();

    private readonly BracedGuidJsonConverter _converter = new();

    [Fact]
    public void Read_BracedGuidValue_ConvertsFromJsonValue()
    {
        var jsonValue = "\"{be540504-826a-4fc6-8ea2-fca1a4373f63}\"";
        var expected = Guid.Parse("be540504-826a-4fc6-8ea2-fca1a4373f63");
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonValue));
        reader.Read();

        var actual = _converter.Read(ref reader, typeof(Guid), _options);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Read_NullValue_ThrowsJsonException()
    {
        var jsonValue = "null";

        Assert.Throws<JsonException>(() =>
        {
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonValue));
            reader.Read();

            _converter.Read(ref reader, typeof(Guid), _options);
        });
    }

    [Fact]
    public void Write_ConvertsToJsonValue()
    {
        var value = Guid.Parse("be540504-826a-4fc6-8ea2-fca1a4373f63");
        var expected = "\"{be540504-826a-4fc6-8ea2-fca1a4373f63}\"";
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new Utf8JsonWriter(buffer);

        _converter.Write(writer, value, _options);
        writer.Flush();

        var actual = Encoding.UTF8.GetString(buffer.WrittenSpan);

        Assert.Equal(expected, actual);
    }
}
