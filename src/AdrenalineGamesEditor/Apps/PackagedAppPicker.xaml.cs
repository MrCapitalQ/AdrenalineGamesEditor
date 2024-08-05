using CommunityToolkit.WinUI.Collections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MrCapitalQ.AdrenalineGamesEditor.Core;
using MrCapitalQ.AdrenalineGamesEditor.Core.Apps;
using System.Diagnostics.CodeAnalysis;
using Windows.Foundation.Collections;

namespace MrCapitalQ.AdrenalineGamesEditor.Apps;

[ExcludeFromCodeCoverage(Justification = ExcludeFromCoverageJustifications.RequiresUIThread)]
public sealed partial class PackagedAppPicker : ContentDialog
{
    private readonly SortDescription _displayNameSort = new(nameof(PackagedAppListing.DisplayName), SortDirection.Ascending);
    private readonly SortDescription _dateInstalledSort = new(nameof(PackagedAppListing.InstalledDate), SortDirection.Descending);
    private readonly SortDescription _idSort = new(nameof(PackagedAppListing.AppUserModelId), SortDirection.Ascending);
    private readonly IPackagedAppsService _packageService;
    private AdvancedCollectionView? _appsCollectionView;

    public PackagedAppPicker()
    {
        InitializeComponent();
        _packageService = App.Current.Services.GetRequiredService<IPackagedAppsService>();

        _ = LoadAsync();
    }

    public PackagedAppListing? SelectedApp => PackagedAppsPickerList.SelectedItem as PackagedAppListing;

    private async Task LoadAsync()
    {
        LoadingIndicator.Visibility = Visibility.Visible;
        PickerContent.Visibility = Visibility.Collapsed;
        EmptyListText.Visibility = Visibility.Collapsed;

        try
        {
            if (_appsCollectionView is not null)
                return;

            var apps = await _packageService.GetAllAsync();
            _appsCollectionView = new(apps.ToList(), true)
            {
                Filter = x =>
                {
                    if (x is not PackagedAppListing app)
                        return false;

                    return (!GamesOnlyFilterToggle.IsChecked || app.IsGame)
                        && app.DisplayName.Contains(SearchBox.Text, StringComparison.CurrentCultureIgnoreCase);
                }
            };
            _appsCollectionView.VectorChanged += AppsCollectionView_VectorChanged;

            UpdateSort();

            PackagedAppsPickerList.ItemsSource = _appsCollectionView;
        }
        finally
        {
            LoadingIndicator.Visibility = Visibility.Collapsed;
            PickerContent.Visibility = Visibility.Visible;

            // Hack to workaround AutosuggestBox not focusing.
            UpdateLayout();
            SearchBox.Focus(FocusState.Keyboard);
        }
    }

    private void UpdateSort()
    {
        if (_appsCollectionView is null)
            return;

        _appsCollectionView.SortDescriptions.Clear();

        if (SortByNameRadio.IsChecked)
        {
            _appsCollectionView.SortDescriptions.Add(_displayNameSort);
            _appsCollectionView.SortDescriptions.Add(_dateInstalledSort);
            _appsCollectionView.SortDescriptions.Add(_idSort);
        }
        else
        {
            _appsCollectionView.SortDescriptions.Add(_dateInstalledSort);
            _appsCollectionView.SortDescriptions.Add(_idSort);
        }

    }

    private void PackagedAppsPickerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        => IsPrimaryButtonEnabled = PackagedAppsPickerList.SelectedItem is not null;

    private void ToggleMenuFlyoutItem_Click(object sender, RoutedEventArgs e) => _appsCollectionView?.RefreshFilter();

    private void SortRadioMenuFlyoutItem_Click(object sender, RoutedEventArgs e) => UpdateSort();

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        => _appsCollectionView?.RefreshFilter();

    private void AppsCollectionView_VectorChanged(IObservableVector<object> sender, IVectorChangedEventArgs args)
        => EmptyListText.Visibility = sender.Any() ? Visibility.Collapsed : Visibility.Visible;
}
