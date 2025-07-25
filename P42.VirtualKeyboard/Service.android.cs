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

        const double Threshold = 50;

        static Activity? _activity;
        static Activity Activity
        {
            get
            {
                if (_activity is null)
                    throw new Exception("P42.VirtualKeyboard not initialized.  Call P42.VirtualKeyboard.AndroidService.Init(this) in MainActivity.Android.cs:");
                return _activity;
            }
            set => _activity = value;
        }

        View _rootView;
        View RootView
        {
            get
            {
                if (_rootView is null)
                {
                    var task = Task.Run(GetRootViewAsync);
                    task.Wait();
                    _rootView = task.Result;
                }
                return _rootView;
            }
            set => _rootView = value;
        }

        
        
        static async Task<View> GetRootViewAsync()
        {
            var view = Activity.FindViewById(Android.Resource.Id.Content);
            
            while (view is null)
            {
                await Task.Delay(200);
                view = Activity.FindViewById(Android.Resource.Id.Content);
            }

            return view;
        }

        public bool IsHardwareKeyboardActive
            => Activity.Resources.Configuration.HardKeyboardHidden == Android.Content.Res.HardKeyboardHidden.No;

        public void Hide()
            => RootView.WindowInsetsController.Hide(WindowInsets.Type.Ime());
        

        public void Show()
            => RootView.WindowInsetsController.Show(WindowInsets.Type.Ime());
        

        public static void Init(Activity activity)
            => Activity = activity;

        public AndroidService()
        {
            var rootLayoutListener = new RootLayoutListener(RootView);
            rootLayoutListener.HeightChanged += OnHeightChanged;
            RootView.ViewTreeObserver.AddOnGlobalLayoutListener(rootLayoutListener);
        }


        private void OnHeightChanged(object sender, double e)
            => Height = RootView.RootWindowInsets.GetInsets(WindowInsets.Type.Ime()).Bottom;


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1422:Validate platform compatibility", Justification = "<Pending>")]
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
                    result = ims?.Locale.Replace('_', '-');

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
            private set
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

        bool _isVisible;
        bool _isVisibleSet;
        public bool IsVisible
        {
            get
            {
                if (!_isVisibleSet)
                {
                    var height = RootView.RootWindowInsets?.GetInsets(WindowInsets.Type.Ime()).Bottom ?? 0;
                    _isVisible = height > Threshold;
                    _isVisibleSet = true;
                }
                return _isVisible;
            }
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    _isVisibleSet = true;
                    Service.OnVisiblityChange(_isVisible);
                }
            }
        }
    }

    class RootLayoutListener : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
    {
        //int[] _discrepancy = { 0 };

        readonly Android.Graphics.Rect _startRect;
        readonly View _rootView;

        public event EventHandler<double> HeightChanged;


        public RootLayoutListener(View view)
        {

            while (view.Parent is ViewGroup viewGroup)
                view = viewGroup;

            _rootView = view;
            _startRect = new Android.Graphics.Rect();
            _rootView.GetWindowVisibleDisplayFrame(_startRect);
        }

        public void OnGlobalLayout()
        {
            Android.Graphics.Rect currentRect = new();
            _rootView.GetWindowVisibleDisplayFrame(currentRect);

            var height = _startRect.Height() - currentRect.Height();


            System.Diagnostics.Debug.WriteLine($"RootLayoutListener.OnGlobalLayout : [{_startRect.Height()}] [{currentRect.Height()}]");


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
                _displayMetricsReference ??= new Java.Lang.Ref.WeakReference(global::Android.App.Application.Context.Resources.DisplayMetrics);
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
