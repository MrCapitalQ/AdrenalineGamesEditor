using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace MrCapitalQ.AdrenalineGamesEditor;

public class AdrenalineGamesDataService
{
    public event EventHandler? DataChanged;

    private const string DefaultNewGameEntry = """
        {
            "amdId": -1,
            "appDisplayScalingSet": "FALSE",
            "appHistogramCapture": "FALSE",
            "arguments": "",
            "athena_support": "FALSE",
            "auto_enable_ps_state": "USEGLOBAL",
            "averageFPS": -1,
            "color_enabled": "FALSE",
            "colors": [
            ],
            "commandline": "",
            "exe_path": "",
            "eyefinity_enabled": "FALSE",
            "framegen_enabled": 0,
            "freeSyncForceSet": "FALSE",
            "guid": "",
            "has_framegen_profile": "FALSE",
            "has_upscaling_profile": "FALSE",
            "hidden": "FALSE",
            "image_info": "",
            "install_location": "",
            "installer_id": "",
            "is_appforlink": "FALSE",
            "is_favourite": "FALSE",
            "last_played_mins": 0,
            "lastlaunchtime": "",
            "lastperformancereporttime": "",
            "lnk_path": "",
            "manual": "TRUE",
            "origin_id": -1,
            "overdrive": [
            ],
            "overdrive_enabled": "FALSE",
            "percentile95_msec": -1,
            "profileCustomized": "FALSE",
            "profileEnabled": "TRUE",
            "rayTracing": "FALSE",
            "rendering_process": "",
            "revertuserprofiletype": -1,
            "smartshift_enabled": "FALSE",
            "special_flags": "",
            "steam_id": -1,
            "title": "",
            "total_played_mins": 0,
            "uninstall_location": -1,
            "uninstalled": "FALSE",
            "uplay_id": -1,
            "upscaling_enabled": "FALSE",
            "upscaling_sharpness": 75,
            "upscaling_target_resolution": "",
            "upscaling_use_borderless": "FALSE",
            "useEyefinity": "FALSE",
            "userprofiletype": -1,
            "week_played_mins": 0
        }
        """;
    private static readonly JsonSerializerOptions s_serializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

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

    public void AddGame(string title, string commandLine, string exePath, string imageInfo)
    {
        using var fileStream = new FileStream(_amdGameDbFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var rootNode = JsonNode.Parse(fileStream);
        if (rootNode is null)
            return;

        var gamesNode = rootNode["games"]?.AsArray();
        if (gamesNode is null)
            return;

        var newGame = JsonNode.Parse(DefaultNewGameEntry);
        if (newGame is null)
            return;

        // TODO: ensure unique
        newGame["guid"] = $"{{{Guid.NewGuid()}}}";
        newGame["title"] = title;
        newGame["commandline"] = commandLine;
        newGame["exe_path"] = exePath;
        newGame["image_info"] = imageInfo;

        gamesNode.Add(newGame);
        File.WriteAllText(_amdGameDbFilePath, rootNode.ToJsonString(s_serializerOptions));
    }

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
        }
    }

    private void OnDataChanged()
    {
        var raiseEvent = DataChanged;
        raiseEvent?.Invoke(this, EventArgs.Empty);
    }

    private async void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        _fileSystemWatcher.Changed -= FileSystemWatcher_Changed;
        await Task.Delay(1000);
        await UpdateAsync();
        _fileSystemWatcher.Changed += FileSystemWatcher_Changed;
    }

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

