using AutoMapper;

using CAT.BusinessServices;
using CAT.BusinessServices.Okapi;
using CAT.Configuration;
using CAT.GRPCServices;
using CAT.Infrastructure.Logging;
using CAT.Services;
using CAT.TM;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();
//builder.Services.AddAutoMapper(cfg =>
//    {
//        cfg.ShouldMapProperty = pi => pi is PropertyInfo && pi.GetMethod != null && !pi.GetMethod.IsVirtual;
//    }, typeof(AutoMapperProfile));
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

builder.Services.AddSingleton<IDataStorage, SQLiteStorage>();
builder.Services.AddSingleton<IOkapiConnector, OkapiConnector>();
builder.Services.AddSingleton<IOkapiService, OkapiService>();
builder.Services.AddSingleton<ITMService, TMService>();
builder.Services.AddTransient<IEmailService, SmtpEmailService>();

//the logger
builder.Logging.AddProvider(new Log4NetLoggerProvider("log4net.config"));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<CATService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
