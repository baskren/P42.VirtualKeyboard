using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;

namespace P42.VirtualKeyboard
{
	class WinUiService : IKeyboardService, IDisposable
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

		Windows.Graphics.Display.DisplayInformation _displayInformation;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Forms9Patch.iOS.KeyboardService"/> class.
		/// </summary>
		public WinUiService()
		{

			Task.Run(() =>
			{
				try
				{
					while (InputPane.GetForCurrentView() is not InputPane)
						Task.Delay(200);

					var inputPane = InputPane.GetForCurrentView();

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

		private void OnOrienationChanged(DisplayInformation sender, object args)
		{
			Height = InputPane.GetForCurrentView().OccludedRect.Height;
		}

		private void KeyboardService_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
		{
			Service.OnVisiblityChange(true);
			Height = InputPane.GetForCurrentView().OccludedRect.Height;
			_displayInformation = Windows.Graphics.Display.DisplayInformation.GetForCurrentView();
			_displayInformation.OrientationChanged += OnOrienationChanged;
		}

		private void KeyboardService_Hiding(InputPane sender, InputPaneVisibilityEventArgs args)
		{
			Service.OnVisiblityChange(false);
			Height = InputPane.GetForCurrentView().OccludedRect.Height;
			if (_displayInformation != null)
				_displayInformation.OrientationChanged -= OnOrienationChanged;
		}

		/// <summary>
		/// Hide this instance.
		/// </summary>
		public void Hide()
		{
			InputPane.GetForCurrentView().TryHide();
		}

		public string LanguageRegion
		{
			get
			{
				return Windows.Globalization.Language.CurrentInputMethodLanguageTag;
			}
		}

		double _height;
		public double Height
		{
			get => _height;
			set
			{
				if (_height != value)
				{
					_height = value;
					Service.OnHeightChanged(_height);
				}
			}
		}

		public bool IsVisible => false;

		#region IDisposable Support
		private bool _disposed = false; // To detect redundant calls
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed && disposing)
			{
				_disposed = true;
				if (_displayInformation != null)
				{
					_displayInformation.OrientationChanged -= OnOrienationChanged;
					_displayInformation = null;
				}

			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
