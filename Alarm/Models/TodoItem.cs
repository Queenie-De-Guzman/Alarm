using SQLite;
using System;

namespace Alarm.Models
{
	public class TodoItem
	{
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }

		public string Text { get; set; }
		public DateTime Date { get; set; }
		public bool IsDone { get; set; }
	}
}
