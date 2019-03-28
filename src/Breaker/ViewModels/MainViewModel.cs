using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Breaker.Core.Events;
using Breaker.Core.Listings.Requests;
using Breaker.Core.Models;
using Breaker.Events;
using Breaker.Utilities;
using Breaker.ViewModels.SubModels;
using MediatR;
using NullFight;

namespace Breaker.ViewModels
{
    using Caliburn.Micro;
    using PropertyChanged;

    /// <summary>
    /// Main view model for the application
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class MainViewModel : ViewAware, IHandle<ShowHotkeyPressedEvent>, IHandle<ShowResultEvent>, IHandle<ClearSearchEvent>
    {
        /// <summary>
        /// Stores the events aggregator
        /// </summary>
        private readonly IEventAggregator _eventAggregator;

        private readonly IMediator _mediator;

        private string _lastCompletedSearchString = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class
        /// </summary>
        /// <param name="eventAggregator">The events</param>
        public MainViewModel(IEventAggregator eventAggregator, IMediator mediator)
        {
            _eventAggregator = eventAggregator;
            _mediator = mediator;
            _eventAggregator.Subscribe(this);
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
            if (context.Key == Key.Enter)
            {
                if (SelectedFoundItem == null)
                    return true;
                if (SelectedFoundItem is SlashCommand slashCommand)
                {
                    await RunSlashCommand(slashCommand);
                    return true;
                }
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
            }
            else if (context.Key == Key.Down)
            {
                SelectedFoundItem = FoundItems[Math.Min(FoundItems.Count - 1, Array.IndexOf(FoundItems.ToArray(), SelectedFoundItem) + 1)];
                return true;
            }
            else if (context.Key == Key.Up)
            {
                SelectedFoundItem = FoundItems[Math.Max(0, Array.IndexOf(FoundItems.ToArray(), SelectedFoundItem) - 1)];
                return true;
            }
            else if (context.Key == Key.Escape)
            {
                ClearSearchAndHide();
                return true;
            }
            return false;
        }

        private async Task RunSlashCommand(SlashCommand slashCommand, bool closeWindow = true)
        {
            var (commandText, _) = ParseCommandText(SearchText);
            if (closeWindow)
                ClearSearchAndHide();
            await _mediator.Send(slashCommand.CreateRequest(commandText));
        }
        public async void CopyItem(SearchEntry eventArgs)
        {
            if (eventArgs is SlashCommand slashCommand)
            {
                var toClipboard = slashCommand.CurrentResult;
                if(slashCommand.ProcessResultForClipboard != null)
                    toClipboard = slashCommand.ProcessResultForClipboard(toClipboard);
                Clipboard.SetText(toClipboard);
            }
            else
            {
                Clipboard.SetText(eventArgs.Path);
            }
        }

        public async void UpdateSearch(KeyEventArgs context)
        {
            if (string.IsNullOrEmpty(SearchText) || SearchText.Length <= 0)
            {
                ClearFoundItemsAndHeader();
                return;
            }
            if (_lastCompletedSearchString == SearchText)
                return;

            SearchEntry[] foundItems;
            if (SearchText.StartsWith("/"))
            {
                var (commandText, searchText) = ParseCommandText(SearchText);
                var commandName = searchText.Substring(1);
                var slashCommands = SearchEntries.Commands.Where(x => x.Name.StartsWith(commandName)).ToArray();
                foreach (var items in slashCommands)
                {
                    items.Display = string.Format(items.DisplayTemplate, commandText);
                }

                if (slashCommands.Any())
                {
                    if (slashCommands.Length > 1 || !slashCommands[0].Name.Equals(commandName, StringComparison.CurrentCultureIgnoreCase))
                        ClearFoundItemsAndHeader();
                    if (slashCommands.Length == 1 && slashCommands[0].RunOnType)
                        await RunSlashCommand(slashCommands[0], false);
                }

                foundItems = slashCommands;
            }
            else
            {
                ClearFoundItemsAndHeader();
                foundItems = await _mediator.Send(new GetSearchListingsRequest { SearchText = SearchText }).Expect("Could not retrieve search result");
            }
            _lastCompletedSearchString = SearchText;
            FoundItems.Clear();
            foreach (var item in foundItems)
                FoundItems.Add(item);

            if (FoundItems.Any())
            {
                SelectedFoundItem = FoundItems[0];
            }
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
