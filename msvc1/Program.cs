using Serilog;
using Serilog.Context;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddDebug();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Host.UseSerilog((context, services, configuration) =>
{
    var semVer = GitVersionInformation.SemVer; // Get the semantic version from GitVersion

    configuration
        .ReadFrom.Configuration(context.Configuration) // Existing configuration
        .Enrich.WithProperty("Version", semVer); // Enrich logs with the version information
});

builder.WebHost.ConfigureKestrel(opt =>
{
    opt.ListenAnyIP(5001);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<IpLoggingMiddleware>();

app.UseAuthorization();

app.MapControllers();


app.MapGet("/version", () =>
{
    var info = GitVersionInformation.FullBuildMetaData;
    var ginfo = GitVersionInformation.InformationalVersion;

    //Log.Information("SemVer: {version}", info);

    var mucho = new
    {
        GitVersionInformation.BranchName,
        GitVersionInformation.FullSemVer,
        GitVersionInformation.FullBuildMetaData,
        GitVersionInformation.SemVer,
        GitVersionInformation.InformationalVersion,
        GitVersionInformation.Patch,
        GitVersionInformation.BuildMetaData,
        GitVersionInformation.CommitsSinceVersionSource,
        GitVersionInformation.CommitsSinceVersionSourcePadded,
        GitVersionInformation.CommitDate,
        GitVersionInformation.NuGetVersionV2,
        GitVersionInformation.NuGetVersion,
        GitVersionInformation.NuGetPreReleaseTagV2,
        GitVersionInformation.NuGetPreReleaseTag,
        GitVersionInformation.VersionSourceSha,
    };


    var version = $"{GitVersionInformation.SemVer}";
    return version;
});

app.Run();

public class IpLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public IpLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        using (LogContext.PushProperty("IPAddress", ipAddress))
        {
            await _next(context);
        }
    }
}
