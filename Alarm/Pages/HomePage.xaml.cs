using Firebase.Auth;
using Microsoft.Maui.Storage;
using System;
using Alarm.Data;
using Alarm.Models;

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
			string userEmail = Preferences.Get("UserEmail", "user@example.com");
			string photoUrl = Preferences.Get("UserPhotoUrl", "");
			string profileUrl = Preferences.Get("UserPhotoUrl", "https://via.placeholder.com/70");

			WelcomeLabel.Text = "Welcome!";
			UserEmailLabel.Text = userEmail;

			ProfileImage.Source = ImageSource.FromUri(new Uri(profileUrl));
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
		private async void OnTodoListClicked(object sender, EventArgs e)
		{
			// Example of navigating to a different page when the button is clicked
			await Navigation.PushModalAsync(new TodoPage());
		}
		private async void OnNotesClicked(object sender, EventArgs e)
		{
			await Navigation.PushModalAsync(new NotesPage());
		}





		private async void OnCalendarClicked(object sender, EventArgs e)
		{
			string dbPath = Path.Combine(FileSystem.AppDataDirectory, "alarm.db");
			var database = new AlarmDatabase(dbPath);
			await Navigation.PushModalAsync(new CalendarPage(database));
		}
		
	}
}

