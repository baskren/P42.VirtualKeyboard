
#if __ANDROID__
using Android.Content;
#endif
using Microsoft.UI.Xaml;
using System;

namespace P42.VirtualKeyboard
{
    /// <summary>
    /// Keyboard service.
    /// </summary>
    public static class Service
    {
        static Service()
            => Init();

        internal static bool IsAvailable => Instance is not null;

        static IKeyboardService _instance;
        static IKeyboardService Instance
        {
            get
            {
                if (_instance == null)
                    Init();
                return _instance;
            }
            set => _instance = value;
        }

#if __ANDROID__

        public static void Init()
            => Instance = new AndroidService();


#elif __IOS__

        public static void Init()
            => Instance = new IosService();


#elif WINDOWS

        public static void Init()
            => Instance = new WinUiService();
        
#else

        public static void Init() { }
#endif


        /// <summary>
        /// Forces the device's on screen keyboard to be hidden
        /// </summary>
        public static void Hide() => Instance?.Hide();

        /// <summary>
        /// Occurs when hidden.
        /// </summary>
        public static event EventHandler<bool> IsVisibleChanged;

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


        public static bool IsVisible => Instance?.IsVisible ?? false;


        internal static void OnVisiblityChange(KeyboardVisibilityChange state)
            => IsVisibleChanged?.Invoke(null, state == KeyboardVisibilityChange.Shown);

        internal static void OnHeightChanged(double height)
            => HeightChanged?.Invoke(null, height);



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
