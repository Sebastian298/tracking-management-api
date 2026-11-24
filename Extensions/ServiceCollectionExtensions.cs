using FluentValidation;
using tracking_management_api.Infrastructure.Messaging;
using tracking_management_api.Infrastructure.Metrics;
using tracking_management_api.Mappers;
using tracking_management_api.Options.Loads;
using tracking_management_api.Options.SafetyIncidents;
using tracking_management_api.Repositories.Loads;
using tracking_management_api.Repositories.SafetyIncidents;
using tracking_management_api.Services.Loads;
using tracking_management_api.Services.SafetyIncidents;
using tracking_management_api.Validators;

namespace tracking_management_api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLoadsModule(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<LoadsDataOptions>()
            .Bind(configuration.GetSection(LoadsDataOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.FilePath), "LoadsData:FilePath is required.")
            .Validate(options => options.MaxItems > 0, "LoadsData:MaxItems must be greater than zero.")
            .ValidateOnStart();

        services.AddOptions<LoadsCacheOptions>()
            .Bind(configuration.GetSection(LoadsCacheOptions.SectionName))
            .Validate(
                options => options.AbsoluteExpirationSeconds > 0,
                "LoadsCache:AbsoluteExpirationSeconds must be greater than zero.")
            .ValidateOnStart();

        services.AddMemoryCache();
        services.AddSingleton<ILoadMapper, LoadMapper>();
        services.AddSingleton<ILoadRepository, CsvLoadRepository>();
        services.AddSingleton<ILoadService, LoadService>();
        services.AddSingleton<MemoryCachedLoadService>();
        services.AddSingleton<ICachedLoadService>(static provider =>
            provider.GetRequiredService<MemoryCachedLoadService>());
        services.AddSingleton<ILoadCacheStats>(static provider =>
            provider.GetRequiredService<MemoryCachedLoadService>());

        return services;
    }

    public static IServiceCollection AddSafetyIncidentsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<SafetyIncidentsOptions>()
            .Bind(configuration.GetSection(SafetyIncidentsOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.FilePath),
                "SafetyIncidents:FilePath is required.")
            .Validate(
                options => options.ProcessingDelayMs >= 0,
                "SafetyIncidents:ProcessingDelayMs must be zero or greater.")
            .ValidateOnStart();

        services.AddSingleton<ISafetyIncidentMapper, SafetyIncidentMapper>();
        services.AddSingleton<ISafetyIncidentRepository, CsvSafetyIncidentRepository>();
        services.AddSingleton<ISafetyIncidentService, SafetyIncidentService>();
        services.AddKeyedSingleton<ISafetyIncidentNotifier, InlineSafetyIncidentNotifier>(
            SafetyIncidentNotifierKeys.Inline);
        services.AddValidatorsFromAssemblyContaining<CreateSafetyIncidentRequestValidator>();

        return services;
    }

    public static IServiceCollection AddRabbitMqMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.HostName),
                "RabbitMq:HostName is required.")
            .Validate(
                options => options.Port > 0,
                "RabbitMq:Port must be greater than zero.")
            .Validate(
                options => RabbitMqOptions.IsSecretConfigured(options.UserName),
                "RabbitMq:UserName must be provided with user secrets or environment variables, not <Set_In_Secret>.")
            .Validate(
                options => RabbitMqOptions.IsSecretConfigured(options.Password),
                "RabbitMq:Password must be provided with user secrets or environment variables, not <Set_In_Secret>.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.QueueName),
                "RabbitMq:QueueName is required.")
            .ValidateOnStart();

        services.AddSingleton<RabbitMqConnectionProvider>();
        services.AddKeyedSingleton<ISafetyIncidentNotifier, RabbitMqSafetyIncidentPublisher>(
            SafetyIncidentNotifierKeys.RabbitMq);

        return services;
    }

    public static IServiceCollection AddEndpointMetrics(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<EndpointMetricsOptions>()
            .Bind(configuration.GetSection(EndpointMetricsOptions.SectionName))
            .Validate(
                options => options.ReportIntervalSeconds > 0,
                "EndpointMetrics:ReportIntervalSeconds must be greater than zero.")
            .ValidateOnStart();

        services.AddSingleton<IEndpointMetricsRegistry, EndpointMetricsRegistry>();
        services.AddHostedService<MetricsReporterService>();

        return services;
    }
}
