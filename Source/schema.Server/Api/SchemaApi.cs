namespace schema.Server.Api;

using Json.Schema;
using Microsoft.AspNetCore.Http.Extensions;
using Orleans;
using schema.Abstractions.Grains;
using CacheControlHeaderValue = Microsoft.Net.Http.Headers.CacheControlHeaderValue;

public static class SchemaApi
{

    public static void AddSchemaApi(this WebApplication app)
    {

        UseSchemaMiddleware(app);

        MapSchemaApi(app);
    }

    public static IEndpointRouteBuilder MapSchemaApi(this IEndpointRouteBuilder app)
    {
        app.MapGet("/schema/{apiKey}/profile", async (context) =>
        {
            var Request = context.Request;

            var schema = await context
                .RequestServices
                .GetRequiredService<IGrainFactory>()
                .GetGrain<ISchemaRegistryGrain>(new Uri(Request.GetDisplayUrl()).GetParentUri().ToString().TrimEnd('/'))
                .GetSchemaAsync("profile")
                .ConfigureAwait(false);

            context.Items["schema"] = schema;
        })
            .WithName("get-profile-schema");


        app.MapGet("/schema/{apiKey}", async (context) =>
        {
            var Request = context.Request;


            var schema = await context
                .RequestServices
                .GetRequiredService<IGrainFactory>()
                .GetGrain<ISchemaGrain>(Request.GetDisplayUrl())
                .GetSchemaAsync()
                .ConfigureAwait(false);

            context.Items["schema"] = schema;
        })
            .WithName("get-site-schema");


        app.MapGet("/schema/{apiKey}/bundled", async (context) =>
        {
            var Request = context.Request;


            var schema = await context
                .RequestServices
                .GetRequiredService<IGrainFactory>()
                .GetGrain<IBundledSchemaGrain>(new Uri(Request.GetDisplayUrl()).GetParentUri().ToString().TrimEnd('/'))
                .GetSchemaAsync()
                .ConfigureAwait(false);

            context.Items["schema"] = schema;
        })
            .WithName("get-site-bundled-schema");

        app.MapPost("/schema/{apiKey}/{type}", async (context) =>
        {
            var Request = context.Request;
            var type = context.GetRouteValue("type") as string;

            var schema = await context
                .RequestServices
                .GetRequiredService<IGrainFactory>()
                .GetGrain<ISchemaRegistryGrain>(new Uri(Request.GetDisplayUrl()).GetParentUri().ToString().TrimEnd('/'))
                .GetSchemaAsync(type)
                .ConfigureAwait(false);

            context.Items["schema"] = schema;
        })
            .WithName("get-site-typed-schema");

        return app;
    }

    public static IApplicationBuilder UseSchemaMiddleware(this IApplicationBuilder app)
    {

        var logGetSchema =
            LoggerMessage.Define<string>(LogLevel.Trace, new EventId(12, "Get Schema"), "Get Schema {ID}");
        var logResolveSchema =
            LoggerMessage.Define<string>(LogLevel.Trace, new EventId(13, "Resolve Schema"), "Resolve Schema {ID}");


        //add middleware to resolve schema if x-resolve header is present
        app.Use(async (context, next) =>
            {
                var Request = context.Request;
                var Response = context.Response;
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                logGetSchema(logger, Request.GetDisplayUrl(), null);


                await next().ConfigureAwait(false);

                if (context.Items.TryGetValue("schema", out var schema) && schema is not null)
                {
                    logResolveSchema(logger, Request.GetDisplayUrl(), null);
                    // content type for json schema
                    Response.ContentType = "application/schema+json";
                    Response.Headers.ETag = "\"" + schema.GetHashCode() + "\"";
                    Response.Headers.CacheControl = new CacheControlHeaderValue()
                    {
                        NoCache = true, MaxAge = TimeSpan.FromDays(1), Public = true, MustRevalidate = true,
                    }.ToString();

                    await Response.WriteAsJsonAsync(schema).ConfigureAwait(false);
                }
            }
        );
        return app;
    }
}
