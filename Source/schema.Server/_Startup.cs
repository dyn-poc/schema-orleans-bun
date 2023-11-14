namespace schema.Server;

using Abstractions.Grains;
using Boxed.AspNetCore;
using Grains;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpLogging;
using HealthChecks;
using Api;

#pragma warning disable CA1724 // The type name conflicts with the namespace name 'Orleans.Runtime.Startup'
public class _Startup
#pragma warning restore CA1724 // The type name conflicts with the namespace name 'Orleans.Runtime.Startup'
{
    private readonly IConfiguration configuration;
    private readonly IWebHostEnvironment webHostEnvironment;

    /// <summary>
    /// Initializes a new instance of the <see cref="_Startup"/> class.
    /// </summary>
    /// <param name="configuration">The application configuration, where key value pair settings are stored (See
    /// http://docs.asp.net/en/latest/fundamentals/configuration.html).</param>
    /// <param name="webHostEnvironment">The environment the application is running under. This can be Development,
    /// Staging or Production by default (See http://docs.asp.net/en/latest/fundamentals/environments.html).</param>
    public _Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
    {
        this.configuration = configuration;
        this.webHostEnvironment = webHostEnvironment;
    }

    public virtual void ConfigureServices(IServiceCollection services)
    {
        services
            .AddRouting(options => options.LowercaseUrls = true);
        // services.AddSwaggerGen();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
            c.SwaggerDoc("v1", new()
            {
                Title = this.webHostEnvironment.ApplicationName,
                Version = "v1"
            }));

        services
            .AddHttpClient()
            .AddHttpClient<MockSchemaGrain>();
        services.AddHttpClient()
            .AddHttpClient<ISchemaRegistryGrain>();
        services.AddHttpClient();
        services.AddHealthChecks()
            .AddCheck<ClusterHealthCheck>(nameof(ClusterHealthCheck))
            .AddCheck<GrainHealthCheck>(nameof(GrainHealthCheck))
            .AddCheck<SiloHealthCheck>(nameof(SiloHealthCheck))
            .AddCheck<StorageHealthCheck>(nameof(StorageHealthCheck));

        services.AddLogging(logger=>
        {
            logger.AddConsole();
            logger.AddFilter((string catagory, LogLevel level) =>
                {
                    if (
                        catagory.Contains("Orleans")
                        || catagory.Contains("GrainDirectory")
                        || catagory.Contains("Messaging.")
                // || catagory.Contains("Microsoft.Hosting")
                        )
                {
                    return level >= LogLevel.Warning;
                }
                else
                {
                    return level >= LogLevel.Information;
                }
            });
            // TypeNameHelper.GetTypeDisplayName(typeof (T), includeGenericParameters: false, nestedTypeDelimiter: '.')
        });

        services.AddLogging().AddHttpLogging(http => http.LoggingFields = HttpLoggingFields.All);






    }


    public virtual void Configure(IApplicationBuilder application) =>
        application
            .UseSchemaMiddleware()
            .UseRouting()
            .UseEndpoints(
                builder =>
                {
                    builder.MapSchemaApi();
                    builder.MapHealthChecks("/status");
                    builder.MapHealthChecks("/status/self", new HealthCheckOptions() { Predicate = _ => false });
                    builder.MapSwagger();

                })

            .UseIf(
                this.webHostEnvironment.IsDevelopment(),
                x => x.UseSwagger().UseSwaggerUI())
    .UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("v1/swagger.json", "My API V1");
    });

}
