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

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Debug()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .CreateBootstrapLogger();

try
{
    // 1. First verify configuration is loading properly
    Log.Information("Configuration sources: {@Sources}",
        builder.Configuration.Sources.Select(s => s.ToString()).ToList());

    // 2. Explicitly check connection string
    var connectionString = builder.Configuration.GetConnectionString("DbContext");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Connection string 'DbContext' not found in configuration");
    }
    Log.Information("Using connection string: {ConnectionString}", connectionString);

    #region Service Configuration

    // Configure Application Insights
    builder.Services.AddApplicationInsightsTelemetry(options => {
        options.EnableAdaptiveSampling = false;
        options.EnableDependencyTrackingTelemetryModule = true;
        options.EnablePerformanceCounterCollectionModule = true;
        options.EnableQuickPulseMetricStream = true;
        options.EnableRequestTrackingTelemetryModule = true;
    });

    builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .WriteTo.Console(new ExpressionTemplate(
            "[{@t:HH:mm:ss} {@l:u3}{#if @tr is not null} ({substring(@tr,0,4)}:{substring(@sp,0,4)}){#end}] {@m}\n{@x}"))
        .WriteTo.ApplicationInsights(
            context.Configuration["ApplicationInsights:ConnectionString"],
            TelemetryConverter.Traces));

    Log.Information("Starting the SmartLearnByKarthik API...");
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
         .AddMicrosoftIdentityWebApi(options =>
         {
             builder.Configuration.Bind("AzureAdB2C", options); // ? Use builder.Configuration
             options.Events = new JwtBearerEvents();
             // ...
         },
         options => { builder.Configuration.Bind("AzureAdB2C", options); }); // ? Use builder.Configuration


    // Database Configuration with enhanced options
    builder.Services.AddDbContextPool<OnlineCourseDbContext>(options =>
    {
        options.UseSqlServer(
            connectionString,  // Using the verified connection string
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
        options.LogTo(Console.WriteLine, LogLevel.Information);
    });

    // CORS Configuration
    builder.Services.AddCors(o => o.AddPolicy("default", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    }));

    // API Services
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Application Services
    builder.Services.AddScoped<ICourseCategoryRepository, CourseCategoryRepository>();
    builder.Services.AddScoped<ICourseCategoryService, CourseCategoryService>();
    builder.Services.AddScoped<ICourseRepository, CourseRepository>();
    builder.Services.AddScoped<ICourseService, CourseService>();
    #endregion

    #region Middleware Pipeline
    var app = builder.Build();

    // Database connection test
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
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

            // Log the full error
            Log.Error(exception, "Unhandled exception occurred in {Path}",
                exceptionHandlerPathFeature?.Path);

            // Return detailed error in development
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

    // Custom Logging Middlewares
    app.UseMiddleware<RequestLoggingMiddleware>();
    app.UseMiddleware<ResponseLoggingMiddleware>();



    // API Middlewares
    app.UseCors("default");
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();


    #region AD B2C
    app.UseAuthentication();
    app.UseAuthorization();
    #endregion  AD B2C

    app.MapControllers();

    // Auto-open browser
    try
    {
        var applicationUrls = app.Urls.Any() ? app.Urls.ToArray()
                         : new[] { "https://localhost:7045", "http://localhost:5088" };

        var url = applicationUrls.FirstOrDefault(u => u.StartsWith("https")) ??
                  applicationUrls.FirstOrDefault();

        if (url != null)
        {
            var browserUrl = url.Replace("0.0.0.0", "localhost") + "/swagger";
            Console.WriteLine($"Launching browser at: {browserUrl}");
            Process.Start(new ProcessStartInfo
            {
                FileName = browserUrl,
                UseShellExecute = true
            });
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to launch browser: {ex.Message}");
    }

    Log.Information("Application startup complete");
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