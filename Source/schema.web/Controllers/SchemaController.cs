namespace schema.web.Controllers;

using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using Abstractions;
using Abstractions.Grains;
using Json.More;
using Json.Schema;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using CacheControlHeaderValue = Microsoft.Net.Http.Headers.CacheControlHeaderValue;

public static class SchemaAPI
{
    public static void AddSchemaApi(this WebApplication app)
    {
        static JsonObject GetSchema(string baseUri) => new JsonObject()
        {
            ["$schema"] = "https://json-schema.org/draft-07/schema#",
            ["$id"] = baseUri,
            // ["apiKey"] =apiKey,
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["name"] = new JsonObject
                {
                    ["$id"] = $"{baseUri}/name",
                    ["type"] = "string",
                    ["description"] = "Name of the person",
                    ["minLength"] = 2,
                    ["maxLength"] = 10
                },
                ["age"] = new JsonObject
                {
                    ["$anchor"] = "age", ["type"] = "integer", ["minimum"] = 18, ["maximum"] = 99
                }
            },
            ["required"] = new JsonArray { "name", "age" }
        };

          static JsonObject GetDataSchema(string baseUri) => new()
        {
            ["$schema"] = "https://json-schema.org/draft-07/schema#",
            ["$id"] = baseUri,
            ["$anchor"] = "data",
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["zip"] = new JsonObject
                {
                    ["$anchor"] = $"zip",
                    ["type"] = "string",
                    ["description"] = "Name of the person",
                    ["minLength"] = 2,
                    ["maxLength"] = 10
                },
                ["customer_id"] = new JsonObject
                {
                    ["$anchor"] = "customer_id", ["type"] = "integer", ["minimum"] = 18, ["maximum"] = 99
                }
            }
        };
        var logGetSchema=LoggerMessage.Define<string>(LogLevel.Trace, new EventId(12, "Get Schema"), "Get Schema {ID}");
        var logResolveSchema=LoggerMessage.Define<string>(LogLevel.Trace, new EventId(13, "Resolve Schema"), "Resolve Schema {ID}");




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
                        NoCache = true,
                        MaxAge = TimeSpan.FromDays(1), Public = true, MustRevalidate = true,
                    }.ToString();

                    await Response.WriteAsJsonAsync(schema).ConfigureAwait(false);
                }
            }
        );

        app.MapGet("/schema/{apiKey}/profile", async (context) =>
        {
            var Request = context.Request;

            var schema = await context
                .RequestServices
                .GetRequiredService<SchemaRegistryService>()
                .ClusterClient.GetGrain<ISchemaRegistryGrain>(new Uri(Request.GetDisplayUrl()).GetParentUri().ToString().TrimEnd('/'))
                .GetSchemaAsync("profile")
                .ConfigureAwait(false);

            context.Items["schema"] = schema;
        });



        app.MapGet("/schema/{apiKey}", async (context) =>
        {
            var Request = context.Request;
            var apiKey =context.GetRouteValue("apiKey");


            var schema = await context
                .RequestServices
                .GetRequiredService<SchemaRegistryService>()
                .ClusterClient.GetGrain<ISchemaGrain>(Request.GetDisplayUrl())
                .GetSchemaAsync()
                .ConfigureAwait(false);

            context.Items["schema"] = schema;
        });


        app.MapGet("/schema/{apiKey}/bundled", async (context) =>
        {
            var Request = context.Request;


            var schema = await app
                .Services
                .GetRequiredService<SchemaRegistryService>()
                .ClusterClient.GetGrain<IBundledSchemaGrain>(new Uri(Request.GetDisplayUrl()).GetParentUri().ToString().TrimEnd('/'))
                .GetSchemaAsync()
                .ConfigureAwait(false);

            context.Items["schema"] = schema;
        });

        app.MapPost("/schema/{apiKey}/{type}", async (context ) =>
        {
            var Request = context.Request;
            var type = context.GetRouteValue("type") as string;

            var schema = await context
                .RequestServices
                .GetRequiredService<SchemaRegistryService>()
                .ClusterClient.GetGrain<ISchemaRegistryGrain>(new Uri(Request.GetDisplayUrl()).GetParentUri().ToString().TrimEnd('/'))
                .GetSchemaAsync(type)
                .ConfigureAwait(false);

            context.Items["schema"] = schema;

        });

    }
}
