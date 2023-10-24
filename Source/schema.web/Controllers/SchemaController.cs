namespace schema.web.Controllers;

using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using Abstractions;
using Json.More;
using Json.Schema;
using Microsoft.AspNetCore.Http.Extensions;
using Orleans;
using CacheControlHeaderValue = Microsoft.Net.Http.Headers.CacheControlHeaderValue;

public static class SchemaAPI
{
    public static void AddSchemaApi(this WebApplication app)
    {
        static JsonObject GetSchema(string baseUri) => new JsonObject()
        {
            ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
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



        //add middleware to resolve schema if x-resolve header is present
        app.Use(async (context, next) =>
            {
                var Request = context.Request;
                var Response = context.Response;
                var logger = app.Services.GetRequiredService<ILogger<Program>>();
                var httpClient = app.Services.GetRequiredService<IHttpClientFactory>().CreateClient("schema");
                logger.LogInformation("Resolve Schema");


                await next().ConfigureAwait(false);

                if (context.Items.TryGetValue("schema", out var schema) && schema is not null)
                {
                    // var resolve = Request.Headers["x-resolve"].FirstOrDefault();
                    // if (resolve != null)
                    // {
                    //
                    //     logger.LogInformation(schema.ToString());
                    //     schema = JsonSchema.FromText(schema.ToString()).Bundle();
                    // }

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

        app.MapGet("/schema/{apiKey}", async (context) =>
        {
            var Request = context.Request;
            var Response = context.Response;
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Get Schema");


            var baseUri = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            // var schema = new JsonObject()
            // {
            //     ["$id"] = baseUri,
            //     ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
            //     ["type"] = "object",
            //     ["properties"] = new JsonObject()
            //     {
            //         ["profile"] = new JsonObject() { ["$ref"] = $"{baseUri}/profile" },
            //         ["data"] = new JsonObject() { ["$ref"] = $"{baseUri}/profile" }
            //         // ["name"] = new JsonObject() { ["$ref"] = $"profile/name" }
            //     }
            // };
            logger.LogInformation("Get Schema");
            var schema = await app.Services
                .GetRequiredService<SchemaRegistryService>()
                .ClusterClient.GetGrain<IResolvedSchemaGrain>(baseUri)
                .GetSchema();

            logger.LogInformation(JsonSerializer.Serialize(schema, new JsonSerializerOptions()
            {
                WriteIndented = true
            }));
            context.Items["schema"] = schema;
        });

        app.MapGet("/schema/{apiKey}/profile", async (context) =>
        {
            var Request = context.Request;
            var Response = context.Response;
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Get Schema");

            var baseUri = Request.GetDisplayUrl();
            var schema = GetSchema(baseUri);
            context.Items["schema"] = schema;
        });
    }
}
