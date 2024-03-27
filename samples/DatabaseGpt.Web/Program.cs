using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using ChatGptNet;
using DatabaseGpt;
using DatabaseGpt.Web.Extensions;
using DatabaseGpt.Web.Services;
using DatabaseGpt.Web.Services.Interfaces;
using DatabaseGpt.Web.Settings;
using DatabaseGpt.Web.Swagger;
using Microsoft.OpenApi.Models;
using OperationResults.AspNetCore.Http;
using TinyHelpers.AspNetCore.Extensions;
using TinyHelpers.AspNetCore.Swagger;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true);

// Add services to the container.
var settings = builder.Services.ConfigureAndGet<AppSettings>(builder.Configuration, nameof(AppSettings));
var swagger = builder.Services.ConfigureAndGet<SwaggerSettings>(builder.Configuration, nameof(SwaggerSettings));

builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorPages();

builder.Services.AddWebOptimizer(minifyCss: true, minifyJavaScript: builder.Environment.IsProduction());

builder.Services.AddDatabaseGpt(database =>
{
    // For SQL Server.
    database.UseConfiguration(builder.Configuration)
            .UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection"));

    // For PostgreSQL.
    //database.UseConfiguration(context.Configuration)
    //        .UseNpgsql(context.Configuration.GetConnectionString("NpgsqlConnection"));
},
chatGpt =>
{
    chatGpt.UseConfiguration(builder.Configuration);
});

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(_ =>
    {
        return RateLimitPartition.GetTokenBucketLimiter("Default", _ => new TokenBucketRateLimiterOptions
        {
            TokenLimit = 100,
            TokensPerPeriod = 20,
            ReplenishmentPeriod = TimeSpan.FromMinutes(30),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = (context, token) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var window))
        {
            context.HttpContext.Response.Headers.RetryAfter = window.TotalSeconds.ToString();
        }

        return ValueTask.CompletedTask;
    };
});

builder.Services.AddDefaultProblemDetails();
builder.Services.AddDefaultExceptionHandler();

builder.Services.AddScoped<IChatService, ChatService>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

if (swagger.IsEnabled)
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "DatabaseGPT API", Version = "v1" });

        options.AddDefaultResponse();
    });
}

builder.Services.AddOperationResult(options =>
{
    options.ErrorResponseFormat = ErrorResponseFormat.List;
});

var app = builder.Build();
app.Services.GetRequiredService<IWebHostEnvironment>().ApplicationName = settings.ApplicationName;

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.UseWhen(context => context.IsWebRequest(), builder =>
{
    if (!app.Environment.IsDevelopment())
    {
        builder.UseExceptionHandler("/errors/500");

        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        builder.UseHsts();
    }

    builder.UseStatusCodePagesWithReExecute("/errors/{0}");
});

app.UseWhen(context => context.IsApiRequest(), builder =>
{
    builder.UseExceptionHandler();
    builder.UseStatusCodePages();
});

app.UseWebOptimizer();
app.UseStaticFiles();

if (swagger.IsEnabled)
{
    app.UseMiddleware<SwaggerBasicAuthenticationMiddleware>();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "DatabaseGPT API v1");
        options.InjectStylesheet("/css/swagger.css");
    });
}

app.UseRouting();

app.UseWhen(context => context.IsApiRequest(), builder =>
{
    builder.UseRateLimiter();
});

//app.UseCors();

// In Razor Pages apps and apps with controllers, UseOutputCache must be called after UseRouting.
//app.UseOutputCache();

app.MapEndpoints();
app.MapRazorPages();

app.Run();