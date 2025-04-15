using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Alarm.Data;
using Alarm.Models;
using System.Windows.Input;

namespace Alarm.Pages
{
    public partial class TodoPage : ContentPage
    {
		private readonly AlarmDatabase _database;
		public ICommand DeleteCommand { get; }
		public ICommand MarkDoneCommand { get; }
		public ObservableCollection<TodoItem> TodoItems { get; set; }
		public TodoPage()
		{
			InitializeComponent();

			string dbPath = Path.Combine(FileSystem.AppDataDirectory, "AlarmApp.db3");
			_database = new AlarmDatabase(dbPath);

			TodoItems = new ObservableCollection<TodoItem>();
			BindingContext = this;

			DeleteCommand = new Command<TodoItem>(async (item) => await DeleteItem(item));
			MarkDoneCommand = new Command<TodoItem>(async (item) => await MarkAsDone(item));

			LoadTasks();
		}

		private async void LoadTasks()
		{
			var items = await _database.GetTodosAsync();
			TodoItems.Clear();
			foreach (var item in items)
				TodoItems.Add(item);
		}

		// Add Todo Item with timestamp
		private async void OnAddTodoClicked(object sender, EventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(TodoEntry.Text))
			{
				var newItem = new TodoItem
				{
					Text = TodoEntry.Text,
					Date = DateTime.Now,
					IsDone = false
				};
				await _database.SaveTodoAsync(newItem);
				TodoItems.Add(newItem);
				TodoEntry.Text = string.Empty;
			}
		}
		private async Task DeleteItem(TodoItem item)
		{
			await _database.DeleteTodoAsync(item);
			TodoItems.Remove(item);
		}

		private async Task MarkAsDone(TodoItem item)
		{
			item.IsDone = true;
			await _database.SaveTodoAsync(item);
			TodoItems.Remove(item);
		}

		private async void OnBackButtonHome(object sender, EventArgs e)
		{
			// Example of navigating to a different page when the button is clicked
			await Navigation.PushModalAsync(new HomePage());
		}
		private async void OnMenuPageClicked(object sender, EventArgs e)
		{
			await Navigation.PushModalAsync(new MenuPage());
		}

	}

}
