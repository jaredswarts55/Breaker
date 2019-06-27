using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NullFight;

using static NullFight.FunctionalExtensions;
namespace Breaker.Core.Utilities
{
    public class WindowsUtilities
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ShowWindowAsync(IntPtr windowHandle, int nCmdShow);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ShowWindow(IntPtr hWnd, uint msg);
        private const uint RestoreWindowMessage = 0x09;

        public static Option<IntPtr> GetWindowHandleByWindowTitle(string windowName)
        {
            var windowHandle = IntPtr.Zero;
            foreach (var pList in Process.GetProcesses())
            {
                if (pList.MainWindowTitle.Contains(windowName))
                {
                    windowHandle = pList.MainWindowHandle;
                    break;
                }
            }
            if (windowHandle == IntPtr.Zero)
                return None();
            return Some(windowHandle);
        }

        public static Option<IntPtr> GetWindowHandleByProcessName(string processName)
        {
            var windowHandle = IntPtr.Zero;
            foreach (var pList in Process.GetProcesses())
            {
                if (pList.ProcessName.Equals(processName, StringComparison.CurrentCultureIgnoreCase))
                {
                    windowHandle = pList.MainWindowHandle;
                    break;
                }
            }
            if (windowHandle == IntPtr.Zero)
                return None();
            return Some(windowHandle);
        }

        public static void RestoreWindow(IntPtr windowHandle)
        {
            ShowWindow(windowHandle, RestoreWindowMessage);
        }
    }
    public enum ShowWindowCommands
    {
        /// <summary>
        /// Hides the window and activates another window.
        /// </summary>
        Hide = 0,
        /// <summary>
        /// Activates and displays a window. If the window is minimized or
        /// maximized, the system restores it to its original size and position.
        /// An application should specify this flag when displaying the window
        /// for the first time.
        /// </summary>
        Normal = 1,
        /// <summary>
        /// Activates the window and displays it as a minimized window.
        /// </summary>
        ShowMinimized = 2,
        /// <summary>
        /// Maximizes the specified window.
        /// </summary>
        Maximize = 3, // is this the right value?
                      /// <summary>
                      /// Activates the window and displays it as a maximized window.
                      /// </summary>      
        ShowMaximized = 3,
        /// <summary>
        /// Displays a window in its most recent size and position. This value
        /// is similar to <see cref="Win32.ShowWindowCommand.Normal"/>, except
        /// the window is not activated.
        /// </summary>
        ShowNoActivate = 4,
        /// <summary>
        /// Activates the window and displays it in its current size and position.
        /// </summary>
        Show = 5,
        /// <summary>
        /// Minimizes the specified window and activates the next top-level
        /// window in the Z order.
        /// </summary>
        Minimize = 6,
        /// <summary>
        /// Displays the window as a minimized window. This value is similar to
        /// <see cref="Win32.ShowWindowCommand.ShowMinimized"/>, except the
        /// window is not activated.
        /// </summary>
        ShowMinNoActive = 7,
        /// <summary>
        /// Displays the window in its current size and position. This value is
        /// similar to <see cref="Win32.ShowWindowCommand.Show"/>, except the
        /// window is not activated.
        /// </summary>
        ShowNA = 8,
        /// <summary>
        /// Activates and displays the window. If the window is minimized or
        /// maximized, the system restores it to its original size and position.
        /// An application should specify this flag when restoring a minimized window.
        /// </summary>
        Restore = 9,
        /// <summary>
        /// Sets the show state based on the SW_* value specified in the
        /// STARTUPINFO structure passed to the CreateProcess function by the
        /// program that started the application.
        /// </summary>
        ShowDefault = 10,
        /// <summary>
        ///  <b>Windows 2000/XP:</b> Minimizes a window, even if the thread
        /// that owns the window is not responding. This flag should only be
        /// used when minimizing windows from a different thread.
        /// </summary>
        ForceMinimize = 11
    }

}
