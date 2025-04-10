using Firebase.Auth;
using Microsoft.Maui.Storage;
using System;

namespace Alarm.Pages
{
	public partial class HomePage : ContentPage
	{
		public HomePage()
		{
			InitializeComponent();
			LoadUserData();
		}

		private void LoadUserData()
		{
			// Kunin ang email mula sa local storage (Preferences)
			string userEmail = Preferences.Get("UserEmail", "user@example.com");

			WelcomeLabel.Text = "Welcome!";
			UserEmailLabel.Text = userEmail;
		}

		private async void OnLogoutClicked(object sender, EventArgs e)
		{
			// Clear saved email (logout user)
			Preferences.Remove("UserEmail");

			// Redirect to Login Page
			await Navigation.PushModalAsync(new LoginPage());
		}
		private async void OnAlarmClicked(object sender, EventArgs e)
		{
			// Navigate to Alarm setup page or open alarm dialog
			await Navigation.PushModalAsync(new AlarmPage());
		}
	}
}

