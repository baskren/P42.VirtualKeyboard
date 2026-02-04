using UIKit;

namespace P42.VirtualKeyboard;

// https://stackoverflow.com/questions/31991873/how-to-reliably-detect-if-an-external-keyboard-is-connected-on-ios-9

public class IosService : IKeyboardService
{
    private const double Threshold = 50;


    public bool IsHardwareKeyboardActive
        => GameController.GCKeyboard.CoalescedKeyboard != null;

    public IosService()
    {
        UIKeyboard.Notifications.ObserveWillHide(OnHidden);
        UIKeyboard.Notifications.ObserveWillShow(OnShown);
        UIKeyboard.Notifications.ObserveDidChangeFrame(OnFrameChanged);
    }

    
    private bool _hidden = true;
    private void OnHidden(object? sender, UIKeyboardEventArgs e)
    {
        Height = 0;
        _hidden = true;
    }

    private void OnShown(object? sender, UIKeyboardEventArgs e)
    {
        Height = e.FrameEnd.Height;
        _hidden = false;
    }
    
    private void OnFrameChanged(object? sender, UIKeyboardEventArgs e)
    {
        var kbSize = e.FrameEnd;
        if (!_hidden)
            Height = kbSize.Height;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1422:Validate platform compatibility", Justification = "<Pending>")]
    void IKeyboardService.Hide()
        => UIApplication.SharedApplication.KeyWindow?.EndEditing(true);



    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1422:Validate platform compatibility", Justification = "<Pending>")]
    string IKeyboardService.LanguageRegion
        => UITextInputMode.CurrentInputMode?.PrimaryLanguage ?? "en";

    double _height;
    public double Height
    {
        get => _height;
        set
        {
            if (Math.Abs(_height - value) > 0.1)
            {
                _height = value;
                Service.OnHeightChanged(_height);
                IsVisible = value > Threshold;
            }

            _height = value;
        }
    }

    public bool IsVisible
    {
        get => field;
        private set
        {
            if (field == value)
                return;

            field = value;
            Service.OnVisibilityChange(field);
        }
    }
}
