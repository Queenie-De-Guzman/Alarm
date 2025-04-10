using SQLite;
using Alarm.Models;

namespace Alarm.Data
{
	public class AlarmDatabase
	{
		private readonly SQLiteAsyncConnection _database;

		public AlarmDatabase(string dbPath)
		{
			_database = new SQLiteAsyncConnection(dbPath);
			_database.CreateTableAsync<AlarmModel>().Wait();
		}

		public Task<int> SaveAlarmAsync(AlarmModel alarm)
		{
			return _database.InsertAsync(alarm);
		}

		public Task<List<AlarmModel>> GetAlarmsAsync()
		{
			return _database.Table<AlarmModel>().ToListAsync();
		}
	}
}
