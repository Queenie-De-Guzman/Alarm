namespace Alarm.Services
{
	public interface IAlarmScheduler
	{
		void ScheduleAlarm(DateTime alarmTime, string title);
	}
}
