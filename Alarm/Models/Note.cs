using SQLite;

namespace Alarm.Models
{
	public class Note
	{
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }

		public string Content { get; set; }

		public string UserId { get; set; }  // <-- Associate note with user
	}
}
