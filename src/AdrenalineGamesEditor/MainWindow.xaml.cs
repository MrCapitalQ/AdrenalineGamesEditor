using Microsoft.UI.Xaml;
using MrCapitalQ.AdrenalineGamesEditor.Core;
using System.Diagnostics.CodeAnalysis;

namespace MrCapitalQ.AdrenalineGamesEditor;

[ExcludeFromCodeCoverage(Justification = ExcludeFromCoverageJustifications.RequiresUIThread)]
public sealed partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();
}
