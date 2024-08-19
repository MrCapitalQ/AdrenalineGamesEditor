using System.Diagnostics.CodeAnalysis;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace MrCapitalQ.AdrenalineGamesEditor.Shared;

[ExcludeFromCodeCoverage]
internal class FilePicker : IFilePicker
{
    public async Task<string?> PickSingleFileAsync(PickerLocationId? suggestedStartLocation = null,
        IEnumerable<string>? fileTypeFilters = null)
    {
        var openPicker = new FileOpenPicker();
        InitializeWithWindow.Initialize(openPicker, WindowNative.GetWindowHandle(App.Current.Window));

        openPicker.ViewMode = PickerViewMode.Thumbnail;

        if (suggestedStartLocation is not null)
            openPicker.SuggestedStartLocation = suggestedStartLocation.Value;

        foreach (var fileTypeFilter in fileTypeFilters ?? [])
        {
            openPicker.FileTypeFilter.Add(fileTypeFilter);
        }

        var file = await openPicker.PickSingleFileAsync();
        return file?.Path;
    }
}
