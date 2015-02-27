using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GWvW_Overlay.Keyboard
{
    /// <summary>
    /// Listens keyboard globally.
    /// 
    /// <remarks>Uses WH_KEYBOARD_LL.</remarks>
    /// </summary>
    public class KeyboardListener : IDisposable
    {
        /// <summary>
        /// Creates global keyboard listener.
        /// </summary>
        public KeyboardListener()
        {
            // We have to store the HookCallback, so that it is not garbage collected runtime
            hookedLowLevelKeyboardProc = (InterceptKeys.LowLevelKeyboardProc)LowLevelKeyboardProc;

            // Set the hook
            hookId = InterceptKeys.SetHook(hookedLowLevelKeyboardProc);

            // Assign the asynchronous callback event
            hookedKeyboardCallbackAsync = new KeyboardCallbackAsync(KeyboardListener_KeyboardCallbackAsync);
        }

        /// <summary>
        /// Destroys global keyboard listener.
        /// </summary>
        ~KeyboardListener()
        {
            Dispose();
        }

        /// <summary>
        /// Fired when any of the keys is pressed down.
        /// </summary>
        public event RawKeyEventHandler KeyDown;

        /// <summary>
        /// Fired when any of the keys is released.
        /// </summary>
        public event RawKeyEventHandler KeyUp;

        #region Inner workings
        /// <summary>
        /// Hook ID
        /// </summary>
        private IntPtr hookId = IntPtr.Zero;

        /// <summary>
        /// Asynchronous callback hook.
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        private delegate void KeyboardCallbackAsync(InterceptKeys.KeyEvent keyEvent, int vkCode);

        /// <summary>
        /// Actual callback hook.
        /// 
        /// <remarks>Calls asynchronously the asyncCallback.</remarks>
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private IntPtr LowLevelKeyboardProc(int nCode, UIntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
                if (wParam.ToUInt32() == (int)InterceptKeys.KeyEvent.WM_KEYDOWN ||
                    wParam.ToUInt32() == (int)InterceptKeys.KeyEvent.WM_KEYUP ||
                    wParam.ToUInt32() == (int)InterceptKeys.KeyEvent.WM_SYSKEYDOWN ||
                    wParam.ToUInt32() == (int)InterceptKeys.KeyEvent.WM_SYSKEYUP)
                    hookedKeyboardCallbackAsync.BeginInvoke((InterceptKeys.KeyEvent)wParam.ToUInt32(), Marshal.ReadInt32(lParam), null, null);

            return InterceptKeys.CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        /// <summary>
        /// Event to be invoked asynchronously (BeginInvoke) each time key is pressed.
        /// </summary>
        private KeyboardCallbackAsync hookedKeyboardCallbackAsync;

        /// <summary>
        /// Contains the hooked callback in runtime.
        /// </summary>
        private InterceptKeys.LowLevelKeyboardProc hookedLowLevelKeyboardProc;

        /// <summary>
        /// HookCallbackAsync procedure that calls accordingly the KeyDown or KeyUp events.
        /// </summary>
        /// <param name="keyEvent">Keyboard event</param>
        /// <param name="vkCode">VKCode</param>
        void KeyboardListener_KeyboardCallbackAsync(InterceptKeys.KeyEvent keyEvent, int vkCode)
        {
            switch (keyEvent)
            {
                // KeyDown events
                case InterceptKeys.KeyEvent.WM_KEYDOWN:
                    if (KeyDown != null)
                        KeyDown(this, new RawKeyEventArgs(vkCode, false));
                    break;
                case InterceptKeys.KeyEvent.WM_SYSKEYDOWN:
                    if (KeyDown != null)
                        KeyDown(this, new RawKeyEventArgs(vkCode, true));
                    break;

                // KeyUp events
                case InterceptKeys.KeyEvent.WM_KEYUP:
                    if (KeyUp != null)
                        KeyUp(this, new RawKeyEventArgs(vkCode, false));
                    break;
                case InterceptKeys.KeyEvent.WM_SYSKEYUP:
                    if (KeyUp != null)
                        KeyUp(this, new RawKeyEventArgs(vkCode, true));
                    break;

                default:
                    break;
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes the hook.
        /// <remarks>This call is required as it calls the UnhookWindowsHookEx.</remarks>
        /// </summary>
        public void Dispose()
        {
            InterceptKeys.UnhookWindowsHookEx(hookId);
        }

        #endregion
    }

    #region WINAPI Helper class

    #endregion
}
