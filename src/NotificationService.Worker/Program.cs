using NotificationService.Worker.Common;

var builder = Host.CreateApplicationBuilder(args);

builder.AddWorkerHost();

var host = builder.Build();
host.Run();
