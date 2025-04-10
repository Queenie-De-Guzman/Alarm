using SQLite;
using System;

namespace Alarm.Models
{
	public class AlarmModel
	{
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }

		public string Title { get; set; }

		// Store the time as a TimeSpan
		public TimeSpan Time { get; set; }
	}
}
