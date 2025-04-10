using Android.App;
using Android.Content;
using Microsoft.Maui.Authentication;
using Android.Content.PM;

namespace Alarm.Platforms.Android
{
	[Activity(NoHistory = true, Exported = true, LaunchMode = LaunchMode.SingleTop)]
	[IntentFilter(new[] { Intent.ActionView },
		Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
		DataScheme = "https",
		DataHost = "maui-49b65.firebaseapp.com",
	 DataPath = "/__/auth/handler")]
	public class WebAuthenticatorCallbackActivity : Microsoft.Maui.Authentication.WebAuthenticatorCallbackActivity
	{
	}
}
