using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Util;
using Microsoft.Maui.Authentication;

namespace Alarm.Platforms.Android
{
	[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
	[IntentFilter(new[] { Intent.ActionView },
				 Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
				 DataScheme = "com.alarm.reminderapp")]

	public class WebAuthenticatorCallbackActivity : Microsoft.Maui.Authentication.WebAuthenticatorCallbackActivity
	{
		protected override void OnResume()
		{
			base.OnResume();

			// For debugging
			Log.Debug("Alarm", "OAuth callback received: " + Intent?.Data?.ToString());
		}
	}



}