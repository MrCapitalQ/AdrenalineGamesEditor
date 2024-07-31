using MrCapitalQ.AdrenalineGamesEditor.Core.Json;
using System.Text.Json.Serialization;

namespace MrCapitalQ.AdrenalineGamesEditor.Core.Adrenaline.Models;

internal record AdrenalineGameModel
{
    [JsonConverter(typeof(BracedGuidJsonConverter))]
    [JsonPropertyName("guid")]
    public Guid Guid { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("image_info")]
    public string ImageInfo { get; init; } = string.Empty;

    [JsonPropertyName("commandline")]
    public string CommandLine { get; init; } = string.Empty;

    [JsonPropertyName("exe_path")]
    public string ExePath { get; init; } = string.Empty;

    [JsonConverter(typeof(StringBooleanJsonConverter))]
    [JsonPropertyName("manual")]
    public bool IsManual { get; init; }

    [JsonConverter(typeof(StringBooleanJsonConverter))]
    [JsonPropertyName("is_appforlink")]
    public bool IsAppForLink { get; init; }

    [JsonConverter(typeof(StringBooleanJsonConverter))]
    [JsonPropertyName("hidden")]
    public bool IsHidden { get; init; }
}
