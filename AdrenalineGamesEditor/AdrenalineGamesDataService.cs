using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MrCapitalQ.AdrenalineGamesEditor;

public class AdrenalineGamesDataService
{
    public event EventHandler? DataChanged;

    private static readonly JsonSerializerOptions s_serializerOptions = new(JsonSerializerDefaults.Web);

    private readonly string _amdGameDbDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"AMD\CN");
    private readonly string _amdGameDbFileName = "gmdb.blb";
    private readonly string _amdGameDbFilePath;
    private readonly FileSystemWatcher _fileSystemWatcher = new();
    private readonly ILogger<AdrenalineGamesDataService> _logger;

    public AdrenalineGamesDataService(ILogger<AdrenalineGamesDataService> logger)
    {
        _logger = logger;

        _amdGameDbFilePath = Path.Combine(_amdGameDbDirectoryPath, _amdGameDbFileName);

        _fileSystemWatcher.Path = _amdGameDbDirectoryPath;
        _fileSystemWatcher.Filter = _amdGameDbFileName;
        _fileSystemWatcher.Changed += FileSystemWatcher_Changed;
        _fileSystemWatcher.EnableRaisingEvents = true;

        _ = UpdateAsync();
    }

    public IEnumerable<AdrenalineGameInfo> GamesData { get; private set; } = [];

    private async Task UpdateAsync()
    {
        try
        {
            using var fileStream = new FileStream(_amdGameDbFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var data = await JsonSerializer.DeserializeAsync<AdrenalineGameDataModel>(fileStream, s_serializerOptions);

            GamesData = data is not null
                ? data.Games
                    .Where(x => !x.IsHidden && !x.IsAppForLink)
                    .Select(x => new AdrenalineGameInfo(x.Guid, x.Title, x.ImageInfo, x.CommandLine, x.ExePath, x.IsManual))
                    .ToList()
                : [];
            OnDataChanged();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Adrenaline games data.");
            throw;
        }
    }

    private void OnDataChanged()
    {
        var raiseEvent = DataChanged;
        raiseEvent?.Invoke(this, EventArgs.Empty);
    }

    private async void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e) => await UpdateAsync();

    private class AdrenalineGameDataModel
    {
        [JsonPropertyName("games")]
        public IEnumerable<AdrenalineGameModel> Games { get; init; } = [];
    }

    private record AdrenalineGameModel
    {
        [JsonConverter(typeof(AdrenalineGuidJsonConverter))]
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

        [JsonConverter(typeof(AdrenalineBooleanJsonConverter))]
        [JsonPropertyName("manual")]
        public bool IsManual { get; init; }

        [JsonConverter(typeof(AdrenalineBooleanJsonConverter))]
        [JsonPropertyName("is_appforlink")]
        public bool IsAppForLink { get; init; }

        [JsonConverter(typeof(AdrenalineBooleanJsonConverter))]
        [JsonPropertyName("hidden")]
        public bool IsHidden { get; init; }
    }

    private class AdrenalineBooleanJsonConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => "TRUE".Equals(reader.GetString(), StringComparison.OrdinalIgnoreCase);

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString().ToUpperInvariant());
    }

    private class AdrenalineGuidJsonConverter : JsonConverter<Guid>
    {
        public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => Guid.Parse(reader.GetString()?.Trim('{', '}') ?? string.Empty);

        public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
            => writer.WriteStringValue($"{{{value}}}");
    }
}

public record AdrenalineGameInfo(Guid Id, string DisplayName, string ImagePath, string CommandLine, string ExePath, bool IsManual);

