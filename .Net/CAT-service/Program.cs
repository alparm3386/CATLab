using AutoMapper;

using CAT.BusinessServices;
using CAT.BusinessServices.Okapi;
using CAT.Configuration;
using CAT.GRPCServices;
using CAT.Infrastructure.Logging;
using CAT.Services;
using CAT.TB;
using CAT.TM;
using CAT.Utils;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 5001, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

builder.Services.AddSingleton<IDataStorage, SQLiteStorage>();
builder.Services.AddSingleton<IOkapiService, OkapiService>();
builder.Services.AddSingleton<ITMService, TMService>();
builder.Services.AddSingleton<ITBService, TBService>();
builder.Services.AddSingleton<IFileSystem, FileSystem>();
builder.Services.AddTransient<IEmailService, SmtpEmailService>();

//the logger
builder.Logging.AddProvider(new Log4NetLoggerProvider("log4net.config"));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<CatService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();

// Additional part for testing
[SuppressMessage("CodeQuality", "S1118:Utility classes should not have public constructors", Justification = "Required by framework")]
public partial class Program
{
}