using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Alarm.Models
{
	public class TodoViewModel : INotifyPropertyChanged
	{
		private ObservableCollection<ToDoItem> _todoItems;
		private ICommand _markAsDoneCommand;

		// Property to hold the list of to-do items
		public ObservableCollection<ToDoItem> TodoItems
		{
			get => _todoItems;
			set
			{
				if (_todoItems != value)
				{
					_todoItems = value;
					OnPropertyChanged();
				}
			}
		}

		// Command to mark a task as done
		public ICommand MarkAsDoneCommand
		{
			get
			{
				if (_markAsDoneCommand == null)
				{
					_markAsDoneCommand = new Command<ToDoItem>(MarkAsDone);
				}
				return _markAsDoneCommand;
			}
		}

		// Constructor to initialize the list of to-do items
		public TodoViewModel()
		{
			TodoItems = new ObservableCollection<ToDoItem>
			{
				new ToDoItem { Id = 1, Title = "Buy groceries", IsDone = false },
				new ToDoItem { Id = 2, Title = "Clean the house", IsDone = false },
				new ToDoItem { Id = 3, Title = "Complete MAUI project", IsDone = false }
			};
		}

		// Method to mark a task as done
		private void MarkAsDone(ToDoItem item)
		{
			if (item != null)
			{
				item.IsDone = true;
				OnPropertyChanged(nameof(TodoItems));  // Notify that the list has changed
			}
		}

		// PropertyChanged event for data binding
		public event PropertyChangedEventHandler PropertyChanged;

		// Method to raise the PropertyChanged event
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	// ToDoItem model (if not already created)
	public class ToDoItem
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public bool IsDone { get; set; }
	}
}
