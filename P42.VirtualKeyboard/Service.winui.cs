using Windows.Graphics.Display;
using Windows.UI.ViewManagement;

namespace P42.VirtualKeyboard;

internal partial class WinUiService : IKeyboardService, IDisposable
{
    //https://learn.microsoft.com/en-us/windows/apps/design/input/respond-to-the-presence-of-the-touch-keyboard

    public bool IsHardwareKeyboardActive
	{
		get
		{
			var keyboardCapabilities = new Windows.Devices.Input.KeyboardCapabilities();
			return keyboardCapabilities.KeyboardPresent != 0;
		}
	}

	private DisplayInformation? _displayInformation;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:P42.VirtualKeyboard.Service"/> class.
	/// </summary>
	public WinUiService()
	{

		Task.Run(async () =>
		{
			try
            {
                InputPane? inputPane = null;
                while (inputPane is null)
                {
                    inputPane = InputPane.GetForCurrentView();
                    if (inputPane is null)
                        await Task.Delay(200);
                }

				//var inputPane = InputPane.GetForCurrentView();

				inputPane.Hiding += KeyboardService_Hiding;
				inputPane.Showing += KeyboardService_Showing;
			}
			catch(Exception)
			{
				System.Diagnostics.Debug.WriteLine("VirtualKeyboard not yet supported in WinUI");
				Console.WriteLine("VirtualKeyboard not yet supported in WinUI");
            }
        });

	}

	private void OnOrientationChanged(DisplayInformation sender, object args)
	{
		Height = InputPane.GetForCurrentView().OccludedRect.Height;
	}

	private void KeyboardService_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
	{
		Service.OnVisibilityChange(true);
		Height = InputPane.GetForCurrentView().OccludedRect.Height;
		_displayInformation = DisplayInformation.GetForCurrentView();
		_displayInformation.OrientationChanged += OnOrientationChanged;
	}

	private void KeyboardService_Hiding(InputPane sender, InputPaneVisibilityEventArgs args)
	{
		Service.OnVisibilityChange(false);
		Height = InputPane.GetForCurrentView().OccludedRect.Height;
		if (_displayInformation != null)
			_displayInformation.OrientationChanged -= OnOrientationChanged;
	}

	/// <summary>
	/// Hide this instance.
	/// </summary>
	public void Hide()
	    => InputPane.GetForCurrentView().TryHide();
	

	public string LanguageRegion => Windows.Globalization.Language.CurrentInputMethodLanguageTag;

	public double Height
	{
		get => field;
		private set
        {
            if (Math.Abs(field - value) < 0.0001)
                return;

            field = value;
            Service.OnHeightChanged(field);
        }
	}

	public bool IsVisible => false;

	#region IDisposable Support
	private bool _disposed; // To detect redundant calls
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed && disposing)
		{
			_disposed = true;
            if (_displayInformation == null)
                return;

            _displayInformation.OrientationChanged -= OnOrientationChanged;
            _displayInformation = null;

        }
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
	#endregion
}
