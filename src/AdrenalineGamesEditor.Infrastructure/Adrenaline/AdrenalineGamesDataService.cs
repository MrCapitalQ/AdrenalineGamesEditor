using Microsoft.Extensions.Logging;
using MrCapitalQ.AdrenalineGamesEditor.Core.Adrenaline;
using System.Collections.Immutable;
using System.Text.Json;

namespace MrCapitalQ.AdrenalineGamesEditor.Infrastructure.Adrenaline;

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

        _ = UpdateGamesDataAsync();
    }

    public IReadOnlyCollection<AdrenalineGameInfo> GamesData { get; private set; } = [];

    private async Task UpdateGamesDataAsync()
    {
        try
        {
            using var fileStream = new FileStream(_amdGameDbFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var data = await JsonSerializer.DeserializeAsync<AdrenalineGameDataModel>(fileStream, s_serializerOptions);

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
        await Task.Delay(1000);
        await UpdateGamesDataAsync();
        _fileSystemWatcher.Changed += FileSystemWatcher_Changed;
    }
}
