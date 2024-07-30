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
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly FakeLogger<AdrenalineGamesDataService> _logger = new();

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
            _timeProvider,
            _logger);

        Assert.Empty(service.GamesData);
        Assert.Equal("Failed to update Adrenaline games data.", _logger.LatestRecord.Message);
        Assert.Equal(LogLevel.Error, _logger.LatestRecord.Level);
    }

    [Fact]
    public void FileSystemWatcher_Changed_UpdatesGamesData()
    {
        var json = "{ \"games\": [] }";
        _readFileStreamCreator.Open(Arg.Any<string>())
            .Returns(new MemoryStream(Encoding.UTF8.GetBytes(json)), new MemoryStream(Encoding.UTF8.GetBytes(json)));
        var service = new AdrenalineGamesDataService(_fileSystemWatcher,
            _readFileStreamCreator,
            _timeProvider,
            _logger);
        var eventRaised = false;
        service.GamesDataChanged += (_, _) =>
        {
            eventRaised = true;
        };

        _fileSystemWatcher.Changed += Raise.Event<FileSystemEventHandler>(service,
            new FileSystemEventArgs(WatcherChangeTypes.Changed, string.Empty, string.Empty));
        _timeProvider.Advance(TimeSpan.FromSeconds(1));

        Assert.True(eventRaised);
    }
}
