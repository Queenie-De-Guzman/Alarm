using Microsoft.Maui.Controls;
using Plugin.Maui.Audio;
using Plugin.LocalNotification;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Alarm.Data;
using Alarm.Models;
using System.Xml.Linq;
using Plugin.LocalNotification.AndroidOption;
using Microsoft.Maui.Storage;

namespace Alarm.Pages
{
	public partial class AlarmPage : ContentPage
	{
		public ObservableCollection<AlarmModel> Alarms { get; set; } = new ObservableCollection<AlarmModel>();
		private AlarmModel? selectedAlarm;
		private IAudioPlayer? player;
		private System.Timers.Timer checkTimer;
		private readonly AlarmDatabase _databaseHelper;

		public AlarmPage()
		{
			InitializeComponent();

			string dbPath = Path.Combine(FileSystem.AppDataDirectory, "alarms.db");
			_databaseHelper = new AlarmDatabase(dbPath);

			LoadAlarmsAsync();

			checkTimer = new System.Timers.Timer(1000);
			checkTimer.Elapsed += CheckAlarms;
			checkTimer.Start();
		}

		private async Task LoadAlarmsAsync()
		{
			var allAlarms = await _databaseHelper.GetAlarmsAsync();
			var currentUserEmail = Preferences.Get("UserEmail", "user@example.com");

			var userAlarms = allAlarms.Where(a => a.UserEmail == currentUserEmail);
			Alarms = new ObservableCollection<AlarmModel>(userAlarms);
			alarmsCollectionView.ItemsSource = Alarms;
		}

		private void OnAlarmSelected(object sender, SelectionChangedEventArgs e)
		{
			selectedAlarm = e.CurrentSelection.FirstOrDefault() as AlarmModel;
			if (selectedAlarm != null)
			{
				titleEntry.Text = selectedAlarm.Title;
				alarmDatePicker.Date = selectedAlarm.AlarmDateTime.Date;
				alarmTimePicker.Time = selectedAlarm.AlarmDateTime.TimeOfDay;

				statusLabel.Text = $"Selected: {selectedAlarm.Title}";
			}
		}

		private async void OnAddAlarmClicked(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(titleEntry.Text))
			{
				await DisplayAlert("Error", "Please enter an alarm title.", "OK");
				return;
			}

			var userEmail = Preferences.Get("UserEmail", "user@example.com");

			var newAlarm = new AlarmModel
			{
				Title = titleEntry.Text.Trim(),
				AlarmDateTime = alarmDatePicker.Date + alarmTimePicker.Time,
				Triggered = false,
				UserEmail = userEmail
			};

			await _databaseHelper.SaveAlarmAsync(newAlarm);
			Alarms.Add(newAlarm);

			statusLabel.Text = $"Alarm added for {newAlarm.AlarmDateTime:yyyy-MM-dd HH:mm}";

			TriggerNotification(newAlarm);
		}

		private async void OnUpdateAlarmClicked(object sender, EventArgs e)
		{
			if (selectedAlarm == null)
			{
				await DisplayAlert("Update Error", "Please select an alarm to update.", "OK");
				return;
			}

			selectedAlarm.Title = titleEntry.Text;
			selectedAlarm.AlarmDateTime = alarmDatePicker.Date + alarmTimePicker.Time;
			selectedAlarm.Triggered = false;

			await _databaseHelper.SaveAlarmAsync(selectedAlarm);
			RefreshCollectionView();

			statusLabel.Text = "Alarm updated.";
		}

		private async void OnDeleteSwipeInvoked(object sender, EventArgs e)
		{
			if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is AlarmModel alarmToDelete)
			{
				await _databaseHelper.DeleteAlarmAsync(alarmToDelete);
				Alarms.Remove(alarmToDelete);
				RefreshCollectionView();

				statusLabel.Text = $"Deleted alarm: {alarmToDelete.Title}";
			}
		}

		private async void OnDeleteAlarmClicked(object sender, EventArgs e)
		{
			if (selectedAlarm != null)
			{
				await _databaseHelper.DeleteAlarmAsync(selectedAlarm);
				Alarms.Remove(selectedAlarm);
				selectedAlarm = null;

				statusLabel.Text = "Alarm deleted.";

				titleEntry.Text = "";
				alarmDatePicker.Date = DateTime.Now;
				alarmTimePicker.Time = DateTime.Now.TimeOfDay;
			}
		}

		private void CheckAlarms(object? sender, System.Timers.ElapsedEventArgs e)
		{
			foreach (var alarm in Alarms.Where(a => !a.Triggered && DateTime.Now >= a.AlarmDateTime))
			{
				alarm.Triggered = true;

				Dispatcher.Dispatch(async () =>
				{
					TriggerNotification(alarm);
					await PlayAlarmSound();
					await DisplayAlert("Alarm", $"Alarm: {alarm.Title}", "OK");
					StopAlarmSound();
				});
			}
		}

		private async Task PlayAlarmSound()
		{
			try
			{
				var file = await FileSystem.OpenAppPackageFileAsync("alarm_sound.mp3");
				player = AudioManager.Current.CreatePlayer(file);
				player?.Play();
			}
			catch (Exception ex)
			{
				await DisplayAlert("Error", $"Failed to play sound: {ex.Message}", "OK");
			}
		}

		private void StopAlarmSound()
		{
			if (player?.IsPlaying == true)
			{
				player.Stop();
			}
		}

		private void TriggerNotification(AlarmModel alarm)
		{
			var request = new NotificationRequest
			{
				NotificationId = alarm.GetHashCode(),
				Title = "Alarm",
				Description = alarm.Title,
				ReturningData = "alarm_data",
				Schedule = new NotificationRequestSchedule
				{
					NotifyTime = alarm.AlarmDateTime
				},
				Android = new AndroidOptions
				{
					ChannelId = "alarm_channel"
				}
			};

			LocalNotificationCenter.Current.Show(request);
		}

		private void RefreshCollectionView()
		{
			var temp = Alarms.ToList();
			Alarms.Clear();
			foreach (var alarm in temp)
				Alarms.Add(alarm);
		}

		private async void OnBackButtonHome(object sender, EventArgs e)
		{
			await Navigation.PushModalAsync(new HomePage());
		}
	}
}
