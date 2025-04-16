using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Alarm.Data;
using Alarm.Models;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace Alarm.Pages
{
	public partial class NotesPage : ContentPage
	{
		private ObservableCollection<Note> notes;
		private AlarmDatabase database;
		private string userId => Preferences.Get("UserEmail", "guest");
		private Note currentEditingNote;

		public NotesPage()
		{
			InitializeComponent();
			string dbPath = Path.Combine(FileSystem.AppDataDirectory, "alarms.db3");
			database = new AlarmDatabase(dbPath);
			notes = new ObservableCollection<Note>();
			NotesCollectionView.ItemsSource = notes;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			await LoadNotes();
		}

		private async Task LoadNotes()
		{
			var dbNotes = await database.GetNotesForUserAsync(userId);
			notes.Clear();
			foreach (var note in dbNotes)
				notes.Add(note);
		}

		private async void OnSaveNoteClicked(object sender, EventArgs e)
		{
			string noteText = NoteEditor.Text?.Trim();
			if (string.IsNullOrEmpty(noteText))
				return;

			if (currentEditingNote != null)
			{
				// Update existing note
				currentEditingNote.Content = noteText;
				await database.UpdateNoteAsync(currentEditingNote);

				// Find and update the note in the collection
				int index = notes.IndexOf(notes.FirstOrDefault(n => n.Id == currentEditingNote.Id));
				if (index >= 0)
				{
					notes[index] = currentEditingNote;
				}

				currentEditingNote = null;
			}
			else
			{
				// Create new note
				var newNote = new Note
				{
					Content = noteText,
					UserId = userId
				};
				await database.SaveNoteAsync(newNote);
				notes.Add(newNote);
			}

			NoteEditor.Text = string.Empty;
		}

		private async void OnDeleteAllClicked(object sender, EventArgs e)
		{
			if (notes.Count > 0)
			{
				bool confirm = await DisplayAlert("Delete All", "Are you sure you want to delete all notes?", "Yes", "No");
				if (confirm)
				{
					await database.DeleteAllNotesForUserAsync(userId);
					notes.Clear();
					NoteEditor.Text = string.Empty;
					currentEditingNote = null;
				}
			}
		}

		private async void OnNoteSelected(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				// Make sure we have a selected item
				if (e.CurrentSelection == null || e.CurrentSelection.Count == 0)
					return;

				// Get the selected note
				var selectedNote = e.CurrentSelection[0] as Note;
				if (selectedNote == null)
					return;

				// Load the selected note content into the editor
				NoteEditor.Text = selectedNote.Content;
				currentEditingNote = selectedNote;

				// Optionally scroll to the editor if needed
				await ScrollToNoteEditor();

				// Deselect the item
				((CollectionView)sender).SelectedItem = null;
			}
			catch (Exception ex)
			{
				await DisplayAlert("Error", $"Could not open note: {ex.Message}", "OK");
			}
		}

		private async void OnEditNoteClicked(object sender, EventArgs e)
		{
			if (sender is Button button && button.CommandParameter is int noteId)
			{
				var noteToEdit = notes.FirstOrDefault(n => n.Id == noteId);
				if (noteToEdit != null)
				{
					NoteEditor.Text = noteToEdit.Content;
					currentEditingNote = noteToEdit;
					await ScrollToNoteEditor();
				}
			}
		}

		private async void OnDeleteNoteClicked(object sender, EventArgs e)
		{
			if (sender is Button button && button.CommandParameter is int noteId)
			{
				bool confirm = await DisplayAlert("Delete Note", "Are you sure you want to delete this note?", "Yes", "No");
				if (confirm)
				{
					var noteToDelete = notes.FirstOrDefault(n => n.Id == noteId);
					if (noteToDelete != null)
					{
						await database.DeleteNoteAsync(noteToDelete);
						notes.Remove(noteToDelete);

						// If we were editing this note, clear the editor
						if (currentEditingNote?.Id == noteId)
						{
							NoteEditor.Text = string.Empty;
							currentEditingNote = null;
						}
					}
				}
			}
		}

		private async Task ScrollToNoteEditor()
		{
			// This method ensures the editor is visible when editing a note
			await Task.Delay(100); // Small delay to allow UI to update
		}
	}
}
