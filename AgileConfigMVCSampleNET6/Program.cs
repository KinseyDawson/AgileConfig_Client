using AgileConfigMVCSampleNET6;
using NLog.Extensions.Logging;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);

// use serilog
//builder.Host.UseSerilog((ctx, lc) => lc
//    .WriteTo.Console()
//    );
//use agileconfig client
builder.Host.UseAgileConfig();

//use nlog
builder.Services.AddLogging(b => {
    var config = builder.Configuration;
    NLog.LogManager.Configuration = new NLogLoggingConfiguration(config.GetSection("NLog"));
    b.AddNLogWeb();
});

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.Configure<GlobalOptions>(builder.Configuration.GetSection("Global"));

var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
