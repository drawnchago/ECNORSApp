using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace ECNORSApp
{
    [Activity(
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = true,
        ConfigurationChanges =
            ConfigChanges.ScreenSize |
            ConfigChanges.Orientation |
            ConfigChanges.UiMode |
            ConfigChanges.ScreenLayout |
            ConfigChanges.SmallestScreenSize |
            ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            ApplyEcnorsaSystemBars();
        }

        protected override void OnResume()
        {
            base.OnResume();
            ApplyEcnorsaSystemBars();
        }

        private void ApplyEcnorsaSystemBars()
        {
            var ecnorsaColor = Android.Graphics.Color.ParseColor("#003A8F"); 

            Window?.ClearFlags(WindowManagerFlags.TranslucentStatus);
            Window?.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);

            Window?.SetStatusBarColor(ecnorsaColor);
            Window?.SetNavigationBarColor(ecnorsaColor);
        }
    }
}
