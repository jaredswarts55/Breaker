namespace Breaker.Core.Models.Settings
{
    public class UserSettings
    {
        public string InstallDirectory { get; set; }
        public string BuildDirectory { get; set; }
        public int Version { get; set; } = 1;
        public QuickRunSetting[] QuickRuns = new QuickRunSetting[0];

        public string[] EnabledCommands = new string[0];

        public static UserSettings Defaults => new UserSettings
                                               {
                                                   InstallDirectory = @"C:\opt\Breaker",
                                                   BuildDirectory = @"C:\src\personal\Breaker\src\Breaker\bin\Debug\net472",
                                                   Version = 1,
                                                   QuickRuns = new[]
                                                               {
                                                                   new QuickRunSetting
                                                                   {
                                                                       Name = "Powershell", Path = @"powershell.exe", WorkingDirectory = @"c:\src\",
                                                                       Arguments = "-NoExit -command \"& {Set-Location c:\\src\\}\" "
                                                                   },
                                                                   new QuickRunSetting
                                                                   {
                                                                       Name = "Reload Breaker", Path = @"powershell.exe", WorkingDirectory = @"c:\opt\Breaker",
                                                                       Arguments = "-noprofile -command \"& { .\\update.ps1 }\" "
                                                                   },
                                                                   new QuickRunSetting
                                                                   {Name = "Cmder", Path = @"C:\opt\cmder\Cmder.exe", WorkingDirectory = @"c:\src\", Arguments = null},
                                                                   new QuickRunSetting
                                                                   {
                                                                       Name = "Shortcuts", Path = @"C:\Program Files\AutoHotkey\AutoHotkey.exe", WorkingDirectory = null,
                                                                       Arguments = @"C:\opt\Shortcuts.ahk"
                                                                   }
                                                               },
                                                   EnabledCommands = new[]
                                                                     {
                                                                         "cmd",
                                                                         "run",
                                                                         "doc",
                                                                         "g",
                                                                         "gl",
                                                                         "guid"
                                                                     }
                                               };
    }
}