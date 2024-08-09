using Microsoft.Extensions.Logging;
using MrCapitalQ.AdrenalineGamesEditor.Core.Adrenaline.Models;
using MrCapitalQ.AdrenalineGamesEditor.Core.FileSystem;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MrCapitalQ.AdrenalineGamesEditor.Core.Adrenaline;

internal class AdrenalineGamesDataService : IAdrenalineGamesDataService
{
    public event EventHandler? GamesDataChanged;
    public event EventHandler? IsRestartRequiredChanged;

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
    private readonly IFileSystemWatcher _fileSystemWatcher;
    private readonly IReadFileStreamCreator _readFileStreamCreator;
    private readonly IFileWriter _fileWriter;
    private readonly IAdrenalineProcessManager _adrenalineProcessManager;
    private readonly GuidGenerator _guidGenerator;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<AdrenalineGamesDataService> _logger;
    private bool _isRestartRequired;

    public AdrenalineGamesDataService(IFileSystemWatcher fileSystemWatcher,
        IReadFileStreamCreator fileReadStreamCreator,
        IFileWriter fileWriter,
        IAdrenalineProcessManager adrenalineProcessManager,
        GuidGenerator guidGenerator,
        TimeProvider timeProvider,
        ILogger<AdrenalineGamesDataService> logger)
    {
        _amdGameDbFilePath = Path.Combine(_amdGameDbDirectoryPath, _amdGameDbFileName);

        _fileSystemWatcher = fileSystemWatcher;
        _readFileStreamCreator = fileReadStreamCreator;
        _fileWriter = fileWriter;
        _adrenalineProcessManager = adrenalineProcessManager;
        _guidGenerator = guidGenerator;
        _timeProvider = timeProvider;
        _logger = logger;

        _fileSystemWatcher.Path = _amdGameDbDirectoryPath;
        _fileSystemWatcher.Filter = _amdGameDbFileName;
        _fileSystemWatcher.Changed += FileSystemWatcher_Changed;
        _fileSystemWatcher.EnableRaisingEvents = true;

        _ = UpdateGamesDataAsync();
    }

    public IReadOnlyCollection<AdrenalineGameInfo> GamesData { get; private set; } = [];

    public bool IsRestartRequired
    {
        get => _isRestartRequired;
        private set
        {
            _isRestartRequired = value;
            OnIsRestartRequiredChanged();
        }
    }

    public async Task AddAsync(AdrenalineGameInfo gameInfo)
    {
        using var fileStream = _readFileStreamCreator.Open(_amdGameDbFilePath);
        var rootNode = await JsonNode.ParseAsync(fileStream)
            ?? throw new InvalidOperationException("Failed to parse Adrenaline data file.");

        var gamesNode = (rootNode["games"]?.AsArray())
            ?? throw new InvalidOperationException("Failed to find games data in Adrenaline data file.");

        // Resulting JSON node cannot be null unless DefaultNewGameEntry is literally "null".
        var newGame = JsonNode.Parse(DefaultNewGameEntry)!;

        var guid = gameInfo.Id;
        var existingIds = gamesNode.Select(x => x?["guid"]?.ToString()).Where(x => x is not null).ToHashSet();
        while (guid == Guid.Empty || existingIds.Contains(guid.ToString("b")))
        {
            guid = _guidGenerator.NewGuid();
        }

        newGame["guid"] = $"{{{guid}}}";
        newGame["title"] = gameInfo.DisplayName;
        newGame["commandline"] = gameInfo.CommandLine;
        newGame["exe_path"] = gameInfo.ExePath;
        newGame["image_info"] = gameInfo.ImagePath;

        gamesNode.Add(newGame);
        await _fileWriter.WriteContentAsync(_amdGameDbFilePath, rootNode.ToJsonString(s_serializerOptions));
        IsRestartRequired = true;
    }

    public async Task<bool> RestartAdrenalineAsync()
    {
        try
        {
            var isSuccessful = await _adrenalineProcessManager.RestartAsync();
            IsRestartRequired = false;

            return isSuccessful;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something went wrong while trying to restart Adrenaline.");
            return false;
        }
    }

    private async Task UpdateGamesDataAsync()
    {
        try
        {
            using var fileStream = _readFileStreamCreator.Open(_amdGameDbFilePath);
            var data = await JsonSerializer.DeserializeAsync<AdrenalineGamesDataModel>(fileStream, s_serializerOptions);

            GamesData = data?.Games
                .Where(x => !x.IsHidden && !x.IsAppForLink)
                .Select(x => new AdrenalineGameInfo(x.Guid, x.Title, x.ImageInfo, x.CommandLine, x.ExePath, x.IsManual))
                .ToImmutableList() ?? [];

            OnGamesDataChanged();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Adrenaline games data.");
        }
    }

    private void OnGamesDataChanged()
    {
        var raiseEvent = GamesDataChanged;
        raiseEvent?.Invoke(this, EventArgs.Empty);
    }

    private void OnIsRestartRequiredChanged()
    {
        var raiseEvent = IsRestartRequiredChanged;
        raiseEvent?.Invoke(this, EventArgs.Empty);
    }

    private async void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        _fileSystemWatcher.Changed -= FileSystemWatcher_Changed;

        await Task.Delay(TimeSpan.FromSeconds(1), _timeProvider);
        await UpdateGamesDataAsync();

        _fileSystemWatcher.Changed += FileSystemWatcher_Changed;
    }
}
