namespace Demo;

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
        System.Diagnostics.Debug.WriteLine("OnNavigatedTo");
        P42.VirtualKeyboard.Service.IsVisibleChanged += OnVirtualKeyboardIsVisibleChanged;
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        System.Diagnostics.Debug.WriteLine("OnNavigatedFrom");

        P42.VirtualKeyboard.Service.IsVisibleChanged -= OnVirtualKeyboardIsVisibleChanged;
    }
    

    private void OnVirtualKeyboardIsVisibleChanged(object? sender, bool e)
    {
        UpdateVisibleText();
    }

    void UpdateVisibleText()
    {
        var isVisible = P42.VirtualKeyboard.Service.IsVisible;
        _textBlock.Text = isVisible ? "KEYBOARD SHOWN" : "KEYBOARD HIDDEN";
    }
    
}
