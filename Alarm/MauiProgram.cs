using Alarm.Data;
using Alarm.Services;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using Plugin.Maui.Audio;

namespace Alarm
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			var builder = MauiApp.CreateBuilder();

			builder.UseMauiApp<App>()

				.UseLocalNotification()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
					fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				});

			// Register the AudioManager
			builder.Services.AddSingleton<IAudioManager, AudioManager>();

#if DEBUG
			string dbPath = Path.Combine(FileSystem.AppDataDirectory, "alarm.db3");
			builder.Services.AddSingleton(new AlarmDatabase(dbPath));
			builder.Logging.AddDebug();
#endif
			builder.Services.AddTransient<GoogleAuthService>();

			
			builder.Services.AddSingleton<FacebookAuthService>();
			return builder.Build();
		}
	}
}
