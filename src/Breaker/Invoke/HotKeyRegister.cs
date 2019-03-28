﻿using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;

namespace Breaker.Invoke
{
    public class HotKeyRegister : IMessageFilter, IDisposable
    {
        /// <summary>
        ///     Define a system-wide hot key.
        /// </summary>
        /// <param name="hWnd">
        ///     A handle to the window that will receive WM_HOTKEY messages generated by the
        ///     hot key. If this parameter is NULL, WM_HOTKEY messages are posted to the
        ///     message queue of the calling thread and must be processed in the message loop.
        /// </param>
        /// <param name="id">
        ///     The identifier of the hot key. If the hWnd parameter is NULL, then the hot
        ///     key is associated with the current thread rather than with a particular
        ///     window.
        /// </param>
        /// <param name="fsModifiers">
        ///     The keys that must be pressed in combination with the key specified by the
        ///     uVirtKey parameter in order to generate the WM_HOTKEY message. The fsModifiers
        ///     parameter can be a combination of the following values.
        ///     MOD_ALT     0x0001
        ///     MOD_CONTROL 0x0002
        ///     MOD_SHIFT   0x0004
        ///     MOD_WIN     0x0008
        /// </param>
        /// <param name="vk">The virtual-key code of the hot key.</param>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool RegisterHotKey(
            IntPtr hWnd,
            int id,
            KeyModifiers fsModifiers,
            Keys vk
        );

        /// <summary>
        ///     Frees a hot key previously registered by the calling thread.
        /// </summary>
        /// <param name="hWnd">
        ///     A handle to the window associated with the hot key to be freed. This parameter
        ///     should be NULL if the hot key is not associated with a window.
        /// </param>
        /// <param name="id">
        ///     The identifier of the hot key to be freed.
        /// </param>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        /// <summary>
        ///     Get the modifiers and key from the KeyData property of KeyEventArgs.
        /// </summary>
        /// <param name="keydata">
        ///     The KeyData property of KeyEventArgs. The KeyData is a key in combination
        ///     with modifiers.
        /// </param>
        /// <param name="key">The pressed key.</param>
        public static KeyModifiers GetModifiers(Keys keydata, out Keys key)
        {
            key = keydata;
            var modifers = KeyModifiers.None;

            // Check whether the keydata contains the CTRL modifier key.
            // The value of Keys.Control is 131072.
            if ((keydata & Keys.Control) == Keys.Control)
            {
                modifers |= KeyModifiers.Control;

                key = keydata ^ Keys.Control;
            }

            // Check whether the keydata contains the SHIFT modifier key.
            // The value of Keys.Control is 65536.
            if ((keydata & Keys.Shift) == Keys.Shift)
            {
                modifers |= KeyModifiers.Shift;
                key = key ^ Keys.Shift;
            }

            // Check whether the keydata contains the ALT modifier key.
            // The value of Keys.Control is 262144.
            if ((keydata & Keys.Alt) == Keys.Alt)
            {
                modifers |= KeyModifiers.Alt;
                key = key ^ Keys.Alt;
            }

            // Check whether a key other than SHIFT, CTRL or ALT (Menu) is pressed.
            if (key == Keys.ShiftKey || key == Keys.ControlKey || key == Keys.Menu)
                key = Keys.None;

            return modifers;
        }

        /// <summary>
        ///     Specify whether this object is disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        ///     This constant could be found in WinUser.h if you installed Windows SDK.
        ///     Each windows message has an identifier, 0x0312 means that the mesage is
        ///     a WM_HOTKEY message.
        /// </summary>
        private const int WM_HOTKEY = 0x0312;

        /// <summary>
        ///     A handle to the window that will receive WM_HOTKEY messages generated by the
        ///     hot key.
        /// </summary>
        public IntPtr Handle { get; }

        /// <summary>
        ///     A normal application can use any value between 0x0000 and 0xBFFF as the ID
        ///     but if you are writing a DLL, then you must use GlobalAddAtom to get a
        ///     unique identifier for your hot key.
        /// </summary>
        public int ID { get; }

        public KeyModifiers Modifiers { get; }

        public Keys Key { get; }

        /// <summary>
        ///     Raise an event when the hotkey is pressed.
        /// </summary>
        public event EventHandler HotKeyPressed;


        public HotKeyRegister(IntPtr handle, int id, KeyModifiers modifiers, Keys key)
        {
            if (key == Keys.None || modifiers == KeyModifiers.None)
                throw new ArgumentException("The key or modifiers could not be None.");

            Handle = handle;
            ID = id;
            Modifiers = modifiers;
            Key = key;

            RegisterHotKey();

            // Adds a message filter to monitor Windows messages as they are routed to
            // their destinations.
            Application.AddMessageFilter(this);
        }


        /// <summary>
        ///     Register the hotkey.
        /// </summary>
        private void RegisterHotKey()
        {
            var isKeyRegisterd = RegisterHotKey(Handle, ID, Modifiers, Key);

            // If the operation failed, try to unregister the hotkey if the thread 
            // has registered it before.
            if (!isKeyRegisterd)
            {
                // IntPtr.Zero means the hotkey registered by the thread.
                UnregisterHotKey(IntPtr.Zero, ID);

                // Try to register the hotkey again.
                isKeyRegisterd = RegisterHotKey(Handle, ID, Modifiers, Key);

                // If the operation still failed, it means that the hotkey was already 
                // used in another thread or process.
                if (!isKeyRegisterd)
                    throw new ApplicationException("The hotkey is in use");
            }
        }

        /// <summary>
        ///     Filters out a message before it is dispatched.
        /// </summary>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public bool PreFilterMessage(ref Message m)
        {
            // The property WParam of Message is typically used to store small pieces 
            // of information. In this scenario, it stores the ID.
            if (m.Msg == WM_HOTKEY && m.HWnd == Handle && m.WParam == (IntPtr) ID && HotKeyPressed != null)
            {
                // Raise the HotKeyPressed event if it is an WM_HOTKEY message.
                HotKeyPressed(this, EventArgs.Empty);

                // True to filter the message and stop it from being dispatched.
                return true;
            }

            // Return false to allow the message to continue to the next filter or 
            // control.
            return false;
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Unregister the hotkey.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            // Protect from being called multiple times.
            if (disposed)
                return;

            if (disposing)
            {
                // Removes a message filter from the message pump of the application.
                Application.RemoveMessageFilter(this);

                UnregisterHotKey(Handle, ID);
            }

            disposed = true;
        }
    }
}