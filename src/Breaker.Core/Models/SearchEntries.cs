using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading.Tasks;
using Breaker.Core.Commands.Requests;
using Breaker.Core.Listings.Requests;
using Breaker.Core.Models;
using Breaker.Core.Models.Settings;
using Breaker.Core.Services.Base;
using Jint.Parser.Ast;

namespace Breaker.ViewModels.SubModels
{
    public class SearchEntries
    {
        public static SearchEntry[] AllEntries = new SearchEntry[0];
        public static Dictionary<string, List<SearchEntry>> IndexedLookupTable = new Dictionary<string, List<SearchEntry>>();

        public static SlashCommand[] Commands =
        {
            new SlashCommand
            {
                Name = "cmd",
                DisplayTemplate = "Run Command '{0}' in CMD",
                CreateRequest = (s, userSettings) => new ExecuteRunCommandRequest {CommandText = s, Hide = false}
            },
            new SlashCommand
            {
                Name = "run",
                DisplayTemplate = "Run Command '{0}'",
                CreateRequest = (s, userSettings) => new ExecuteRunCommandRequest {CommandText = s, Hide = true}
            },
            new SlashCommand
            {
                Name = "settings",
                DisplayTemplate = "Open Path To User Settings",
                CreateRequest = (s, userSettings) => new ExecuteRunCommandRequest {CommandText = $"start {Path.GetDirectoryName(userSettings.UserSettingsDirectory)}", Hide = true}
            },
            new SlashCommand
            {
                Name = "kill",
                DisplayTemplate = "Kill Process '{0}'",
                CreateRequest = (s, userSettings) => new ExecuteRunCommandRequest {CommandText = $"taskkill /f /im {s}", Hide = true},
                AutoComplete = async (s, command) => {
                    var processes = await command.GetOrCreateCache("processes", () => Task.FromResult(Process.GetProcesses().Select(x => $"{x.ProcessName}.exe").GroupBy(x => x).Select(x => x.First())), 5);
                    return processes.Where(x => x.ToLower().Contains(s.ToLower())).ToArray();
                }
},
            new SlashCommand
            {
                Name = "doc",
                DisplayTemplate = "Search Docs for '{0}'",
                CreateRequest = (s, userSettings) => new ExecuteSearchDocsRequest {CommandText = $"start dash-plugin://query={Uri.EscapeUriString(s)}", Hide = true}
            },
            new SlashCommand
            {
                Name = "g",
                DisplayTemplate = "Google Search '{0}'",
                CreateRequest = (s, userSettings) => new ExecuteGoogleSearchRequest {SearchText = s}
            },
            new SlashCommand
            {
                Name = "gl",
                DisplayTemplate = "Google Feeling Lucky Search '{0}'",
                CreateRequest = (s, userSettings) => new ExecuteGoogleSearchRequest {SearchText = s, FeelingLucky = true}
            },
            new SlashCommand
            {
                Name = "guid",
                DisplayTemplate = "Copy a New Guid to Clipboard",
                CreateRequest = (s, userSettings) => new ExecuteCopyGuidRequest()
            },

            new SlashCommand
            {
                Name = "js",
                DisplayTemplate = "Javascript '{0}'",
                RunOnType = true,
                ProcessResultForClipboard = s => s.Trim('"', ' '),
                CreateRequest = (s, userSettings) => new ExecuteJavascriptRequest {Javascript = s}
            }
        };

        public static void InitializeSearch(Func<string, (string path, string arguments)> getShortcutTarget, IUserSettingsService userSettingsService)
        {
            var userSettings = userSettingsService.GetUserSettings();
            var userStartMenu = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
            var commonStartMenu = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
            var userStartMenuLinks = Directory.GetFiles(userStartMenu, "*.lnk", SearchOption.AllDirectories);
            var commonStartMenuLinks = Directory.GetFiles(commonStartMenu, "*.lnk", SearchOption.AllDirectories);
            Commands = Commands.Where(x => !userSettings.DisabledCommands.Any(y => string.Equals(y, x.Name, StringComparison.CurrentCultureIgnoreCase))).ToArray();

            AllEntries = userSettings.QuickRuns.Select(
                x => new SearchEntry
                {
                    Path = x.Path,
                    Arguments = x.Arguments,
                    Name = x.Name,
                    WorkingDirectory = x.WorkingDirectory,
                    Priority = x.Priority ?? 1
                }
            ).ToArray();
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

            foreach (var entry in AllEntries)
            {
                var terms = entry.Name
                                 .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                 .Concat(entry.Terms)
                                 .Select(x => x.Trim().ToLower())
                                 .Distinct()
                                 .Where(x => !string.IsNullOrWhiteSpace(x));
                foreach (var term in terms)
                {
                    if (IndexedLookupTable.TryGetValue(term, out var listForFullTerm))
                        listForFullTerm.Add(entry);
                    else
                        IndexedLookupTable[term] = new List<SearchEntry> { entry };

                    for (var i = 0; i < term.Length; i++)
                    {
                        var key = term.Remove(i, 1);
                        if (IndexedLookupTable.TryGetValue(key, out var list))
                            list.Add(entry);
                        else
                            IndexedLookupTable[key] = new List<SearchEntry> { entry };

                        if (i > 0)
                        {
                            var substringKey = term.Substring(0, i);
                            if (IndexedLookupTable.TryGetValue(substringKey, out var substringList))
                                substringList.Add(entry);
                            else
                                IndexedLookupTable[substringKey] = new List<SearchEntry> { entry };
                        }
                    }
                }
            }
        }

    }
}