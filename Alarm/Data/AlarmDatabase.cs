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
			_database.CreateTableAsync<TodoItem>().Wait(); // 👈 Add this line
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
	}
}
