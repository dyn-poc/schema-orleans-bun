using schema.web.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient<SchemaRegistryService>();
builder.Services.AddSingleton<SchemaRegistryService>();

builder.Services.AddHostedService<SchemaRegistryService>(services => services.GetRequiredService<SchemaRegistryService>());

builder.Services.AddLogging(logger=>logger.AddConsole());
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();

    app.UseHttpsRedirection();

}

// if (app.Environment.IsDevelopment())
// {
//     app.UseSpa(spaBuilder =>
//     {
//         spaBuilder.Options.PackageManagerCommand = "yarn";
//         spaBuilder.Options.SourcePath = Path.GetFullPath("./RemixApp");
//
//         spaBuilder.UseReactDevelopmentServer("dev");
//     });
// }

app.UseStaticFiles();
app.UseRouting();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.UseCors(options => options
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()
);

app.AddSchemaApi();
app.Run();
