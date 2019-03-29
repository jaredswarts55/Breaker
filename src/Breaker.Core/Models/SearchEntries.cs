using System;
using System.IO;
using System.Linq;
using System.Web;
using Breaker.Core.Commands.Requests;
using Breaker.Core.Listings.Requests;
using Breaker.Core.Models;

namespace Breaker.ViewModels.SubModels
{
    public class SearchEntries
    {
        public static SearchEntry[] AllEntries = new (string name, string path, string workingDirectory, string arguments)[]
                                                 {
                                                     ("Powershell", @"powershell.exe", @"c:\src\", "-NoExit -command \"& {Set-Location c:\\src\\}\" "),
                                                     ("Reload Breaker", @"powershell.exe", @"c:\opt\Breaker", "-noprofile -command \"& { .\\update.ps1 }\" "),
                                                     ("Cmder", @"C:\opt\cmder\Cmder.exe", @"c:\src\", null),
                                                     ("Shortcuts", @"C:\Program Files\AutoHotkey\AutoHotkey.exe", null, @"C:\opt\Shortcuts.ahk")
                                                 }
                                                 .Select(
                                                     x => new SearchEntry
                                                     {
                                                         Name = x.name,
                                                         Path = x.path,
                                                         WorkingDirectory = x.workingDirectory,
                                                         Arguments = x.arguments,
                                                         Priority = 1
                                                     }
                                                 )
                                                 .ToArray();

        public static SlashCommand[] Commands =
        {
            new SlashCommand
            {
                Name = "cmd",
                DisplayTemplate = "Run Command '{0}' in CMD",
                CreateRequest = s => new ExecuteRunCommandRequest {CommandText = s, Hide = false}
            },
            new SlashCommand
            {
                Name = "run",
                DisplayTemplate = "Run Command '{0}'",
                CreateRequest = s => new ExecuteRunCommandRequest {CommandText = s, Hide = true}
            },
            new SlashCommand
            {
                Name = "doc",
                DisplayTemplate = "Search Docs for '{0}'",
                CreateRequest = s => new ExecuteSearchDocsRequest {CommandText = $"start dash-plugin://query={Uri.EscapeUriString(s)}", Hide = true}
            },
            new SlashCommand
            {
                Name = "g",
                DisplayTemplate = "Google Search '{0}'",
                CreateRequest = s => new ExecuteGoogleSearchRequest {SearchText = s }
            },
            new SlashCommand
            {
                Name = "guid",
                DisplayTemplate = "Copy a New Guid to Clipboard",
                CreateRequest = s => new ExecuteCopyGuidRequest()
            },
            new SlashCommand
            {
                Name = "js",
                DisplayTemplate = "Javascript '{0}'",
                RunOnType = true,
                ProcessResultForClipboard = (string s) => s.Trim('"', ' '),
                CreateRequest = s => new ExecuteJavascriptRequest {Javascript = s }
            }
        };

        public static void LoadStartMenuItems(Func<string, (string path, string arguments)> getShortcutTarget)
        {
            var userStartMenu = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
            var commonStartMenu = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
            var userStartMenuLinks = Directory.GetFiles(userStartMenu, "*.lnk", SearchOption.AllDirectories);
            var commonStartMenuLinks = Directory.GetFiles(commonStartMenu, "*.lnk", SearchOption.AllDirectories);
            AllEntries = userStartMenuLinks.Union(commonStartMenuLinks)
                                           .Select(
                                               x =>
                                               {
                                                   var (targetPath, arguments) = getShortcutTarget(x);
                                                   var executableName = Path.GetFileName(x);
                                                   var entry = new
                                                   {
                                                       shortcutPath = x,
                                                       target = targetPath,
                                                       arguments,
                                                       fileName = Path.GetFileNameWithoutExtension(x),
                                                       executableName
                                                   };
                                                   return entry;
                                               }
                                           ).Select(
                                               x => new SearchEntry
                                               {
                                                   Name = $"{x.fileName}",
                                                   Path = x.shortcutPath,
                                                   Keywords = new[] { x.fileName },
                                                   Display = $"{x.target} {x.arguments}".Trim(),
                                                   Terms = new[] { x.fileName, x.executableName }
                                               }
                                           ).Union(AllEntries).ToArray();
            var test = AllEntries.Where(x => x.Name.Contains("owershell"));
        }
    }
}