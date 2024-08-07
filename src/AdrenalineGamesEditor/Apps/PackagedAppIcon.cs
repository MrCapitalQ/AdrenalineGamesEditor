using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Windows.ApplicationModel.Resources;
using MrCapitalQ.AdrenalineGamesEditor.Core;
using MrCapitalQ.AdrenalineGamesEditor.Core.Apps;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace MrCapitalQ.AdrenalineGamesEditor.Apps;

[ExcludeFromCodeCoverage(Justification = ExcludeFromCoverageJustifications.RequiresUIThread)]
public sealed class PackagedAppIcon : Control
{
    public static readonly DependencyProperty PackageProperty =
       DependencyProperty.Register(nameof(Package),
          typeof(IPackagedAppIconInfo),
          typeof(PackagedAppIcon),
          new PropertyMetadata(null));
    public static readonly DependencyProperty IconProperty =
       DependencyProperty.Register(nameof(Icon),
          typeof(ImageSource),
          typeof(PackagedAppIcon),
          new PropertyMetadata(null));

    private ResourceMap? _resourceMap;
    private ResourceContext? _resourceContext;

    public PackagedAppIcon()
    {
        DefaultStyleKey = typeof(PackagedAppIcon);
        ActualThemeChanged += PackagedAppIcon_ActualThemeChanged;
        SizeChanged += PackagedAppIcon_SizeChanged;
    }

    public IPackagedAppIconInfo? Package
    {
        get => GetValue(PackageProperty) as IPackagedAppIconInfo;
        set
        {
            SetValue(PackageProperty, value);
            InitResources();
            UpdateIconPath();
        }
    }

    public ImageSource? Icon
    {
        get => GetValue(IconProperty) as ImageSource;
        private set => SetValue(IconProperty, value);
    }

    private void InitResources()
    {
        _resourceContext = null;
        _resourceMap = null;

        if (Package is null)
            return;

        var priPath = Path.Combine(Package.InstalledPath, "resources.pri");
        if (!Path.Exists(priPath))
            return;

        var resourceManager = new ResourceManager(priPath);
        _resourceMap = resourceManager.MainResourceMap.TryGetSubtree("Files");
        _resourceContext = resourceManager.CreateResourceContext();
    }

    private void UpdateIconPath()
    {
        if (Package is null || string.IsNullOrEmpty(Package.Square44x44Logo))
        {
            Icon = null;
            return;
        }

        var nonQualifiedIconUri = new Uri(Path.Combine(Package.InstalledPath, Package.Square44x44Logo));

        var context = GetResourceContext();
        if (_resourceMap is null || context is null)
        {
            Icon = new BitmapImage(nonQualifiedIconUri);
            return;
        }

        var path = _resourceMap.GetValue(Package.Square44x44Logo, context).ValueAsString;

        var uri = !string.IsNullOrEmpty(path) ? new Uri(path) : nonQualifiedIconUri;
        Icon = new BitmapImage(uri);
    }

    private ResourceContext? GetResourceContext()
    {
        if (_resourceContext is not null)
        {
            var scale = (XamlRoot?.RasterizationScale ?? 1) * 100;
            var isLightMode = RequestedTheme == ElementTheme.Light
                || RequestedTheme == ElementTheme.Default && App.Current.RequestedTheme == ApplicationTheme.Light;
            var targetSize = Math.Min(ActualWidth, ActualHeight);

            _resourceContext.QualifierValues[KnownResourceQualifierName.Scale] = scale.ToString(CultureInfo.InvariantCulture);
            _resourceContext.QualifierValues[KnownResourceQualifierName.Theme] = isLightMode ? "light" : "dark";
            _resourceContext.QualifierValues["AlternateForm"] = isLightMode ? "lightunplated" : "unplated";
            _resourceContext.QualifierValues[KnownResourceQualifierName.TargetSize] = targetSize.ToString(CultureInfo.InvariantCulture);
        }

        return _resourceContext;
    }

    private void PackagedAppIcon_ActualThemeChanged(FrameworkElement sender, object args) => UpdateIconPath();

    private void PackagedAppIcon_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateIconPath();
}
