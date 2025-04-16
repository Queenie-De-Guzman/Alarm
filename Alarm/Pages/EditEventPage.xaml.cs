using System;
using System.Threading.Tasks;
using Alarm.Data;
using Alarm.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Alarm.Pages
{
	public partial class EditEventPage : ContentPage
	{
		private readonly AlarmDatabase _database;
		private readonly CalendarEvent _event;
		private readonly Func<Task> _refreshCallback;
		private string _selectedColor;

		public EditEventPage(AlarmDatabase database, CalendarEvent calendarEvent, Func<Task> refreshCallback)
		{
			InitializeComponent();
			_database = database;
			_event = calendarEvent;
			_refreshCallback = refreshCallback;
			_selectedColor = calendarEvent.Color;

			// Set the title
			Title = "Edit Event";

			// Fill in existing data
			TitleEntry.Text = _event.Title;
			DateLabel.Text = _event.Date.ToString("MMMM d, yyyy");
			TimeEntry.Text = _event.TimeDisplay;
			NotesEditor.Text = _event.Notes;
			AllDaySwitch.IsToggled = _event.IsAllDay;

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
					BorderWidth = (color == _selectedColor) ? 3 : 0,
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
				// Update event
				_event.Title = TitleEntry.Text;
				_event.TimeDisplay = TimeEntry.Text;
				_event.Color = _selectedColor;
				_event.Notes = NotesEditor.Text;
				_event.IsAllDay = AllDaySwitch.IsToggled;

				// Save to database
				await _database.SaveEventAsync(_event);

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
				await DisplayAlert("Error", $"Failed to update event: {ex.Message}", "OK");
			}
		}

		private async void CancelButton_Clicked(object sender, EventArgs e)
		{
			await Navigation.PopModalAsync();
		}

		private void AllDaySwitch_Toggled(object sender, ToggledEventArgs e)
		{
			TimeEntry.IsEnabled = !e.Value;
			if (e.Value && TimeEntry.Text != "All day")
			{
				TimeEntry.Text = "All day";
				TimeEntry.TextColor = Colors.Gray;
			}
			else if (!e.Value && TimeEntry.Text == "All day")
			{
				TimeEntry.Text = "";
				TimeEntry.TextColor = Colors.Black;
				TimeEntry.Focus();
			}
		}

		private async void DeleteButton_Clicked(object sender, EventArgs e)
		{
			bool confirm = await DisplayAlert("Delete Event",
				$"Are you sure you want to delete '{_event.Title}'?",
				"Delete", "Cancel");

			if (confirm)
			{
				await _database.DeleteEventAsync(_event);

				// Refresh calendar
				if (_refreshCallback != null)
				{
					await _refreshCallback();
				}

				// Navigate back
				await Navigation.PopModalAsync();
			}
		}
	}
}