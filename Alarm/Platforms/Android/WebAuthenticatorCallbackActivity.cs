using Android.App;
using Android.Content;
using Android.Content.PM;
using Microsoft.Maui.Authentication;

namespace Alarm.Platforms.Android
{
	[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
	[IntentFilter(new[] { Intent.ActionView },
				 Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
				 DataScheme = "com.alarm.reminderapp")]
	public class WebAuthenticatorCallbackActivity : Microsoft.Maui.Authentication.WebAuthenticatorCallbackActivity
	{
		// We don't need to add any code here, just the attributes above
	}
}