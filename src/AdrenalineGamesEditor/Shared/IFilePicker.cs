
using Windows.Storage.Pickers;

namespace MrCapitalQ.AdrenalineGamesEditor.Shared;

public interface IFilePicker
{
    Task<string?> PickSingleFileAsync(PickerLocationId? suggestedStartLocation = null, IEnumerable<string>? fileTypeFilters = null);
}