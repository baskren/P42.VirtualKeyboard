using System;

namespace P42.VirtualKeyboard
{
    /// <summary>
    /// Keyboard service.
    /// </summary>
    interface IKeyboardService
    {
        /// <summary>
        /// Forces the device's on screen keyboard to be hidden.
        /// </summary>
        void Hide();

        /// <summary>
        /// Gets a value indicating whether the hardware keyboard active.
        /// </summary>
        /// <value><c>true</c> if is hardware keyboard active; otherwise, <c>false</c>.</value>
        bool IsHardwareKeyboardActive { get; }

        /// <summary>
        /// Gets the language region.
        /// </summary>
        /// <value>The language region.</value>
        string LanguageRegion { get; }

        /// <summary>
        /// Gets the current height of the on-screen keyboard
        /// </summary>
        /// <value>The height.</value>
        double Height { get; }
    }
}
