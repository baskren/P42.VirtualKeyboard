using UIKit;

namespace P42.VirtualKeyboard
{
    // https://stackoverflow.com/questions/31991873/how-to-reliably-detect-if-an-external-keyboard-is-connected-on-ios-9

    public class IosService : IKeyboardService
    {
        const double Threshold = 50;


        public bool IsHardwareKeyboardActive
            => GameController.GCKeyboard.CoalescedKeyboard != null;

        public IosService()
        {
            UIKeyboard.Notifications.ObserveWillHide(OnHidden);
            UIKeyboard.Notifications.ObserveWillShow(OnShown);
            UIKeyboard.Notifications.ObserveDidChangeFrame(OnFrameChanged);
        }

        
        bool _hidden = true;
        void OnHidden(object sender, UIKeyboardEventArgs e)
        {
            Height = 0;
            _hidden = true;
        }

        void OnShown(object sender, UIKeyboardEventArgs e)
        {
            Height = e.FrameEnd.Height;
            _hidden = false;
        }
        
        void OnFrameChanged(object sender, UIKeyboardEventArgs e)
        {
            var kbSize = e.FrameEnd;
            if (!_hidden)
                Height = kbSize.Height;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1422:Validate platform compatibility", Justification = "<Pending>")]
        void IKeyboardService.Hide()
            => UIApplication.SharedApplication.KeyWindow.EndEditing(true);



        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1422:Validate platform compatibility", Justification = "<Pending>")]
        string IKeyboardService.LanguageRegion
            => UITextInputMode.CurrentInputMode.PrimaryLanguage;

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
                    IsVisible = value > Threshold;
                }

                _height = value;
            }
        }

        bool isVisible;
        public bool IsVisible
        {
            get => isVisible;
            private set
            {
                if (isVisible != value)
                {
                    isVisible = value;
                    Service.OnVisiblityChange(isVisible);
                }
            }
        }
    }
}
