using Alarm.Models;
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Alarm.Data
{
	public class AlarmDatabase
	{
		private readonly SQLiteAsyncConnection _database;

		public AlarmDatabase(string dbPath)
		{
			_database = new SQLiteAsyncConnection(dbPath);
			_database.CreateTableAsync<AlarmModel>().Wait();
			_database.CreateTableAsync<TodoItem>().Wait();
			_database.CreateTableAsync<Note>().Wait();// 👈 Add this line
		}

		// ALARM METHODS
		public Task<int> SaveAlarmAsync(AlarmModel alarm) =>
			_database.InsertOrReplaceAsync(alarm);

		public Task<List<AlarmModel>> GetAlarmsAsync() =>
			_database.Table<AlarmModel>().ToListAsync();

		public Task<int> DeleteAlarmAsync(AlarmModel alarm) =>
			_database.DeleteAsync(alarm);

		// TODO METHODS
		public Task<int> SaveTodoAsync(TodoItem todo) =>
			_database.InsertOrReplaceAsync(todo);

		public Task<List<TodoItem>> GetTodosAsync() =>
			_database.Table<TodoItem>().Where(t => !t.IsDone).ToListAsync();

		public Task<List<TodoItem>> GetDoneTodosAsync() =>
			_database.Table<TodoItem>().Where(t => t.IsDone).ToListAsync();

		public Task<int> DeleteTodoAsync(TodoItem todo) =>
			_database.DeleteAsync(todo);

		// NOTE METHODS
		public Task<int> SaveNoteAsync(Note note) =>
			_database.InsertAsync(note);

		public Task<List<Note>> GetNotesForUserAsync(string userId) =>
			_database.Table<Note>().Where(n => n.UserId == userId).ToListAsync();

		public Task<int> DeleteNoteAsync(Note note) =>
			_database.DeleteAsync(note);

		public Task<int> DeleteAllNotesForUserAsync(string userId) =>
			_database.ExecuteAsync("DELETE FROM Note WHERE UserId = ?", userId);

		// Add this new method for updating notes
		public Task<int> UpdateNoteAsync(Note note) =>
			_database.UpdateAsync(note);
	}
}