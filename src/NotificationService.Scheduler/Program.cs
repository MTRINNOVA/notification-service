using NotificationService.Application;
using NotificationService.Infrastructure;
using NotificationService.Scheduler;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<SchedulerService>();

var host = builder.Build();
host.Run();
