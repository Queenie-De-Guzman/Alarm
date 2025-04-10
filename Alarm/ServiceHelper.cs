using Microsoft.Extensions.DependencyInjection;

namespace Alarm;

public static class ServiceHelper
{
	public static IServiceProvider Services { get; private set; } = default!;

	public static void Init(IServiceProvider services)
	{
		Services = services;
	}

	public static T GetService<T>() where T : class
	{
		return Services.GetRequiredService<T>();
	}
}
