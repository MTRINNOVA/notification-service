using NotificationService.Application;
using NotificationService.Infrastructure;
using NotificationService.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<WorkerService>();

var host = builder.Build();
host.Run();
