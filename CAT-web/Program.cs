using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CATWeb.Data;
using CATWeb.Models;
using CATWeb.Services.CAT;
using CATWeb.Services.MT;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<CATWebContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CATWebContext") ?? throw new InvalidOperationException("Connection string 'CATWebContext' not found.")));

// Add services to the container.
builder.Services.AddControllersWithViews();
//builder.Services.AddControllers();

builder.Services.AddScoped<CATClientService>();
builder.Services.TryAddEnumerable(new[]
    {
        // Type-based services
        ServiceDescriptor.Singleton<IMachineTranslator, MMT>(),
        //ServiceDescriptor.Singleton<IMachineTranslator, MachineTranslator2>(),
    });

//builder.Services.AddDbContext<CATWebContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("CATWebContext"))
//    .LogTo(Console.WriteLine, LogLevel.Information) // <-- Add this line
//);

//builder.Services.AddDbContext<CATWebContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("CATWebContext"))
//        .AddInterceptors(new CATDbCommandInterceptor()) // Add your interceptor here
//);


builder.Services.AddSession();
builder.Services.AddMemoryCache();

var app = builder.Build();

app.UseSession();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    SeedData.Initialize(services);

    //var memoryCache = services.GetRequiredService<IMemoryCache>();
    //var someObject = new Object(); // your object
    //memoryCache.Set("Sessions", new Dictionary<String, Object>());
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseStaticFiles();
app.UseDefaultFiles();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
