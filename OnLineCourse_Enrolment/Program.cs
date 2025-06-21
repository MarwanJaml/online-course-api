using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OnLineCourse.Core.Entities;
using OnLineCourse.Data;
using OnLineCourse.Service;
using Serilog;
using Serilog.Templates;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using OnLineCourse_Enrolment.Middlewares;
using System.Diagnostics;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Logging;
using Microsoft.Identity.Web;
using System.Text.Json;
using OnLineCourse_Enrolment.Common;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog early for bootstrap logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Debug()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting application configuration...");

    // Verify configuration is loading properly
    Log.Information("Configuration sources: {@Sources}",
        builder.Configuration.Sources.Select(s => s.ToString()).ToList());

    // Explicitly check connection string
    var connectionString = builder.Configuration.GetConnectionString("DbContext");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Connection string 'DbContext' not found in configuration");
    }
    Log.Information("Using connection string: {ConnectionString}", connectionString);

    #region Service Configuration

    // Health Checks - FIXED: Use builder.Configuration instead of 'configuration'
    builder.Services.AddHealthChecks()
        .AddSqlServer(
            connectionString: connectionString, // Use the verified connection string
            healthQuery: "SELECT 1;",
            name: "sqlserver",
            failureStatus: HealthStatus.Degraded,
            tags: new[] { "db", "sql" })
        .AddCheck("Memory", new PrivateMemoryHealthCheck(1024 * 1024 * 1024));

    // Configure Application Insights
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.EnableAdaptiveSampling = false;
        options.EnableDependencyTrackingTelemetryModule = true;
        options.EnablePerformanceCounterCollectionModule = true;
        options.EnableQuickPulseMetricStream = true;
        options.EnableRequestTrackingTelemetryModule = true;
    });

    // Final Serilog configuration
    builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .WriteTo.Console(new ExpressionTemplate(
            "[{@t:HH:mm:ss} {@l:u3}{#if @tr is not null} ({substring(@tr,0,4)}:{substring(@sp,0,4)}){#end}] {@m}\n{@x}"))
        .WriteTo.ApplicationInsights(
            context.Configuration["ApplicationInsights:ConnectionString"],
            TelemetryConverter.Traces));

    // Authentication
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(
            options => builder.Configuration.Bind("AzureAdB2C", options),
            options => builder.Configuration.Bind("AzureAdB2C", options));

    // Database Configuration
    builder.Services.AddDbContextPool<OnlineCourseDbContext>(options =>
    {
        options.UseSqlServer(
            connectionString,
            sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                sqlOptions.CommandTimeout(60);
                sqlOptions.MigrationsAssembly(typeof(OnlineCourseDbContext).Assembly.FullName);
            });

        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());

        if (builder.Environment.IsDevelopment())
        {
            options.LogTo(message => Log.Information("EF Core: {Message}", message),
                LogLevel.Information);
        }
    });

    // CORS Configuration
    builder.Services.AddCors(o => o.AddPolicy("default", policy =>
    {
        policy.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    }));

    // API Services
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddAutoMapper(typeof(MappingProfile));

    // Application Services
    builder.Services.AddScoped<ICourseCategoryRepository, CourseCategoryRepository>();
    builder.Services.AddScoped<ICourseCategoryService, CourseCategoryService>();
    builder.Services.AddScoped<ICourseRepository, CourseRepository>();
    builder.Services.AddScoped<ICourseService, CourseService>();
    builder.Services.AddScoped<IVideoRequestRepository, VideoRequestRepository>();
    builder.Services.AddScoped<IVideoRequestService, VideoRequestService>();
    builder.Services.AddScoped<IUserClaims, UserClaims>();

    #endregion

    #region Middleware Pipeline
    var app = builder.Build();

    // Database connection test
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            Log.Information("Testing database connection...");
            var db = services.GetRequiredService<OnlineCourseDbContext>();
            db.Database.OpenConnection();
            db.Database.CloseConnection();
            Log.Information("Database connection test succeeded");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Database connection test failed");
            throw;
        }
    }

    // Exception Handling Middleware
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            var exception = exceptionHandlerPathFeature?.Error;

            Log.Error(exception, "Unhandled exception in {Path}", exceptionHandlerPathFeature?.Path);

            if (app.Environment.IsDevelopment())
            {
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    error = exception?.Message,
                    stackTrace = exception?.StackTrace,
                    innerException = exception?.InnerException?.Message
                }));
            }
            else
            {
                await context.Response.WriteAsync("An unexpected error occurred");
            }
        });
    });

    // Custom Middlewares
    app.UseMiddleware<RequestLoggingMiddleware>();
    app.UseMiddleware<ResponseLoggingMiddleware>();

    // API Middlewares
    app.UseCors("default");
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();

    // Authentication
    app.UseAuthentication();
    app.UseAuthorization();

    // Health Checks
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = HealthCheckResponseWriter.WriteJsonResponse
    });

    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = _ => false,
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                status = report.Status.ToString(),
                description = "Liveness check - the app is up"
            });
        }
    });

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = HealthCheckResponseWriter.WriteJsonResponse
    });

    app.MapControllers();

    // Auto-open browser in development
    if (app.Environment.IsDevelopment())
    {
        try
        {
            var applicationUrls = app.Configuration["ASPNETCORE_URLS"]?.Split(';') ??
                new[] { "https://localhost:7045", "http://localhost:5088" };

            var url = applicationUrls.FirstOrDefault(u => u.StartsWith("https")) ??
                    applicationUrls.FirstOrDefault();

            if (url != null)
            {
                var browserUrl = url.Replace("0.0.0.0", "localhost") + "/swagger";
                Log.Information("Launching browser to {Url}", browserUrl);
                Process.Start(new ProcessStartInfo
                {
                    FileName = browserUrl,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to launch browser");
        }
    }

    Log.Information("Application startup complete. Running...");
    app.Run();
    #endregion
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}