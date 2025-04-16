using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Alarm.Models;
using Alarm.Data;
using System.Threading.Tasks;

namespace Alarm.Pages
{
	public partial class CalendarPage : ContentPage
	{
		private DateTime currentMonth;
		private Dictionary<DateTime, Grid> dateCells = new Dictionary<DateTime, Grid>();
		private DateTime? selectedDate = null;
		private ObservableCollection<CalendarEvent> events = new ObservableCollection<CalendarEvent>();
		private Dictionary<DateTime, List<CalendarEvent>> eventsByDate = new Dictionary<DateTime, List<CalendarEvent>>();
		private readonly AlarmDatabase _database;
		private string _userId = "default"; // In a real app, get this from authentication

		public bool HasEvents => events.Count > 0;

		public CalendarPage(AlarmDatabase database)
		{
			InitializeComponent();
			_database = database;
			currentMonth = DateTime.Now;

			// Bind events collection to the CollectionView
			EventsCollection.ItemsSource = events;

			// Set the binding context for visibility binding
			BindingContext = this;

			// Generate Calendar UI
			GenerateCalendar();

			// Load events from database
			LoadEventsAsync();

			// Select today's date by default
			SelectDate(DateTime.Today);
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			LoadEventsAsync();
		}

		private async Task LoadEventsAsync()
		{
			try
			{
				// Clear existing events
				eventsByDate.Clear();

				// Load events from database
				var allEvents = await _database.GetEventsForUserAsync(_userId);

				// Group by date
				foreach (var evt in allEvents)
				{
					if (!eventsByDate.ContainsKey(evt.Date.Date))
					{
						eventsByDate[evt.Date.Date] = new List<CalendarEvent>();
					}
					eventsByDate[evt.Date.Date].Add(evt);
				}

				// Update UI with events
				foreach (var date in dateCells.Keys)
				{
					UpdateDateCellWithEvents(date);
				}

				// Update currently selected date's events if any
				if (selectedDate.HasValue)
				{
					UpdateEventsList(selectedDate.Value);
				}

				OnPropertyChanged(nameof(HasEvents));
			}
			catch (Exception ex)
			{
				await DisplayAlert("Error", $"Failed to load events: {ex.Message}", "OK");
			}
		}

		private void GenerateCalendar()
		{
			// Update month/year label
			MonthYearLabel.Text = currentMonth.ToString("MMMM yyyy");

			// Clear previous calendar
			CalendarGrid.Children.Clear();
			dateCells.Clear();

			// Get the first day of the month
			DateTime firstDay = new DateTime(currentMonth.Year, currentMonth.Month, 1);

			// Calculate what day of the week the month starts on (0 = Sunday)
			int startDayOfWeek = (int)firstDay.DayOfWeek;

			// Get days in the month
			int daysInMonth = DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month);

			// Generate dates
			int dayCounter = 1;

			// Go through each row
			for (int row = 0; row < 6; row++)
			{
				// Go through each column
				for (int col = 0; col < 7; col++)
				{
					// Skip positions before the first day of the month
					if (row == 0 && col < startDayOfWeek)
					{
						// Empty space
						continue;
					}

					// Stop when we've placed all days of the month
					if (dayCounter > daysInMonth)
					{
						break;
					}

					// Create date cell
					DateTime date = new DateTime(currentMonth.Year, currentMonth.Month, dayCounter);
					Grid dateCell = CreateDateCell(date, col);

					// Track cell in dictionary for later use
					dateCells[date] = dateCell;

					// Add to grid
					CalendarGrid.Add(dateCell, col, row);

					// Update cell with any events
					UpdateDateCellWithEvents(date);

					dayCounter++;
				}
			}

			// Highlight today if visible
			DateTime today = DateTime.Today;
			if (today.Year == currentMonth.Year && today.Month == currentMonth.Month)
			{
				HighlightDate(today, "#E3F2FD", "#1976D2", true);
			}

			// Restore previously selected date highlight if it's in this month
			if (selectedDate.HasValue &&
				selectedDate.Value.Year == currentMonth.Year &&
				selectedDate.Value.Month == currentMonth.Month)
			{
				SelectDate(selectedDate.Value);
			}
			else if (selectedDate.HasValue)
			{
				// Clear events list if the selected date is no longer visible
				events.Clear();
				OnPropertyChanged(nameof(HasEvents));
			}
		}

		private Grid CreateDateCell(DateTime date, int column)
		{
			// Create a grid cell for the date
			Grid cell = new Grid
			{
				RowDefinitions =
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto }
				}
			};

			// Date button
			Button dateButton = new Button
			{
				Text = date.Day.ToString(),
				BackgroundColor = Colors.Transparent,
				TextColor = (column == 0 || column == 6) ? Color.FromArgb("#FF5252") : Color.FromArgb("#333333"),
				FontSize = 14,
				CornerRadius = 20,
				HeightRequest = 40,
				WidthRequest = 40,
				Padding = 0,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};

			// Event indicator box (will be visible if date has events)
			BoxView eventIndicator = new BoxView
			{
				HeightRequest = 4,
				CornerRadius = 2,
				HorizontalOptions = LayoutOptions.Center,
				WidthRequest = 20,
				IsVisible = false
			};

			// Handle date selection
			dateButton.Clicked += (sender, e) => DateButton_Clicked(date);

			// Add elements to cell
			cell.Add(dateButton, 0, 0);
			cell.Add(eventIndicator, 0, 1);

			// Make entire cell tappable
			TapGestureRecognizer tapGesture = new TapGestureRecognizer();
			tapGesture.Tapped += (s, e) => DateButton_Clicked(date);
			cell.GestureRecognizers.Add(tapGesture);

			return cell;
		}

		private void UpdateDateCellWithEvents(DateTime date)
		{
			if (!dateCells.ContainsKey(date))
				return;

			Grid cell = dateCells[date];

			if (cell.Children.Count >= 2 && cell.Children[1] is BoxView eventIndicator)
			{
				bool hasEvents = eventsByDate.ContainsKey(date) && eventsByDate[date].Count > 0;
				eventIndicator.IsVisible = hasEvents;

				if (hasEvents)
				{
					// Use the color of the first event for the indicator
					eventIndicator.Color = Color.FromArgb(eventsByDate[date][0].Color);
				}
			}
		}

		private void DateButton_Clicked(DateTime date)
		{
			SelectDate(date);
		}

		private void SelectDate(DateTime date)
		{
			// Reset previously selected date's styling
			if (selectedDate.HasValue && dateCells.ContainsKey(selectedDate.Value))
			{
				// Reset its color, but keep today's special color if needed
				if (selectedDate.Value.Date == DateTime.Today)
				{
					HighlightDate(selectedDate.Value, "#E3F2FD", "#1976D2", true);
				}
				else
				{
					int column = ((int)selectedDate.Value.DayOfWeek);
					Color textColor = (column == 0 || column == 6) ? Color.FromArgb("#FF5252") : Color.FromArgb("#333333");
					HighlightDate(selectedDate.Value, Colors.Transparent, textColor, false);
				}
			}

			// Set new selected date
			selectedDate = date;

			// Update the selected date label
			SelectedDateLabel.Text = date.ToString("MMMM d, yyyy");

			// Highlight selected date
			HighlightDate(date, "#FF5722", Colors.White, true);

			// Update events list
			UpdateEventsList(date);
		}

		private void UpdateEventsList(DateTime date)
		{
			events.Clear();

			if (eventsByDate.ContainsKey(date))
			{
				foreach (var evt in eventsByDate[date])
				{
					events.Add(evt);
				}
			}

			OnPropertyChanged(nameof(HasEvents));
		}

		private void HighlightDate(DateTime date, object backgroundColor, object textColor, bool highlight)
		{
			if (!dateCells.ContainsKey(date))
				return;

			Grid cell = dateCells[date];
			if (cell.Children.Count > 0 && cell.Children[0] is Button button)
			{
				button.BackgroundColor = backgroundColor is Color ?
					(Color)backgroundColor :
					Color.FromArgb(backgroundColor.ToString());

				button.TextColor = textColor is Color ?
					(Color)textColor :
					Color.FromArgb(textColor.ToString());
			}
		}

		private void PrevMonthButton_Clicked(object sender, EventArgs e)
		{
			currentMonth = currentMonth.AddMonths(-1);
			GenerateCalendar();
		}

		private void NextMonthButton_Clicked(object sender, EventArgs e)
		{
			currentMonth = currentMonth.AddMonths(1);
			GenerateCalendar();
		}

		private async void AddEventButton_Clicked(object sender, EventArgs e)
		{
			if (selectedDate.HasValue)
			{
				await Navigation.PushModalAsync(new AddEventPage(_database, selectedDate.Value, _userId, RefreshEventsCallback));
			}
			else
			{
				await DisplayAlert("No Date Selected", "Please select a date first", "OK");
			}
		}

		private async Task RefreshEventsCallback()
		{
			await LoadEventsAsync();
		}

		private async void EventsCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.CurrentSelection.FirstOrDefault() is CalendarEvent selectedEvent)
			{
				// Show options for the event
				string action = await DisplayActionSheet(
					selectedEvent.Title,
					"Cancel",
					"Delete",
					"View Details", "Edit Event");

				switch (action)
				{
					case "View Details":
						await DisplayAlert(selectedEvent.Title,
							$"Time: {selectedEvent.TimeDisplay}\n" +
							$"Date: {selectedEvent.Date.ToString("MMMM d, yyyy")}\n" +
							$"Notes: {selectedEvent.Notes ?? "None"}",
							"Close");
						break;

					case "Edit Event":
						await Navigation.PushModalAsync(new EditEventPage(_database, selectedEvent, RefreshEventsCallback));
						break;

					case "Delete":
						bool confirm = await DisplayAlert("Delete Event",
							$"Are you sure you want to delete '{selectedEvent.Title}'?",
							"Delete", "Cancel");

						if (confirm)
						{
							await _database.DeleteEventAsync(selectedEvent);
							await LoadEventsAsync();
						}
						break;
				}

				// Clear selection
				EventsCollection.SelectedItem = null;
			}
		}
	}
}