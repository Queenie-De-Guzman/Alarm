using Firebase.Auth;
using Firebase.Auth.Providers;
using Microsoft.Maui.Storage;
using System;
using System.Threading.Tasks;

namespace Alarm.Pages
{
	public partial class SignUp : ContentPage
	{
		private readonly FirebaseAuthClient _authClient;

		public SignUp()
		{
			InitializeComponent();
			_authClient = new FirebaseAuthClient(new FirebaseAuthConfig
			{
				ApiKey = "AIzaSyDlkEYudHYVDZ9xF8tAAdH4Zocos1MPMec",
				AuthDomain = "maui-49b65.firebaseapp.com",
				Providers = new FirebaseAuthProvider[]
				{
					new EmailProvider() // Ginamit ang EmailProvider() para sa email registration
                }
			});
		}

		private async void OnSignUpClicked(object sender, EventArgs e)
		{
			try
			{
				var fullName = FullNameEntry.Text?.Trim();
				var email = EmailEntry.Text?.Trim();
				var password = PasswordEntry.Text;
				var confirmPassword = ConfirmPasswordEntry.Text;

				if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) ||
					string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
				{
					await DisplayAlert("Error", "All fields are required.", "OK");
					return;
				}

				if (password != confirmPassword)
				{
					await DisplayAlert("Error", "Passwords do not match.", "OK");
					return;
				}

				var authResult = await _authClient.CreateUserWithEmailAndPasswordAsync(email, password);

				if (authResult?.User != null)
				{
					Preferences.Set("UserEmail", authResult.User.Info.Email);
					await DisplayAlert("Success", "Account created successfully!", "OK");

					await Navigation.PushModalAsync(new HomePage());
				}
				else
				{
					await DisplayAlert("Error", "Sign-up failed. Try again.", "OK");
				}
			}
			catch (Exception ex)
			{
				await DisplayAlert("Sign-Up Failed", ex.Message, "OK");
			}
		}

		private async void OnLoginTapped(object sender, EventArgs e)
		{
			var loginPage = Handler.MauiContext.Services.GetService<LoginPage>();
			await Navigation.PushModalAsync(loginPage);
		}
	}
}
