using Serilog;
using Swashbuckle.AspNetCore;
using tracking_management_api.Extensions;
using tracking_management_api.Infrastructure.Validation;

Log.Logger = new LoggerConfiguration()
    .WriteTo.File(
        path: "logs/tracking-management-api-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        shared: true)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<FluentValidationFilter>();
    });
    builder.Services.AddOpenApi();
    builder.Services.AddLoadsModule(builder.Configuration);
    builder.Services.AddSafetyIncidentsModule(builder.Configuration);
    builder.Services.AddRabbitMqMessaging(builder.Configuration);
    builder.Services.AddEndpointMetrics(builder.Configuration);

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "tracking-management-api v1");
        });
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "The application failed to start.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
