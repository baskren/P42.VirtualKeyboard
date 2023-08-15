//using Windows.UI.ViewManagement;

namespace Demo
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();


        }

        private void OnVirutalKeyboardShown(object? sender, EventArgs e)
        {
            _textBlock.Text = "KEYBOARD SHOWN";
        }

        private void OnVirtualKeyboardHidden(object? sender, EventArgs e)
        {
            _textBlock.Text = "KEYBOARD HIDDEN";
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);


            P42.VirtualKeyboard.Service.Hidden += OnVirtualKeyboardHidden;
            P42.VirtualKeyboard.Service.Shown += OnVirutalKeyboardShown;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            P42.VirtualKeyboard.Service.Hidden -= OnVirtualKeyboardHidden;
            P42.VirtualKeyboard.Service.Shown -= OnVirutalKeyboardShown;
        }
    }
}