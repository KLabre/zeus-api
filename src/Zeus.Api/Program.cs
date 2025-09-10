using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Identity.Web;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System.Diagnostics;
using System.Net.Mime;
using System.Text.Json;
using Zeus.Api.Models;
using Zeus.Api.Mappers;
using Zeus.Api.Filters;
using System.Reflection;
using Newtonsoft.Json;
using Zeus.Api.Services;
using Zeus.Api.Infrastructure;

namespace Zeus.Api;

public class Program
{
    public static void Main(string[] args)
    {
        // The first thing we should do is configure our Logging
        // Per default, let's log to console so we can debug on primordial errors
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        // We shall wrap it all in a try and catch to log startup issues like missing secrets
        try
        {
            var builder = WebApplication.CreateBuilder(args);

            AddLogging(builder);
            ConfigureAutoMapper(builder);
            ConfigureAppSettings(builder);
            AddServices(builder);
            // TODO: AddCaching(builder);
            ConfigureControllers(builder);

            // Add Authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

            var app = builder.Build();

            AddSwagger(app);
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseSerilogRequestLogging(); // This will log every https request
            AssertSecretsAreOverridenInConfiguration(app);
            app.UseCors(AppConstants.ZeusSpecificOrigins);

            // TODO: Register the rate limit middleware
            // app.UseMiddleware<RateLimitMiddleware>();

            app.MapControllers();
            AddHealthChecks(app);

            WriteErrorCodesAsJson(); // So we can easily copy into our client application

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "The application could not start due to an unhandle Program.cs exception");
        }
        finally
        {
            Log.Information("Init complete");
            Log.CloseAndFlush();
        }
    }

    #region private methods
    private static void AddLogging(WebApplicationBuilder builder)
    {
        Log.Information("Initializing Serilog...");
        var outputTemplate = "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message}{NewLine}in method {MethodName} at {FilePath}:{LineNumber}{NewLine}{Exception}{NewLine}";

        builder.Host.UseSerilog((context, configuration) =>
        {
            configuration
                .ReadFrom
                .Configuration(builder.Configuration)
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                .Enrich.FromLogContext()
                .Enrich.With<UserEnricher>()
                .WriteTo.Console(LogEventLevel.Verbose, outputTemplate, theme: AnsiConsoleTheme.Literate);

            if (Debugger.IsAttached) configuration.WriteTo.Trace();
        });

        Log.Information("Serilog has been initialized.");

        // Configure Serilog for DI
        builder.Services.AddSingleton(Log.Logger);
    }

    private static Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.ToString(),
            entries = report.Entries.Select(e => new
            {
                key = e.Key,
                status = Enum.GetName(typeof(HealthStatus), e.Value.Status),
                duration = e.Value.Duration.ToString(),
            })
        });

        context.Response.ContentType = MediaTypeNames.Application.Json;
        return context.Response.WriteAsync(result);
    }

    private static void ConfigureAutoMapper(WebApplicationBuilder builder)
    {
        var mapperConfig = new MapperConfiguration(config =>
        {
            config.AddProfile(new MappingProfile());
        });

        var mapper = mapperConfig.CreateMapper();
        builder.Services.AddSingleton(mapper);
    }

    private static void ConfigureControllers(WebApplicationBuilder builder)
    {
        builder.Services.AddRouting(options =>
        {
            options.LowercaseUrls = true;
        });

        builder.Services
            .AddControllers(options =>
            {
                //if (!builder.Environment.IsDevelopment())
                //options.Filters.Add(new TokenCacheAuthorizationFilter());

                options.Filters.Add(new ModelValidationFilter());
                options.Filters.Add<ServiceResultResponseFilter>();
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter()); // Make sure Dates are treated in ISO
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; // allMyProperties must be converted asSuch.
            });
    }

    private static void AddCaching(WebApplicationBuilder builder)
    {
        // TODO:
        //builder.Servicer.AddSingleton<ICacheService, CacheService>();
        //CacheSettings.ConnectionString = builder.Configuration["CacheSettings:ConnectionString"]
    }

    private static void ConfigureAppSettings(WebApplicationBuilder builder)
    {
        // We can use this to access configuration via IOptions in the DI
        // Where AppSettings is a SomethingAppSettings in the Models.AppSettings folder
        // builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("ParentObject:ChildObject"));
    }

    private static void AddServices(WebApplicationBuilder builder)
    {
        builder.Services.AddExceptionHandler<CustomExceptionHandler>();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddHealthChecks();
        builder.Services.AddProblemDetails();

        builder.Services.AddHttpClient();
        builder.Services.AddSingleton<ErrorCodeMessages>();
        
        // Scopped services
        builder.Services.AddScoped<IRootService, RootService>();
        builder.Services.AddScoped<ISubjectsService, SubjectsService>();

        builder.Services.AddSwaggerGen();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(AppConstants.ZeusSpecificOrigins, policy =>
            {
                var allowedOrigins = builder.Configuration.GetValue<string>("CorsOrigins")?.Split(',');
                if (allowedOrigins is { Length: > 0 })
                {
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }
            });
        });

        // This will be great at somepoint but I will avoid overhead for now
        //builder.Services.AddMvc(options =>
        //{
        //    options.Filters.Add<LinkRewritingFilter>();
        //});
    }

    private static void AddHealthChecks(WebApplication? app)
    {
        if(app != null)
        {
            app.MapHealthChecks("healthz", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions()
            {
                Predicate = (check) => check.Tags.Contains("healthy"),
                ResponseWriter = WriteHealthCheckResponse
            });

            app.MapHealthChecks("readyz", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions()
            {
                Predicate = (check) => check.Tags.Contains("ready"),
                ResponseWriter = WriteHealthCheckResponse
            });
        }
    }

    private static void AddSwagger(WebApplication? app)
    {
        if(app != null && app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.DefaultModelsExpandDepth(-1); // hide internal schema classes/models from swagger UI HTML files
            });
        }
    }

    private static void AssertSecretsAreOverridenInConfiguration(WebApplication host)
    {
        Log.Information("Checking if all secrets are overriden in AppSettings");

        var configuration = host.Services.GetService(typeof(IConfiguration)) as IConfiguration;

        var nonOverridenSecrets = configuration?.AsEnumerable().
            Where(pair => pair.Value != null && pair.Value.Contains("IAMASECRET", StringComparison.CurrentCultureIgnoreCase));

        (nonOverridenSecrets ?? throw new InvalidOperationException("There are one or more AppSettings that have not been configured / overriden. Tip: for local development, use usersecrets.jsonl for Azure, use environment variables."))
                .ToList()
                .ForEach(pair => Log.Error("The AppSetting {appSettingsKey} has not been configured / overriden.", pair.Key));

        if (nonOverridenSecrets.Any()) {
            var e = new InvalidOperationException("There are one or more AppSettings that have not been configured / overriden. Tip: for local development, use usersecrets.jsonl for Azure, use environment variables.");
            Log.Error(e, "Not all AppSettings have been configured: unable to start application.");
            throw e;
        }

        Log.Information("All secrets have been configured.");
    }

    private static void WriteErrorCodesAsJson()
    {
        // We need to use reflection to get the public static filed of the ErrorCodes class
        var errorCodes = new Dictionary<string, string>();

        // Get all public static fields (constatns)
        var fields = typeof(ErrorCodes).GetFields(BindingFlags.Public | BindingFlags.Static);

        // Loop through the fields and dynamically add them to the dictionary
        foreach (var field in fields)
        { 
            // Addd field name and value to the dictionay
            errorCodes.Add(field.Name, field.GetValue(null)?.ToString() ?? string.Empty);
        }

        // Get the current workiung directory
        var currentDirectory = Directory.GetCurrentDirectory();

        // Save the JSON file in the same directory
        var filePath = Path.Combine(currentDirectory, "errorCodes.json");
        var json = JsonConvert.SerializeObject(errorCodes, Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText(filePath, json);
        Debug.WriteLine($"JSON file save to: {filePath}");
    }
    #endregion
}
