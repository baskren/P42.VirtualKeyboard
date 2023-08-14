
#if __ANDROID__
using Android.Content;
#endif
using System;

namespace P42.VirtualKeyboard
{
    /// <summary>
    /// Keyboard service.
    /// </summary>
    public static class Service
    {
        internal static bool IsAvailable => Instance is not null;

        static IKeyboardService _instance;
        static IKeyboardService Instance
        {
            get
            {
                if (_instance == null)
                    PlatformInit();
                return _instance;
            }
            set => _instance = value;
        }

#if __ANDROID__

        static void PlatformInit()
            => Instance = new AndroidService();


#elif __IOS__

        static void PlatformInit()
            => Instance = new IosService();


#elif WINDOWS

        static void PlatformInit()
            => Instance = new WinUiService();
        
#else

        static void PlatformInit() { }
#endif

        /*
        /// <summary>
        /// Activates the Keyboard Service (required to monitor software keyboard height)
        /// </summary>
        public static void Activate()
        {
            if (Instance == null)
                Console.WriteLine("KEYBOARD SERVICE IS NOT AVAILABLE");
        }
        */

        /// <summary>
        /// Forces the device's on screen keyboard to be hidden
        /// </summary>
        public static void Hide() => Instance.Hide();



        internal static void OnVisiblityChange(KeyboardVisibilityChange state)
        {
            switch (state)
            {
                case KeyboardVisibilityChange.Shown:
                    Shown?.Invoke(null, EventArgs.Empty);
                    break;
                case KeyboardVisibilityChange.Hidden:
                    Hidden?.Invoke(null, EventArgs.Empty);
                    break;
            }
        }

        internal static void OnHeightChanged(double height)
        {
            HeightChanged?.Invoke(null, height);
        }

        /// <summary>
        /// Occurs when hidden.
        /// </summary>
        public static event EventHandler Hidden;
        /// <summary>
        /// Occurs when shown.
        /// </summary>
        public static event EventHandler Shown;

        /// <summary>
        /// Occurs when virtual keyboard height has changed.
        /// </summary>
        public static event EventHandler<double> HeightChanged;

        /// <summary>
        /// Gets a value indicating whether the hardware keyboard is active.
        /// </summary>
        /// <value><c>true</c> if is hardware keyboard active; otherwise, <c>false</c>.</value>
        public static bool IsHardwareKeyboardActive => Instance?.IsHardwareKeyboardActive ?? true;

        /// <summary>
        /// Gets the Keyboard's language-region.
        /// </summary>
        /// <value>The language region.</value>
        public static string LanguageRegion => Instance?.LanguageRegion ?? "";

        /// <summary>
        /// Gets the current height of the on-screen software keyboard
        /// </summary>
        /// <value>The height.</value>
        public static double Height => Instance?.Height ?? 0.0;

    }

    /// <summary>
    /// Keyboard visibility change.
    /// </summary>
    public enum KeyboardVisibilityChange
    {
        /// <summary>
        /// The keyboard will show/has shown.
        /// </summary>
        Shown,
        /// <summary>
        /// The keyboard will hide/has been hidden.
        /// </summary>
        Hidden
    }
}
