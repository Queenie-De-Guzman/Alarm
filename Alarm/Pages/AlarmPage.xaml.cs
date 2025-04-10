using Microsoft.Maui.Controls;
using Plugin.Maui.Audio;
using System;
using System.Threading.Tasks;
using Plugin.LocalNotification;

namespace Alarm.Pages
{
	public partial class AlarmPage : ContentPage
	{
		private DateTime alarmDateTime;
		private System.Timers.Timer checkTimer;
		private IAudioPlayer? player;
		private bool alarmTriggered = false;

		public AlarmPage()
		{
			InitializeComponent();
			checkTimer = new System.Timers.Timer(1000); // 1-second interval
			checkTimer.Elapsed += CheckAlarm;
		}

		private void OnSetAlarmClicked(object sender, EventArgs e)
		{
			alarmDateTime = alarmDatePicker.Date + alarmTimePicker.Time;
			statusLabel.Text = $"Alarm set for {alarmDateTime:yyyy-MM-dd HH:mm}";
			alarmTriggered = false;
			checkTimer.Start();
		}

		private void CheckAlarm(object? sender, System.Timers.ElapsedEventArgs e)
		{
			if (!alarmTriggered && DateTime.Now >= alarmDateTime)
			{
				alarmTriggered = true;
				checkTimer.Stop();

				Dispatcher.Dispatch(async () =>
				{
					// Trigger notification
					TriggerNotification();

					// Play the alarm sound
					await PlayAlarmSoundDirectly();

					// Show alert and stop sound after user clicks OK
					await DisplayAlert("Alarm", "Time's up!", "OK");

					// Stop sound after alert is closed
					StopAlarmSound();

					statusLabel.Text = "Alarm triggered.";
				});
			}
		}

		private async Task PlayAlarmSoundDirectly()
		{
			try
			{
				var filePath = "alarm_sound.mp3";
				var file = await FileSystem.OpenAppPackageFileAsync(filePath);

				if (file == null)
				{
					await DisplayAlert("Error", "Audio file not found. Make sure it's in Resources/Raw with Build Action: MauiAsset.", "OK");
					return;
				}

				player = AudioManager.Current.CreatePlayer(file);

				if (player == null)
				{
					await DisplayAlert("Error", "Failed to create audio player.", "OK");
					return;
				}

				player.Play();
			}
			catch (Exception ex)
			{
				await DisplayAlert("Error", $"Failed to play alarm sound: {ex.Message}", "OK");
			}
		}

		private void StopAlarmSound()
		{
			if (player != null && player.IsPlaying)
			{
				player.Stop();
			}
		}

		private void TriggerNotification()
		{
			var notification = new NotificationRequest
			{
				NotificationId = 1,
				Title = "Alarm",
				Description = "Time's up!",
				ReturningData = "Dummy data",
				Schedule = new NotificationRequestSchedule
				{
					NotifyTime = alarmDateTime
				}
			};

			LocalNotificationCenter.Current.Show(notification);
		}
	}
}
