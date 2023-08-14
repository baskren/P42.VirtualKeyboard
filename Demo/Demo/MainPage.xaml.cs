namespace Demo
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            P42.VirtualKeyboard.Service.Hidden += OnVirtualKeyboardHidden;
            P42.VirtualKeyboard.Service.Shown += OnVirutalKeyboardShown;

        }

        private void OnVirutalKeyboardShown(object? sender, EventArgs e)
        {
            _textBlock.Text = "KEYBOARD SHOWN";
        }

        private void OnVirtualKeyboardHidden(object? sender, EventArgs e)
        {
            _textBlock.Text = "KEYBOARD HIDDEN";
        }
    }
}