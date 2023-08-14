using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Views.InputMethods;
using Java.Util;
using Android.Views;

namespace P42.VirtualKeyboard
{
    public class AndroidService : IKeyboardService
    {
        
        Android.App.Activity Activity => Application.Context as Activity;

        public bool IsHardwareKeyboardActive
        {
            get
            {
                return Activity.Resources.Configuration.HardKeyboardHidden == Android.Content.Res.HardKeyboardHidden.No;
            }
        }

        public void Hide()
        {
            if (Android.App.Application.Context.GetSystemService(Context.InputMethodService) is InputMethodManager im
                && Application.Context is Activity activity)
            {
                var token = activity.CurrentFocus?.WindowToken;
                im.HideSoftInputFromWindow(token, HideSoftInputFlags.NotAlways);
            }
        }

        public AndroidService()
        {
            Android.Views.View root = Activity.FindViewById(Android.Resource.Id.Content);


            var rootLayoutListener = new RootLayoutListener(root);
            rootLayoutListener.HeightChanged += OnHeightChanged;
            root.ViewTreeObserver.AddOnGlobalLayoutListener(rootLayoutListener);
        }


        double _lastHeight;
        private void OnHeightChanged(object sender, double e)
        {
            Height = e;
            if (Height > 0 && _lastHeight <= 0)
                Service.OnVisiblityChange(KeyboardVisibilityChange.Shown);
            else if (_lastHeight > 0 && Height <= 0)
                Service.OnVisiblityChange(KeyboardVisibilityChange.Hidden);
            _lastHeight = Height;
        }

        public string LanguageRegion
        {
            get
            {
                var imm = (InputMethodManager)Activity.GetSystemService(Context.InputMethodService);
                var ims = imm.CurrentInputMethodSubtype;
                string result;
                //if (Android.OS.Build.VERSION.SDK_INT > 24)
                if (Android.OS.Build.VERSION.SdkInt > Android.OS.BuildVersionCodes.M)
                    result = ims?.LanguageTag.Replace('_', '-');
                else
#pragma warning disable CS0618 // Type or member is obsolete
                    result = ims?.Locale.Replace('_', '-');
#pragma warning restore CS0618 // Type or member is obsolete

                if (string.IsNullOrWhiteSpace(result))
                {
                    var language = Locale.Default.Language;
                    var country = Locale.Default.Country;

                    if (string.IsNullOrWhiteSpace(language))
                        return country;
                    if (string.IsNullOrWhiteSpace(country))
                        return language;
                    return language + "-" + country;
                }
                return result;
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

    class RootLayoutListener : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
    {
        //int[] _discrepancy = { 0 };

        readonly Android.Graphics.Rect _startRect;
        readonly Android.Views.View _rootView;

        public event EventHandler<double> HeightChanged;


        public RootLayoutListener(Android.Views.View view)
        {
            _rootView = view;
            _startRect = new Android.Graphics.Rect();
            _rootView.GetWindowVisibleDisplayFrame(_startRect);
        }

        public void OnGlobalLayout()
        {
            Android.Graphics.Rect currentRect = new Android.Graphics.Rect();
            _rootView.GetWindowVisibleDisplayFrame(currentRect);

            var height = _startRect.Height() - currentRect.Height();

            HeightChanged?.Invoke(this, height / Scale);
        }

        bool _disposed;
        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;
                _startRect?.Dispose();
            }
            base.Dispose(disposing);
        }

        Java.Lang.Ref.WeakReference _displayMetricsReference;
        Android.Util.DisplayMetrics DisplayMetrics
        {
            get
            {
                _displayMetricsReference = _displayMetricsReference ?? new Java.Lang.Ref.WeakReference(global::Android.App.Application.Context.Resources.DisplayMetrics);
                var displayMetrics = (Android.Util.DisplayMetrics)_displayMetricsReference.Get();
                if (displayMetrics == null)
                {
                    displayMetrics = global::Android.App.Application.Context.Resources.DisplayMetrics;
                    _displayMetricsReference = new Java.Lang.Ref.WeakReference(displayMetrics);
                }
                return displayMetrics;
            }
        }

        public float Scale => DisplayMetrics.Density;

    }
}
