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
	
		public LoginPage()
		{
			InitializeComponent();
			_authService = new GoogleAuthService();
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
					await DisplayAlert("Success", $"Welcome, {authResult.User.Info.Email}!", "OK");

					// Navigate to Home Page
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
					// User successfully authenticated
					await DisplayAlert("Success", $"Logged in as {user.Name}", "OK");

					// Navigate to HomePage directly
					await Navigation.PushAsync(new HomePage());
				}
			}
			catch (Exception ex)
			{
				await DisplayAlert("Error", $"Login failed: {ex.Message}", "OK");
			}
		}


		private async void FacebookLoginClicked(object sender, EventArgs e)
		{
#if ANDROID || IOS || MACCATALYST
			try
			{
				var authResult = await WebAuthenticator.AuthenticateAsync(new WebAuthenticatorOptions
				{
					Url = new Uri("https://www.facebook.com/v12.0/dialog/oauth?" +
								  "client_id=641791538781902" +
								  "&redirect_uri=https://maui-49b65.firebaseapp.com/__/auth/handler" +
								  "&response_type=token" +
								  "&scope=email"),
					CallbackUrl = new Uri("https://maui-49b65.firebaseapp.com/__/auth/handler")
				});

				if (authResult?.Properties.TryGetValue("access_token", out var token) == true)
				{
					var firebaseAuthUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithIdp?key=AIzaSyDlkEYudHYVDZ9xF8tAAdH4Zocos1MPMec";

					var content = new StringContent($"{{\"postBody\":\"access_token={token}&providerId=facebook.com\",\"requestUri\":\"https://maui-49b65.firebaseapp.com/__/auth/handler\",\"returnIdpCredential\":true,\"returnSecureToken\":true}}", System.Text.Encoding.UTF8, "application/json");

					using var client = new HttpClient();
					var response = await client.PostAsync(firebaseAuthUrl, content);
					var jsonResponse = await response.Content.ReadAsStringAsync();

					if (response.IsSuccessStatusCode)
					{
						await DisplayAlert("Success", "Facebook login successful!", "OK");
						await Navigation.PushModalAsync(new HomePage());
					}
					else
					{
						await DisplayAlert("Login Failed", jsonResponse, "OK");
					}
				}
			}
			catch (Exception ex)
			{
				await DisplayAlert("Login Failed", ex.Message, "OK");
			}
#else
			await DisplayAlert("Unsupported", "Facebook login is not supported on this platform.", "OK");
#endif
		}


		private async void OnSignUpTapped(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new SignUp()); // Navigate to SignUp Page
		}
	}
}