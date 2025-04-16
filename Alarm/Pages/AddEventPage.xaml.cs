using System;
using System.Threading.Tasks;
using Alarm.Data;
using Alarm.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Alarm.Pages
{
	public partial class AddEventPage : ContentPage
	{
		private readonly AlarmDatabase _database;
		private readonly DateTime _selectedDate;
		private readonly string _userId;
		private readonly Func<Task> _refreshCallback;
		private string _selectedColor = "#4CAF50";

		public AddEventPage(AlarmDatabase database, DateTime selectedDate, string userId, Func<Task> refreshCallback)
		{
			InitializeComponent();
			_database = database;
			_selectedDate = selectedDate;
			_userId = userId;
			_refreshCallback = refreshCallback;

			// Set the date display
			DateLabel.Text = selectedDate.ToString("MMMM d, yyyy");

			// Initialize color picker
			InitializeColorPicker();
		}

		private void InitializeColorPicker()
		{
			string[] colors = { "#4CAF50", "#2196F3", "#FF9800", "#F44336", "#9C27B0", "#009688", "#795548" };

			var colorStack = new HorizontalStackLayout
			{
				Spacing = 10,
				HorizontalOptions = LayoutOptions.Center
			};

			foreach (var color in colors)
			{
				var colorButton = new Button
				{
					BackgroundColor = Color.FromArgb(color),
					WidthRequest = 40,
					HeightRequest = 40,
					CornerRadius = 20,
					Margin = new Thickness(5),
					BorderWidth = (_selectedColor == color) ? 3 : 0,
					BorderColor = Colors.Black
				};

				// Set command for color selection
				colorButton.Clicked += (sender, e) =>
				{
					// Update selected color
					_selectedColor = color;

					// Reset all borders
					foreach (var child in colorStack.Children)
					{
						if (child is Button btn)
						{
							btn.BorderWidth = 0;
						}
					}

					// Highlight selected color
					colorButton.BorderWidth = 3;
				};

				colorStack.Add(colorButton);
			}

			ColorPickerContainer.Children.Add(colorStack);
		}

		private async void SaveButton_Clicked(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(TitleEntry.Text))
			{
				await DisplayAlert("Error", "Please enter a title for the event", "OK");
				return;
			}

			try
			{
				// Create new event
				var newEvent = new CalendarEvent
				{
					Title = TitleEntry.Text,
					Date = _selectedDate,
					TimeDisplay = TimeEntry.Text ?? "All day",
					Color = _selectedColor,
					UserId = _userId,
					Notes = NotesEditor.Text,
					IsAllDay = AllDaySwitch.IsToggled,
					Created = DateTime.Now
				};

				// Save to database
				await _database.SaveEventAsync(newEvent);

				// Refresh calendar
				if (_refreshCallback != null)
				{
					await _refreshCallback();
				}

				// Navigate back
				await Navigation.PopModalAsync();
			}
			catch (Exception ex)
			{
				await DisplayAlert("Error", $"Failed to save event: {ex.Message}", "OK");
			}
		}

		private async void CancelButton_Clicked(object sender, EventArgs e)
		{
			await Navigation.PopModalAsync();
		}

		private void AllDaySwitch_Toggled(object sender, ToggledEventArgs e)
		{
			TimeEntry.IsEnabled = !e.Value;
			if (e.Value)
			{
				TimeEntry.Text = "All day";
				TimeEntry.TextColor = Colors.Gray;
			}
			else
			{
				TimeEntry.Text = "";
				TimeEntry.TextColor = Colors.Black;
				TimeEntry.Focus();
			}
		}
	}
}