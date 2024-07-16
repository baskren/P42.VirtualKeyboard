//using Windows.UI.ViewManagement;

namespace Demo
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            UpdateVisibleText();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);


            P42.VirtualKeyboard.Service.IsVisibleChanged += OnVirtualKeyboardIsVisibleChanged;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            P42.VirtualKeyboard.Service.IsVisibleChanged -= OnVirtualKeyboardIsVisibleChanged;
        }

        private void OnVirtualKeyboardIsVisibleChanged(object? sender, bool e)
            => UpdateVisibleText();

        void UpdateVisibleText() => _textBlock.Text = P42.VirtualKeyboard.Service.IsVisible ? "KEYBOARD SHOWN" : "KEYBOARD HIDDEN";
    }
}