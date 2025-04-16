using SQLite;
using System;

namespace Alarm.Models
{
	public class CalendarEvent
	{
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }
		public DateTime Date { get; set; }
		public string Title { get; set; }
		public string TimeDisplay { get; set; }
		public string Color { get; set; }
		public string UserId { get; set; } // For user-specific events
		public string Notes { get; set; } // Additional details for the event
		public bool IsAllDay { get; set; }
		public DateTime Created { get; set; }
	}
}