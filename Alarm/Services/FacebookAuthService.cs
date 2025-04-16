using Microsoft.Maui.Authentication;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Alarm.Services
{
	public class FacebookAuthService
	{
		private readonly string _appId = "641791538781902";
		// WARNING: App secret should ideally be stored on a server, not in client code
		private readonly string _appSecret = "b84a7166c9c8170ce911cf237fae9c41";
		private readonly string _redirectUri = "https://queenie-de-guzman.github.io/callback/Callback.html";
		private readonly string _appScheme = "com.alarm.reminderapp";
		private readonly string[] _scopes = { "email", "public_profile" };

		public async Task<FacebookUser> AuthenticateAsync()
		{
			try
			{
				// Create the authentication request with the correct redirect URI
				WebAuthenticatorResult authResult = await WebAuthenticator.Default.AuthenticateAsync(
					new Uri($"https://www.facebook.com/v18.0/dialog/oauth" +
						   $"?client_id={_appId}" +
						   $"&redirect_uri={Uri.EscapeDataString(_redirectUri)}" +
						   $"&response_type=code" +
						   $"&scope={string.Join(",", _scopes)}" +
						   $"&state={Guid.NewGuid()}"),
					new Uri($"{_appScheme}:/oauth2redirect"));

				// Extract the authorization code from the result
				if (!authResult.Properties.TryGetValue("code", out var authCode))
				{
					throw new Exception("Authorization code not found in the authentication result");
				}

				Console.WriteLine($"Received auth code: {authCode}");

				// Exchange the authorization code for an access token
				var tokenResponse = await ExchangeCodeForAccessTokenAsync(authCode);

				// Get user information using the access token
				var userInfo = await GetUserInfoAsync(tokenResponse.AccessToken);

				return userInfo;
			}
			catch (TaskCanceledException)
			{
				// User canceled authentication
				Console.WriteLine("Authentication was canceled by the user");
				throw new Exception("Authentication was canceled");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Facebook authentication failed: {ex.Message}");
				Console.WriteLine($"Stack trace: {ex.StackTrace}");
				throw;
			}
		}

		private async Task<FacebookTokenResponse> ExchangeCodeForAccessTokenAsync(string code)
		{
			using var httpClient = new HttpClient();

			var tokenUrl = $"https://graph.facebook.com/v18.0/oauth/access_token" +
						  $"?client_id={_appId}" +
						  $"&client_secret={_appSecret}" +
						  $"&redirect_uri={Uri.EscapeDataString(_redirectUri)}" +
						  $"&code={code}";

			var response = await httpClient.GetAsync(tokenUrl);

			string responseContent = await response.Content.ReadAsStringAsync();
			Console.WriteLine($"Token response: {responseContent}");

			if (!response.IsSuccessStatusCode)
			{
				throw new Exception($"Failed to exchange code for token: {responseContent}");
			}

			var options = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			};

			return JsonSerializer.Deserialize<FacebookTokenResponse>(responseContent, options);
		}

		private async Task<FacebookUser> GetUserInfoAsync(string accessToken)
		{
			using var httpClient = new HttpClient();

			// Request user data including fields we want
			var userInfoUrl = $"https://graph.facebook.com/v18.0/me" +
							 $"?fields=id,name,first_name,last_name,email,picture" +
							 $"&access_token={accessToken}";

			var response = await httpClient.GetAsync(userInfoUrl);
			var userInfoJson = await response.Content.ReadAsStringAsync();

			Console.WriteLine($"User info response: {userInfoJson}");

			if (!response.IsSuccessStatusCode)
			{
				throw new Exception($"Failed to get user info: {userInfoJson}");
			}

			var options = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			};

			return JsonSerializer.Deserialize<FacebookUser>(userInfoJson, options);
		}
	}

	// Model classes remain the same
	public class FacebookTokenResponse
	{
		[JsonPropertyName("access_token")]
		public string AccessToken { get; set; }

		[JsonPropertyName("token_type")]
		public string TokenType { get; set; }

		[JsonPropertyName("expires_in")]
		public int ExpiresIn { get; set; }
	}

	public class FacebookUser
	{
		[JsonPropertyName("id")]
		public string Id { get; set; }

		[JsonPropertyName("name")]
		public string Name { get; set; }

		[JsonPropertyName("first_name")]
		public string FirstName { get; set; }

		[JsonPropertyName("last_name")]
		public string LastName { get; set; }

		[JsonPropertyName("email")]
		public string Email { get; set; }

		[JsonPropertyName("picture")]
		public FacebookPicture Picture { get; set; }
	}

	public class FacebookPicture
	{
		[JsonPropertyName("data")]
		public FacebookPictureData Data { get; set; }
	}

	public class FacebookPictureData
	{
		[JsonPropertyName("height")]
		public int Height { get; set; }

		[JsonPropertyName("width")]
		public int Width { get; set; }

		[JsonPropertyName("url")]
		public string Url { get; set; }

		[JsonPropertyName("is_silhouette")]
		public bool IsSilhouette { get; set; }
	}
}