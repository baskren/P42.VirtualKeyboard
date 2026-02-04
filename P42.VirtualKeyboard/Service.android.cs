using Android.App;
using Android.Content;
using Android.Views.InputMethods;
using Java.Util;
using Android.Views;
using AndroidX.Core.View;

namespace P42.VirtualKeyboard;

public class AndroidService : IKeyboardService
{

    private static Activity Activity
    {
        get => field ?? throw new Exception("P42.VirtualKeyboard not initialized.  Call P42.VirtualKeyboard.AndroidService.Init(this) in MainActivity.Android.cs:");
        set;
    }

    private View RootView
    {
        get
        {
            if (field is not null)
                return field;

            var task = Task.Run(GetRootViewAsync);
            task.Wait();
            field = task.Result;
            return field ?? throw new Exception("P42.VirtualKeyboard: RootView not found.");
        }
    }


    private static async Task<View> GetRootViewAsync()
    {
        while (true)
        {
            if (Activity.FindViewById(Android.Resource.Id.Content) is { } view)
                return view;
            await Task.Delay(200);
        }
    }


    private static Android.Content.Res.Configuration Configuration => field ??= Activity.Resources?.Configuration ?? throw new Exception("P42.VirtualKeyboard not initialized.  Call P42.VirtualKeyboard.AndroidService.Init(this) in MainActivity.Android.cs:");

    public bool IsHardwareKeyboardActive
        => Configuration.HardKeyboardHidden == Android.Content.Res.HardKeyboardHidden.No;

    public void Hide()
    {
        if (Android.OS.Build.VERSION.SdkInt >= (Android.OS.BuildVersionCodes)30)
#pragma warning disable CA1416 // Validate platform compatibility
            RootView.WindowInsetsController?.Hide(WindowInsets.Type.Ime());
#pragma warning restore CA1416 // Validate platform compatibility
        else
        {
            if (Activity.GetSystemService(Context.InputMethodService) is InputMethodManager imm)
                imm.HideSoftInputFromWindow(RootView.WindowToken, HideSoftInputFlags.None);
        }
    }


    public void Show()
    {
        if (Android.OS.Build.VERSION.SdkInt >= (Android.OS.BuildVersionCodes)30)
#pragma warning disable CA1416 // Validate platform compatibility
            RootView.WindowInsetsController?.Show(WindowInsets.Type.Ime());
#pragma warning restore CA1416 // Validate platform compatibility
        else
        {
            if (Activity.GetSystemService(Context.InputMethodService) is InputMethodManager imm)
                imm.ShowSoftInput(RootView, ShowFlags.Implicit);
        }
    }
    

    public static void Init(Activity activity)
        => Activity = activity;

    public AndroidService()
    {
        var rootLayoutListener = new RootLayoutListener(RootView!);
        rootLayoutListener.HeightChanged += OnHeightChanged;
        RootView!.ViewTreeObserver?.AddOnGlobalLayoutListener(rootLayoutListener);
    }

    private double _oldHeight;
    private void OnHeightChanged(object? sender, double e)
    {
        Service.OnHeightChanged(e);
        if (_oldHeight > 0 == e > 0)
            return;

        _oldHeight = e;
        Service.OnVisibilityChange(e > 0);
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1422:Validate platform compatibility", Justification = "<Pending>")]
    public string LanguageRegion
    {
        get
        {
            if (Activity.GetSystemService(Context.InputMethodService) is not InputMethodManager imm)
                return "en";
            if (imm.CurrentInputMethodSubtype is not {} ims)
                return "en";

            var result = Android.OS.Build.VERSION.SdkInt >= (Android.OS.BuildVersionCodes)24 
#pragma warning disable CA1416
                ? ims.LanguageTag.Replace('_', '-') 
#pragma warning restore CA1416
                : ims.Locale.Replace('_', '-');

            if (!string.IsNullOrWhiteSpace(result))
                return result;

            var language = Locale.Default.Language;
            var country = Locale.Default.Country;

            if (string.IsNullOrWhiteSpace(language) && string.IsNullOrWhiteSpace(country))
                return "en";
            if (string.IsNullOrWhiteSpace(language))
                return country;
            if (string.IsNullOrWhiteSpace(country))
                return language;
            return language + "-" + country;
        }
    }


    public double Height
    {
        get
        {

#pragma warning disable CA1416 // Validate platform compatibility
            if (Android.OS.Build.VERSION.SdkInt >= (Android.OS.BuildVersionCodes)30)
            {
                if (RootView.RootWindowInsets is {} rootInsets)
                    field = rootInsets.GetInsets(WindowInsets.Type.Ime()).Bottom;
#pragma warning restore CA1416 // Validate platform compatibility
                else if (ViewCompat.GetRootWindowInsets(RootView) is { } insets)
                {
                    // Get specific inset types (e.g., status bar, navigation bar)
                    var systemBars = insets.GetInsets(WindowInsetsCompat.Type.SystemBars());
                    //var statusBar = insets.GetInsets(WindowInsetsCompat.Type.StatusBars());
                    //var navigationBars = insets.GetInsets(WindowInsetsCompat.Type.NavigationBars());

                    field = systemBars?.Bottom ?? 0;
                }
                else
                    field = 0;
            }
            else
            {
#pragma warning disable CA1422 // Validate platform compatibility
                var frame = new Android.Graphics.Rect();
                RootView.GetWindowVisibleDisplayFrame(frame);

                int screenHeight = RootView.Height;
                field = screenHeight - frame.Bottom;
#pragma warning restore CA1422 // Validate platform compatibility
            }

            return field;
        }
    }

    public bool IsVisible => Height > 0;
    
}

class RootLayoutListener : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
{
    //int[] _discrepancy = { 0 };

    private readonly Android.Graphics.Rect _startRect;
    private readonly View _rootView;

    public event EventHandler<double>? HeightChanged;


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


        //System.Diagnostics.Debug.WriteLine($"RootLayoutListener.OnGlobalLayout : [{_startRect.Height()}] [{currentRect.Height()}]");


        HeightChanged?.Invoke(this, height / Scale);
    }

    private bool _disposed;
    protected override void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            _disposed = true;
            _startRect.Dispose();
        }
        base.Dispose(disposing);
    }

    Java.Lang.Ref.WeakReference? _displayMetricsReference;
    Android.Util.DisplayMetrics? DisplayMetrics
    {
        get
        {
            _displayMetricsReference ??= new Java.Lang.Ref.WeakReference(Android.App.Application.Context.Resources?.DisplayMetrics);
            var displayMetrics = (Android.Util.DisplayMetrics?)_displayMetricsReference.Get();
            if (displayMetrics == null)
            {
                displayMetrics = Android.App.Application.Context.Resources?.DisplayMetrics;
                _displayMetricsReference = new Java.Lang.Ref.WeakReference(displayMetrics);
            }
            return displayMetrics;
        }
    }

    public float Scale => DisplayMetrics?.Density ?? 1.0f;

}
