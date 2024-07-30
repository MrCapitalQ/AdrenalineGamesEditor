using Microsoft.Extensions.Logging;
using MrCapitalQ.AdrenalineGamesEditor.Core.FileSystem;
using System.Collections.Immutable;
using System.Text.Json;

namespace MrCapitalQ.AdrenalineGamesEditor.Core.Adrenaline;

internal class AdrenalineGamesDataService : IAdrenalineGamesDataService
{
    public event EventHandler? GamesDataChanged;

    private static readonly JsonSerializerOptions s_serializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly string _amdGameDbDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"AMD\CN");
    private readonly string _amdGameDbFileName = "gmdb.blb";
    private readonly string _amdGameDbFilePath;
    private readonly IFileSystemWatcher _fileSystemWatcher;
    private readonly IReadFileStreamCreator _readFileStreamCreator;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<AdrenalineGamesDataService> _logger;

    public AdrenalineGamesDataService(IFileSystemWatcher fileSystemWatcher,
        IReadFileStreamCreator fileReadStreamCreator,
        TimeProvider timeProvider,
        ILogger<AdrenalineGamesDataService> logger)
    {
        _amdGameDbFilePath = Path.Combine(_amdGameDbDirectoryPath, _amdGameDbFileName);

        _fileSystemWatcher = fileSystemWatcher;
        _readFileStreamCreator = fileReadStreamCreator;
        _timeProvider = timeProvider;
        _logger = logger;

        _fileSystemWatcher.Path = _amdGameDbDirectoryPath;
        _fileSystemWatcher.Filter = _amdGameDbFileName;
        _fileSystemWatcher.Changed += FileSystemWatcher_Changed;
        _fileSystemWatcher.EnableRaisingEvents = true;

        _ = UpdateGamesDataAsync();
    }

    public IReadOnlyCollection<AdrenalineGameInfo> GamesData { get; private set; } = [];

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

    private async void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        _fileSystemWatcher.Changed -= FileSystemWatcher_Changed;

        await Task.Delay(TimeSpan.FromSeconds(1), _timeProvider);
        await UpdateGamesDataAsync();

        _fileSystemWatcher.Changed += FileSystemWatcher_Changed;
    }
}
