using Alarm.Models;
using SQLite;
using System;
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
			_database.CreateTableAsync<Note>().Wait();
			_database.CreateTableAsync<CalendarEvent>().Wait(); // Add Calendar Events table
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

		public Task<int> UpdateNoteAsync(Note note) =>
			_database.UpdateAsync(note);

		// CALENDAR EVENT METHODS
		public Task<int> SaveEventAsync(CalendarEvent calEvent)
		{
			if (calEvent.Id != 0)
				return _database.UpdateAsync(calEvent);
			else
			{
				calEvent.Created = DateTime.Now;
				return _database.InsertAsync(calEvent);
			}
		}

		public Task<List<CalendarEvent>> GetEventsAsync() =>
			_database.Table<CalendarEvent>().ToListAsync();

		public Task<List<CalendarEvent>> GetEventsForDateAsync(DateTime date) =>
			_database.Table<CalendarEvent>().Where(e => e.Date.Date == date.Date).ToListAsync();

		public Task<List<CalendarEvent>> GetEventsForUserAsync(string userId) =>
			_database.Table<CalendarEvent>().Where(e => e.UserId == userId).ToListAsync();

		public Task<List<CalendarEvent>> GetEventsForMonthAsync(DateTime monthStart, DateTime monthEnd) =>
			_database.Table<CalendarEvent>()
					.Where(e => e.Date >= monthStart && e.Date <= monthEnd)
					.ToListAsync();

		public Task<int> DeleteEventAsync(CalendarEvent calEvent) =>
			_database.DeleteAsync(calEvent);

		public Task<int> DeleteEventsForUserAsync(string userId) =>
			_database.ExecuteAsync("DELETE FROM CalendarEvent WHERE UserId = ?", userId);
	}
}