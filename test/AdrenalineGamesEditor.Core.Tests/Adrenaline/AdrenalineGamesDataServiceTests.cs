using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using MrCapitalQ.AdrenalineGamesEditor.Core.Adrenaline;
using MrCapitalQ.AdrenalineGamesEditor.Core.FileSystem;
using NSubstitute.ExceptionExtensions;
using System.Text;

namespace MrCapitalQ.AdrenalineGamesEditor.Core.Tests.Adrenaline;

public class AdrenalineGamesDataServiceTests
{
    private readonly IFileSystemWatcher _fileSystemWatcher = Substitute.For<IFileSystemWatcher>();
    private readonly IReadFileStreamCreator _readFileStreamCreator = Substitute.For<IReadFileStreamCreator>();
    private readonly IFileWriter _fileWriter = Substitute.For<IFileWriter>();
    private readonly IAdrenalineProcessManager _adrenalineProcessManager = Substitute.For<IAdrenalineProcessManager>();
    private readonly GuidGenerator _guidGenerator = Substitute.For<GuidGenerator>();
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly FakeLogger<AdrenalineGamesDataService> _logger = new();
    private readonly string _amdGameDbFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"AMD\CN\gmdb.blb");

    private readonly AdrenalineGamesDataService _service;

    public AdrenalineGamesDataServiceTests()
    {
        var json = "{ \"games\": [] }";
        _readFileStreamCreator.Open(Arg.Any<string>())
            .Returns(new MemoryStream(Encoding.UTF8.GetBytes(json)), new MemoryStream(Encoding.UTF8.GetBytes(json)));
        _service = new AdrenalineGamesDataService(_fileSystemWatcher,
            _readFileStreamCreator,
            _fileWriter,
            _adrenalineProcessManager,
            _guidGenerator,
            _timeProvider,
            _logger);
    }

    [Fact]
    public void Ctor_WithGamesData_PopulatesGamesData()
    {
        var expected = new AdrenalineGameInfo(Guid.Parse("be540504-826a-4fc6-8ea2-fca1a4373f63"),
            "Test Game",
            "Path_To_Image.png",
            "Path_To_CommandLine",
            "Path_To_Exe.exe",
            true);
        var json = $$"""
            {
              "games": [
                {
                  "commandline": "{{expected.CommandLine}}",
                  "exe_path": "{{expected.ExePath}}",
                  "guid": "{{{expected.Id}}}",
                  "hidden": "FALSE",
                  "image_info": "{{expected.ImagePath}}",
                  "is_appforlink": "FALSE",
                  "manual": "TRUE",
                  "title": "{{expected.DisplayName}}"
                }
              ]
            }            
            """;
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        _readFileStreamCreator.Open(Arg.Any<string>()).Returns(stream);

        var service = new AdrenalineGamesDataService(_fileSystemWatcher,
            _readFileStreamCreator,
            _fileWriter,
            _adrenalineProcessManager,
            _guidGenerator,
            _timeProvider,
            _logger);

        var actual = Assert.Single(service.GamesData);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Ctor_WithNullData_PopulatesNoGamesData()
    {
        var json = "null";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        _readFileStreamCreator.Open(Arg.Any<string>()).Returns(stream);

        var service = new AdrenalineGamesDataService(_fileSystemWatcher,
            _readFileStreamCreator,
            _fileWriter,
            _adrenalineProcessManager,
            _guidGenerator,
            _timeProvider,
            _logger);

        Assert.Empty(service.GamesData);
    }

    [Fact]
    public void Ctor_WithHiddenGamesData_ExcludesHiddenGames()
    {
        var json = """
            {
              "games": [
                {
                  "commandline": "Path_To_CommandLine",
                  "exe_path": "Path_To_Exe.exe",
                  "guid": "{be540504-826a-4fc6-8ea2-fca1a4373f63}",
                  "hidden": "TRUE",
                  "image_info": "Path_To_Image.png",
                  "is_appforlink": "FALSE",
                  "manual": "TRUE",
                  "title": "Test Game"
                }
              ]
            }            
            """;
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        _readFileStreamCreator.Open(Arg.Any<string>()).Returns(stream);

        var service = new AdrenalineGamesDataService(_fileSystemWatcher,
            _readFileStreamCreator,
            _fileWriter,
            _adrenalineProcessManager,
            _guidGenerator,
            _timeProvider,
            _logger);

        Assert.Empty(service.GamesData);
    }

    [Fact]
    public void Ctor_WithAppForLinkGamesData_ExcludesAppForLinkGames()
    {
        var json = """
            {
              "games": [
                {
                  "commandline": "Path_To_CommandLine",
                  "exe_path": "Path_To_Exe.exe",
                  "guid": "{be540504-826a-4fc6-8ea2-fca1a4373f63}",
                  "hidden": "FALSE",
                  "image_info": "Path_To_Image.png",
                  "is_appforlink": "TRUE",
                  "manual": "TRUE",
                  "title": "Test Game"
                }
              ]
            }            
            """;
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        _readFileStreamCreator.Open(Arg.Any<string>()).Returns(stream);

        var service = new AdrenalineGamesDataService(_fileSystemWatcher,
            _readFileStreamCreator,
            _fileWriter,
            _adrenalineProcessManager,
            _guidGenerator,
            _timeProvider,
            _logger);

        Assert.Empty(service.GamesData);
    }

    [Fact]
    public void Ctor_ExceptionThrown_LogsError()
    {
        _readFileStreamCreator.Open(Arg.Any<string>()).Throws<InvalidOperationException>();

        var service = new AdrenalineGamesDataService(_fileSystemWatcher,
            _readFileStreamCreator,
            _fileWriter,
            _adrenalineProcessManager,
            _guidGenerator,
            _timeProvider,
            _logger);

        Assert.Empty(service.GamesData);
        Assert.Equal("Failed to update Adrenaline games data.", _logger.LatestRecord.Message);
        Assert.Equal(LogLevel.Error, _logger.LatestRecord.Level);
    }

    [Fact]
    public async Task AddAsync_SuccessfulDbFileRead_WritesFileWithNewGame()
    {
        // Arrange
        _guidGenerator.NewGuid().Returns(Guid.Parse("9721c8ae-b162-4e64-bbc6-eac6d933b711"));
        var eventRaised = false;
        _service.IsRestartRequiredChanged += (_, _) => eventRaised = true;
        var gameInfo = new AdrenalineGameInfo(Guid.Empty,
            "Test Display Name",
            @"C:\Path\Image.png",
            "Test-Command",
            @"C:\Path\Executabe.exe",
            true);
        var expected = """
            {
              "games": [
                {
                  "amdId": -1,
                  "appDisplayScalingSet": "FALSE",
                  "appHistogramCapture": "FALSE",
                  "arguments": "",
                  "athena_support": "FALSE",
                  "auto_enable_ps_state": "USEGLOBAL",
                  "averageFPS": -1,
                  "color_enabled": "FALSE",
                  "colors": [],
                  "commandline": "Test-Command",
                  "exe_path": "C:\\Path\\Executabe.exe",
                  "eyefinity_enabled": "FALSE",
                  "framegen_enabled": 0,
                  "freeSyncForceSet": "FALSE",
                  "guid": "{9721c8ae-b162-4e64-bbc6-eac6d933b711}",
                  "has_framegen_profile": "FALSE",
                  "has_upscaling_profile": "FALSE",
                  "hidden": "FALSE",
                  "image_info": "C:\\Path\\Image.png",
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
                  "overdrive": [],
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
                  "title": "Test Display Name",
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
              ]
            }
            """;

        // Act
        await _service.AddAsync(gameInfo);

        // Assert
        Assert.True(_service.IsRestartRequired);
        Assert.True(eventRaised);
        await _fileWriter.Received(1).WriteContentAsync(_amdGameDbFilePath, expected);
    }

    [Fact]
    public async Task AddAsync_CollisionWhenGeneratingIds_UsesFirstUnusedId()
    {
        // Arrange
        var takenGuid = Guid.Parse("8a90fc4c-72fc-4ec2-9f2c-f85f2fde1fe1");
        var expectedGuid = Guid.Parse("9721c8ae-b162-4e64-bbc6-eac6d933b711");
        var json = $$"""
            {
              "games": [
                null,
                {
                  "guid": null
                },
                {
                  "guid": "{{{takenGuid}}}"
                }
              ]
            }
            """;
        _readFileStreamCreator.Open(Arg.Any<string>()).Returns(new MemoryStream(Encoding.UTF8.GetBytes(json)));
        _guidGenerator.NewGuid().Returns(takenGuid, expectedGuid);
        var gameInfo = new AdrenalineGameInfo(Guid.Empty,
            "Test Display Name",
            @"C:\Path\Image.png",
            "Test-Command",
            @"C:\Path\Executabe.exe",
            true);
        var expected = """
            {
              "games": [
                null,
                {
                  "guid": null
                },
                {
                  "guid": "{8a90fc4c-72fc-4ec2-9f2c-f85f2fde1fe1}"
                },
                {
                  "amdId": -1,
                  "appDisplayScalingSet": "FALSE",
                  "appHistogramCapture": "FALSE",
                  "arguments": "",
                  "athena_support": "FALSE",
                  "auto_enable_ps_state": "USEGLOBAL",
                  "averageFPS": -1,
                  "color_enabled": "FALSE",
                  "colors": [],
                  "commandline": "Test-Command",
                  "exe_path": "C:\\Path\\Executabe.exe",
                  "eyefinity_enabled": "FALSE",
                  "framegen_enabled": 0,
                  "freeSyncForceSet": "FALSE",
                  "guid": "{9721c8ae-b162-4e64-bbc6-eac6d933b711}",
                  "has_framegen_profile": "FALSE",
                  "has_upscaling_profile": "FALSE",
                  "hidden": "FALSE",
                  "image_info": "C:\\Path\\Image.png",
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
                  "overdrive": [],
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
                  "title": "Test Display Name",
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
              ]
            }
            """;

        // Act
        await _service.AddAsync(gameInfo);

        // Assert
        await _fileWriter.Received(1).WriteContentAsync(_amdGameDbFilePath, expected);
    }

    [Fact]
    public async Task AddAsync_NullLiteralDbFileContent_ThrowsException()
    {
        _readFileStreamCreator.Open(_amdGameDbFilePath).Returns(new MemoryStream(Encoding.UTF8.GetBytes("null")));
        var gameInfo = new AdrenalineGameInfo(Guid.Empty,
            "Test Display Name",
            @"C:\Path\Image.png",
            "Test-Command",
            @"C:\Path\Executabe.exe",
            true);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddAsync(gameInfo));

        Assert.Equal("Failed to parse Adrenaline data file.", ex.Message);
        await _fileWriter.Received(0).WriteContentAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task AddAsync_DbFileMissingGamesProperty_ThrowsException()
    {
        _readFileStreamCreator.Open(_amdGameDbFilePath).Returns(new MemoryStream(Encoding.UTF8.GetBytes("{}")));
        var gameInfo = new AdrenalineGameInfo(Guid.Empty,
            "Test Display Name",
            @"C:\Path\Image.png",
            "Test-Command",
            @"C:\Path\Executabe.exe",
            true);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddAsync(gameInfo));

        Assert.Equal("Failed to find games data in Adrenaline data file.", ex.Message);
        await _fileWriter.Received(0).WriteContentAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task RestartAdrenalineAsync_ProcessManagerReturnsValue_ReturnsThatValue()
    {
        _guidGenerator.NewGuid().Returns(Guid.NewGuid());
        _adrenalineProcessManager.RestartAsync().Returns(true);
        var gameInfo = new AdrenalineGameInfo(Guid.Empty,
            "Test Display Name",
            @"C:\Path\Image.png",
            "Test-Command",
            @"C:\Path\Executabe.exe",
            true);
        await _service.AddAsync(gameInfo);

        var actual = await _service.RestartAdrenalineAsync();

        Assert.True(actual);
        Assert.False(_service.IsRestartRequired);
    }

    [Fact]
    public async Task RestartAdrenalineAsync_ProcessManagerThrowsException_ReturnsFalse()
    {
        _adrenalineProcessManager.RestartAsync().ThrowsAsync<Exception>();

        var actual = await _service.RestartAdrenalineAsync();

        Assert.False(actual);
        Assert.Equal("Something went wrong while trying to restart Adrenaline.", _logger.LatestRecord.Message);
        Assert.Equal(LogLevel.Error, _logger.LatestRecord.Level);
    }

    [Fact]
    public void FileSystemWatcher_Changed_UpdatesGamesData()
    {
        var eventRaised = false;
        _service.GamesDataChanged += (_, _) => { eventRaised = true; };

        _fileSystemWatcher.Changed += Raise.Event<FileSystemEventHandler>(_service,
            new FileSystemEventArgs(WatcherChangeTypes.Changed, string.Empty, string.Empty));
        _timeProvider.Advance(TimeSpan.FromSeconds(1));

        Assert.True(eventRaised);
    }
}
