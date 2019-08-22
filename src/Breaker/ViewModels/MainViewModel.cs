using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Breaker.Core.Events;
using Breaker.Core.Listings.Requests;
using Breaker.Core.Models;
using Breaker.Core.Models.Settings;
using Breaker.Core.Services;
using Breaker.Core.Services.Base;
using Breaker.Events;
using Breaker.Utilities;
using Breaker.ViewModels.SubModels;
using MediatR;
using NullFight;
using ReactiveUI;

namespace Breaker.ViewModels
{
    using Caliburn.Micro;
    using PropertyChanged;

    /// <summary>
    /// Main view model for the application
    /// </summary>
    public class MainViewModel : ViewAware, IHandle<ShowHotkeyPressedEvent>, IHandle<ShowResultEvent>, IHandle<ClearSearchEvent>
    {
        /// <summary>
        /// Stores the events aggregator
        /// </summary>
        private readonly IEventAggregator _eventAggregator;

        private readonly IMediator _mediator;

        private string _lastCompletedSearchString = string.Empty;
        private UserSettings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class
        /// </summary>
        /// <param name="eventAggregator">The events</param>
        public MainViewModel(IEventAggregator eventAggregator, IMediator mediator, IUserSettingsService userSettingsService)
        {
            _eventAggregator = eventAggregator;
            _mediator = mediator;
            _eventAggregator.Subscribe(this);
            _settings = userSettingsService.GetUserSettings();

            this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromSeconds(0.10), RxApp.TaskpoolScheduler)
                .Select(query => query?.Trim())
                .DistinctUntilChanged()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => UpdateSearch());
        }
        public string SearchText { get; set; }

        public string Header { get; set; } = "Search";
        public ObservableCollection<SearchEntry> FoundItems { get; set; } = new ObservableCollection<SearchEntry>();
        public SearchEntry[] AllItems { get; set; } = SearchEntries.AllEntries;
        public SearchEntry SelectedFoundItem { get; set; }

        protected override void OnViewLoaded(object view)
        {
            this.SetFocus(() => SearchText);
            base.OnViewLoaded(view);
        }

        public async void SelectionChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count >= 1)
            {
                SelectedFoundItem = e.AddedItems[0] as SearchEntry;
            }
        }

        public async Task<bool> HandleControlKeys(KeyEventArgs context)
        {
            switch (context.Key)
            {
                case Key.Enter when SelectedFoundItem == null:
                    return true;
                case Key.Enter when SelectedFoundItem is SlashCommand slashCommand:
                    await RunSlashCommand(slashCommand);
                    return true;
                case Key.Enter:
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            Arguments = SelectedFoundItem.Arguments,
                            FileName = SelectedFoundItem.Path,
                        }
                    };
                    if (!string.IsNullOrWhiteSpace(SelectedFoundItem.WorkingDirectory))
                        process.StartInfo.WorkingDirectory = SelectedFoundItem.WorkingDirectory;
                    ClearSearchAndHide();
                    process.Start();
                    return true;
                case Key.Down:
                    SelectedFoundItem = FoundItems[Math.Min(FoundItems.Count - 1, Array.IndexOf(FoundItems.ToArray(), SelectedFoundItem) + 1)];
                    return true;
                case Key.Up:
                    SelectedFoundItem = FoundItems[Math.Max(0, Array.IndexOf(FoundItems.ToArray(), SelectedFoundItem) - 1)];
                    return true;
                case Key.Escape:
                    ClearSearchAndHide();
                    return true;
                case Key.PageUp:
                    var windowManager = new WindowManager();
                    windowManager.ShowDialog(new ScratchpadViewModel());
                    return true;
                default:
                    return false;
            }
        }

        private async Task RunSlashCommand(SlashCommand slashCommand, bool closeWindow = true)
        {
            var (commandText, _) = ParseCommandText(SearchText);
            if (closeWindow)
                ClearSearchAndHide();
            await _mediator.Send(slashCommand.CreateRequest(commandText, _settings));
        }
        public async void CopyItem(SearchEntry eventArgs)
        {
            if (eventArgs is SlashCommand slashCommand)
            {
                var toClipboard = slashCommand.CurrentResult;
                if (slashCommand.ProcessResultForClipboard != null)
                    toClipboard = slashCommand.ProcessResultForClipboard(toClipboard);
                Clipboard.SetText(toClipboard);
            }
            else
            {
                Clipboard.SetText(eventArgs.Path);
            }
        }

        public async void UpdateSearch()
        {
            if (string.IsNullOrEmpty(SearchText) || SearchText.Length <= 0)
            {
                ClearFoundItemsAndHeader();
                return;
            }
#pragma warning disable 4014
            Task.Run(() => InternalSearch(CancellationToken.None));
#pragma warning restore 4014
        }

        private async Task InternalSearch(CancellationToken cancellationToken)
        {
            SearchEntry[] foundItems;
            if (SearchText.StartsWith("/"))
            {
                var (commandText, searchText) = ParseCommandText(SearchText);
                var commandName = searchText.Substring(1);
                SlashCommand[] slashCommands = SearchEntries.Commands.Where(x => x.Name.StartsWith(commandName)).ToArray();
                foreach (var items in slashCommands)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;
                    items.Display = string.Format(items.DisplayTemplate, commandText);
                }

                if (slashCommands.Any())
                {
                    if (slashCommands.Length > 1 || !slashCommands[0].Name.Equals(commandName, StringComparison.CurrentCultureIgnoreCase))
                        Application.Current.Dispatcher.Invoke(ClearFoundItemsAndHeader);
                    if (slashCommands.Length == 1 && slashCommands[0].RunOnType)
                        await RunSlashCommand(slashCommands[0], false);
                }

                foundItems = slashCommands;
            }
            else
            {
                //Application.Current.Dispatcher.Invoke(ClearFoundItemsAndHeader);
                foundItems = await _mediator.Send(new GetSearchListingsRequest { SearchText = SearchText }, cancellationToken).Expect("Could not retrieve search result");
            }
            if (cancellationToken.IsCancellationRequested)
                return;

            _lastCompletedSearchString = SearchText;
            Application.Current.Dispatcher.Invoke(() =>
            {
                FoundItems = new ObservableCollection<SearchEntry>(foundItems);

                if (foundItems.Length > 0)
                {
                    SelectedFoundItem = FoundItems[0];
                }
            });
        }

        private (string commandText, string searchText) ParseCommandText(string searchTextInput)
        {
            var commandText = string.Empty;
            var searchText = searchTextInput.ToLower();
            var firstSpace = searchTextInput.IndexOf(' ');
            if (firstSpace >= 2)
            {
                commandText = searchTextInput.Substring(firstSpace + 1);
                searchText = searchText.Substring(0, firstSpace);
            }

            return (commandText, searchText);
        }

        private void ClearSearchAndHide()
        {
            ClearSearch();
            _eventAggregator.PublishOnUIThread(new ToggleWindowVisibilityEvent { IsHidden = true });
        }

        private void ClearSearch()
        {
            SearchText = string.Empty;
            ClearFoundItemsAndHeader();
        }

        private void ClearFoundItemsAndHeader()
        {
            SelectedFoundItem = null;
            FoundItems.Clear();
            Header = "Search";
        }

        public void Handle(ShowHotkeyPressedEvent message)
        {
            this.SetFocus(() => SearchText);
        }

        public void Handle(ShowResultEvent message)
        {
            Header = message.Header;
            if (SelectedFoundItem != null && !string.IsNullOrWhiteSpace(message.Result) && SelectedFoundItem is SlashCommand slashCommand)
            {
                var (commandText, _) = ParseCommandText(SearchText);
                SelectedFoundItem.Display = string.Format(slashCommand.DisplayTemplate, commandText) + " = " + message.Result;
                slashCommand.CurrentResult = message.Result;
            }
        }

        public void Handle(ClearSearchEvent message)
        {
            ClearSearch();
        }
    }
}
