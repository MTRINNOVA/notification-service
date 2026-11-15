using NotificationService.Scheduler.Common;

var builder = Host.CreateApplicationBuilder(args);

builder.AddSchedulerHost();

var host = builder.Build();
host.Run();
