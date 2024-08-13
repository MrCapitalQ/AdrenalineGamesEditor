using MrCapitalQ.AdrenalineGamesEditor.Core.Adrenaline;
using System.Diagnostics;

namespace MrCapitalQ.AdrenalineGamesEditor.Infrastructure.Adrenaline;

internal class AdrenalineProcessManager : IAdrenalineProcessManager
{
    private const string AdrenalinePath = @"C:\Program Files\AMD\CNext\CNext\RadeonSoftware.exe";
    private const string AdrenalineProcessName = "RadeonSoftware";

    public async Task<bool> RestartAsync()
    {
        var tasks = Process.GetProcessesByName(AdrenalineProcessName).Select(async x =>
        {
            var processPath = x.MainModule?.FileName;
            if (!AdrenalinePath.Equals(processPath, StringComparison.OrdinalIgnoreCase))
                return;

            x.Kill();
            await x.WaitForExitAsync();
        });

        await Task.WhenAll(tasks);

        Process.Start(AdrenalinePath);

        var timeout = DateTime.Now.Add(TimeSpan.FromSeconds(30));
        while (!Process.GetProcessesByName(AdrenalineProcessName).Any(x => x.MainWindowHandle != nint.Zero))
        {
            if (DateTime.Now > timeout)
                return false;

            await Task.Delay(500);
        }

        return true;
    }
}
