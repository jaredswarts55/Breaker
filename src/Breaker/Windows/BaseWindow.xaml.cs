using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Breaker.Core.Events;
using Breaker.Core.Utilities;
using Breaker.Events;
using Breaker.Invoke;
using Breaker.ViewModels;
using Caliburn.Micro;

namespace Breaker.Windows
{
    /// <summary>
    ///     Interaction logic for BaseWindow.xaml
    /// </summary>
    public partial class BaseWindow : IHandle<ToggleWindowVisibilityEvent>
    {
        private readonly IEventAggregator _eventAggregator;
        private static Window _created = null;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseWindow" /> class
        /// </summary>
        public BaseWindow(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            InitializeComponent();
            _eventAggregator.Subscribe(this);
            _handle = new WindowInteropHelper(this).Handle;
        }

        /// <summary>
        /// Id's to disambiguate multiple hotkey registrations
        /// </summary>
        uint _hotKey1;

        private HotKeyHelper _hotKeys;
        private IntPtr _handle;

        // --------------------------------------------------------------------------
        /// <summary>
        /// Once we have a window handle, register for hot keys
        /// </summary>
        // --------------------------------------------------------------------------
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            _hotKeys = new HotKeyHelper(this, HandleHotKey);
            _hotKey1 = _hotKeys.ListenForHotKey(System.Windows.Forms.Keys.Space, HotKeyModifiers.Shift | HotKeyModifiers.Control);
        }

        // --------------------------------------------------------------------------
        /// <summary>
        /// Hotkey handler.  The keyId is the return value from ListenForHotKey()
        /// </summary>
        // --------------------------------------------------------------------------
        void HandleHotKey(int keyId)
        {
            if (keyId == _hotKey1)
            {
                Visibility = Visibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;
                WindowState = WindowState.Normal;
                if (Visibility == Visibility.Visible)
                {
                    _eventAggregator.PublishOnUIThread(new ShowHotkeyPressedEvent());
                    WindowsUtilities.ShowWindowAsync(_handle, (int)ShowWindowCommands.ShowDefault);
                    WindowsUtilities.ShowWindowAsync(_handle, (int)ShowWindowCommands.Show);
                    WindowsUtilities.SetForegroundWindow(_handle);
                }
            }
        }

        public void Handle(ToggleWindowVisibilityEvent message)
        {
            Visibility = message.IsHidden ? Visibility.Hidden : Visibility.Visible;
        }

        private void MetroWindow_Deactivated(object sender, EventArgs e)
        {
            Visibility = Visibility.Hidden;
            _eventAggregator.PublishOnUIThread(new ClearSearchEvent());
        }
    }
}