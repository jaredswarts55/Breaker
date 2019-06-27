using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NullFight;

namespace Breaker.Core.Utilities.Win32
{
    public class Win32
    {
        [DllImport("user32.dll")]
        static extern bool AllowSetForegroundWindow(int dwProcessId);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool LockSetForegroundWindow(uint uLockCode);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SystemParametersInfo(SPI uiAction, uint uiParam, ref uint pvParam, SPIF fWinIni);

        [DllImport("user32.dll")]
        static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);
        [DllImport("user32.dll", SetLastError=true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        // When you don't want the ProcessId, use this overload and pass IntPtr.Zero for the second parameter
        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr processId);
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();
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
                return FunctionalExtensions.None();
            return FunctionalExtensions.Some(windowHandle);
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
                return FunctionalExtensions.None();
            return FunctionalExtensions.Some(windowHandle);
        }

        public static void RestoreWindow(IntPtr windowHandle)
        {
            ShowWindow(windowHandle, RestoreWindowMessage);
        }
        public static void ForceWindowIntoForeground(IntPtr window)
        {
            var currentThread = GetCurrentThreadId();

            var activeWindow = GetForegroundWindow();
            var activeThread = GetWindowThreadProcessId(activeWindow, out var activeProcess);

            var windowThread = GetWindowThreadProcessId(window, out var windowProcess);

            if (currentThread != activeThread)
                AttachThreadInput(currentThread, activeThread, true);
            if (windowThread != currentThread)
                AttachThreadInput(windowThread, currentThread, true);

            uint oldTimeout = 0, newTimeout = 0;
            SystemParametersInfo(SPI.SPI_GETFOREGROUNDLOCKTIMEOUT, 0, ref oldTimeout, 0);
            SystemParametersInfo(SPI.SPI_SETFOREGROUNDLOCKTIMEOUT, 0, ref newTimeout, 0);

            LockSetForegroundWindow(2 /* LSFW_UNLOCK */);
            AllowSetForegroundWindow(-1 /* ASFW_ANY */);

            SetForegroundWindow(window);
            ShowWindow(window, (int)ShowWindowCommands.Restore);

            SystemParametersInfo(SPI.SPI_SETFOREGROUNDLOCKTIMEOUT, 0, ref oldTimeout, 0);

            if (currentThread != activeThread)
                AttachThreadInput(currentThread, activeThread, false);
            if (windowThread != currentThread)
                AttachThreadInput(windowThread, currentThread, false);
        }
    }
}