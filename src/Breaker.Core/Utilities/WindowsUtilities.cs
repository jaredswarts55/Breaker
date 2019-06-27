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
            if(windowHandle == IntPtr.Zero)
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
            if(windowHandle == IntPtr.Zero)
                return None();
            return Some(windowHandle);
        }

        public static void RestoreWindow(IntPtr windowHandle)
        {
            ShowWindow(windowHandle, RestoreWindowMessage);
        }
    }
}
