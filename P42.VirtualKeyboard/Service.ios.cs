using UIKit;
using Foundation;

namespace P42.VirtualKeyboard
{
    /// <summary>
    /// Keyboard service.
    /// </summary>
    public class IosService : IKeyboardService
    {
        public bool IsHardwareKeyboardActive
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Forms9Patch.iOS.KeyboardService"/> class.
        /// </summary>
        public IosService()
        {
            UIKeyboard.Notifications.ObserveWillHide(OnHidden);
            UIKeyboard.Notifications.ObserveWillShow(OnShown);
            UIKeyboard.Notifications.ObserveDidChangeFrame(OnFrameChanged);
        }

        bool _hidden = true;
        /// <summary>
        /// Ons the hidden.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void OnHidden(object sender, UIKeyboardEventArgs e)
        {
            Service.OnVisiblityChange(KeyboardVisibilityChange.Hidden);
            Height = 0;
            _hidden = true;
        }

        /// <summary>
        /// Ons the shown.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void OnShown(object sender, UIKeyboardEventArgs e)
        {
            Service.OnVisiblityChange(KeyboardVisibilityChange.Shown);
            Height = e.FrameEnd.Height;
            _hidden = false;
        }

        void OnFrameChanged(object sender, UIKeyboardEventArgs e)
        {
            //CGSize kbSize = [[info objectForKey: UIKeyboardFrameBeginUserInfoKey] CGRectValue].size;
            var kbSize = e.FrameEnd;
            if (!_hidden)
                Height = kbSize.Height;

        }


        /// <summary>
        /// Hide this instance.
        /// </summary>
        public void Hide()
        {
            UIApplication.SharedApplication.KeyWindow.EndEditing(true);
        }

        public string LanguageRegion
        {
            get
            {
                //var defs = NSUserDefaults.StandardUserDefaults;
                return UITextInputMode.CurrentInputMode.PrimaryLanguage;
                //return null;
            }
        }

        double _height;
        public double Height
        {
            get => _height;
            set
            {
                if (System.Math.Abs(_height - value) > 0.1)
                {
                    _height = value;
                    Service.OnHeightChanged(_height);
                }
                _height = value;
            }
        }
    }
}
