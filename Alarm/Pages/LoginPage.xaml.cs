using Alarm.Services;
using Firebase.Auth;
using Firebase.Auth.Providers;
using Microsoft.Maui.Authentication;
using System;
using System.Threading.Tasks;

namespace Alarm.Pages
{
	public partial class LoginPage : ContentPage
	{
		private readonly FirebaseAuthClient _authClient;
		private readonly GoogleAuthService _authService;
		private readonly FacebookAuthService _facebookAuthService;

		public LoginPage()
		{
			InitializeComponent();
			_authService = new GoogleAuthService();
			_facebookAuthService = new FacebookAuthService();
			_authClient = new FirebaseAuthClient(new FirebaseAuthConfig
			{
				ApiKey = "AIzaSyDlkEYudHYVDZ9xF8tAAdH4Zocos1MPMec",
				AuthDomain = "maui-49b65.firebaseapp.com",
				Providers = new FirebaseAuthProvider[]
				{
					new EmailProvider(),
					new GoogleProvider(),
					new FacebookProvider()
				}
			});
		}


		private async void OnLoginClicked(object sender, EventArgs e)
		{
			try
			{
				var email = EmailEntry.Text?.Trim();
				var password = PasswordEntry.Text?.Trim();

				if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
				{
					await DisplayAlert("Error", "Please enter email and password", "OK");
					return;
				}

				var authResult = await _authClient.SignInWithEmailAndPasswordAsync(email, password);

				if (authResult?.User != null)
				{
					Preferences.Set("UserEmail", authResult.User.Info.Email); //ave email

					await DisplayAlert("Success", $"Welcome, {authResult.User.Info.Email}!", "OK");
					await Navigation.PushModalAsync(new HomePage());
				}
				else
				{
					await DisplayAlert("Login Failed", "Invalid credentials. Please try again.", "OK");
				}
			}
			catch (Exception ex)
			{
				await DisplayAlert("Login Failed", ex.Message, "OK");
			}
		}
		private async void OnGoogleLoginClicked(object sender, EventArgs e)
		{
			try
			{
				var user = await _authService.AuthenticateAsync();

				if (user != null)
				{
					Preferences.Set("UserEmail", user.Email); // Save Google email
					await DisplayAlert("Success", $"Logged in as {user.Name}", "OK");
					await Navigation.PushModalAsync(new HomePage());
				}
			}
			catch (Exception ex)
			{
				await DisplayAlert("Error", $"Login failed: {ex.Message}", "OK");
			}
		}

		private async void OnFacebookLoginClicked(object sender, EventArgs e)
		{
			try
			{
				var facebookUser = await _facebookAuthService.AuthenticateAsync();
				Preferences.Set("UserEmail", facebookUser.Email);
				Preferences.Set("UserPhotoUrl", facebookUser.Picture.Data.Url); ; // Save Facebook email
				await DisplayAlert("Success", $"Welcome {facebookUser.Name}!", "OK");
				await Navigation.PushModalAsync(new HomePage());

			}
			catch (Exception ex)
			{
				await DisplayAlert("Error", $"Login failed: {ex.Message}", "OK");
			}
		}










		private async void OnSignUpTapped(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new SignUp()); // Navigate to SignUp Page
		}
	}
}