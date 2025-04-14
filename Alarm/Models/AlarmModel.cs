using SQLite;
using System;

namespace Alarm.Models
{
	public class AlarmModel
	{
		[PrimaryKey, AutoIncrement]  // SQLite attribute for primary key and auto-increment
		public int Id { get; set; }  // Use integer ID for the database
		public string Title { get; set; } = string.Empty;
		public DateTime AlarmDateTime { get; set; }
		public bool Triggered { get; set; }
	}
}
