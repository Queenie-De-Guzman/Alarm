using Microsoft.Maui.Authentication;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

public class GoogleAuthService
{
	private readonly string _clientId = "910926851619-bd0c6jqfonbtoqu5i38ckieimvk87tas.apps.googleusercontent.com";

	private readonly string _redirectUri = "com.alarm.reminderapp:/oauth2redirect";

	private readonly string[] _scopes = {
		"openid",
		"profile",
		"email",
		"https://www.googleapis.com/auth/userinfo.email",
		"https://www.googleapis.com/auth/userinfo.profile"
	};

	public async Task<GoogleUser> AuthenticateAsync()
	{
		try
		{
			using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));

			var authResult = await WebAuthenticator.Default.AuthenticateAsync(
				new WebAuthenticatorOptions
				{
					Url = new Uri($"https://accounts.google.com/o/oauth2/v2/auth" +
								 $"?client_id={_clientId}" +
								 $"&redirect_uri={Uri.EscapeDataString(_redirectUri)}" +
								 $"&response_type=code" +
								 $"&scope={string.Join(" ", _scopes.Select(Uri.EscapeDataString))}" +
								 $"&prompt=select_account"),
					CallbackUrl = new Uri(_redirectUri),
					PrefersEphemeralWebBrowserSession = true
				});

			var authCode = authResult?.Properties["code"];
			Console.WriteLine("Authorization code: " + authCode);

			var tokens = await ExchangeCodeForTokensAsync(authCode);

			if (string.IsNullOrWhiteSpace(tokens?.AccessToken))
				throw new Exception("Access token is null or empty.");

			var userInfo = await GetUserInfoAsync(tokens.AccessToken);
			return userInfo;
		}
		catch (TaskCanceledException)
		{
			Console.WriteLine("Authentication timed out or was canceled.");
			throw new Exception("Authentication timed out or was canceled.");
		}
		catch (Exception ex)
		{
			Console.WriteLine("Authentication error: " + ex.Message);
			throw;
		}
	}

	private async Task<TokenResponse> ExchangeCodeForTokensAsync(string code)
	{
		using var httpClient = new HttpClient();

		var tokenRequestParams = new Dictionary<string, string>
		{
			{ "code", code },
			{ "client_id", _clientId },
			{ "redirect_uri", _redirectUri },
			{ "grant_type", "authorization_code" }
		};

		var tokenRequest = new FormUrlEncodedContent(tokenRequestParams);
		var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenRequest);

		var responseJson = await response.Content.ReadAsStringAsync();
		Console.WriteLine("Token response JSON: " + responseJson);

		if (!response.IsSuccessStatusCode)
		{
			throw new Exception($"Token request failed: {response.StatusCode} - {responseJson}");
		}

		var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseJson);
		Console.WriteLine("Access token: " + tokenResponse?.AccessToken);

		return tokenResponse;
	}

	private async Task<GoogleUser> GetUserInfoAsync(string accessToken)
	{
		using var httpClient = new HttpClient();
		httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

		var response = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v3/userinfo");
		var userInfoJson = await response.Content.ReadAsStringAsync();

		Console.WriteLine($"UserInfo response status: {response.StatusCode}");
		Console.WriteLine("UserInfo response body: " + userInfoJson);

		if (!response.IsSuccessStatusCode)
		{
			throw new Exception($"User info request failed: {response.StatusCode} - {userInfoJson}");
		}

		return JsonSerializer.Deserialize<GoogleUser>(userInfoJson);
	}
}

public class TokenResponse
{
	[JsonPropertyName("access_token")]
	public string AccessToken { get; set; }

	[JsonPropertyName("refresh_token")]
	public string RefreshToken { get; set; }

	[JsonPropertyName("expires_in")]
	public int ExpiresIn { get; set; }

	[JsonPropertyName("token_type")]
	public string TokenType { get; set; }

	[JsonPropertyName("id_token")]
	public string IdToken { get; set; }
}

public class GoogleUser
{
	[JsonPropertyName("sub")]
	public string Sub { get; set; }

	[JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonPropertyName("given_name")]
	public string GivenName { get; set; }

	[JsonPropertyName("family_name")]
	public string FamilyName { get; set; }

	[JsonPropertyName("picture")]
	public string Picture { get; set; }

	[JsonPropertyName("email")]
	public string Email { get; set; }

	[JsonPropertyName("email_verified")]
	public bool EmailVerified { get; set; }
}
