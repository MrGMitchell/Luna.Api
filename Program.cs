using Azure.Identity;
using Luna.Api.Services.CosmosDB;
using Luna.Api.Services.SpendingPlan;
using Luna.Api.Services.Football;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

var builder = WebApplication.CreateBuilder(args);

string endpoint = builder.Configuration.GetValue<string>("Endpoints:AppConfiguration")
    ?? throw new InvalidOperationException("The setting `Endpoints:AppConfiguration` was not found.");

builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(new Uri(endpoint), new DefaultAzureCredential())
           .Select(KeyFilter.Any, LabelFilter.Null)
           .ConfigureRefresh(refreshOptions =>
               refreshOptions.RegisterAll());
});

// Add services to the container.

builder.Services.AddAzureAppConfiguration();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ICosmosDbService, CosmosDbService>();
builder.Services.AddSingleton<ISpendingPlanService, SpendingPlanService>();
builder.Services.AddSingleton<IFootballQuestionService, FootballQuestionService>();
builder.Services.AddSingleton<ISubscriberService, SubscriberService>();
builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();

builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
{
    var endpointUri = builder.Configuration.GetValue<string>("CosmosDbEndPoint");

    if (string.IsNullOrWhiteSpace(endpointUri))
    {
        throw new InvalidOperationException("CosmosDbEndPoint configuration value is missing or empty.");
    }
    
    var key = builder.Configuration.GetValue<string>("CosmosDbKey");

    if (string.IsNullOrWhiteSpace(endpointUri))
    {
        throw new InvalidOperationException("CosmosDbKey configuration value is missing or empty.");
    }

    return new CosmosClient(endpointUri, key);
});

var app = builder.Build();

app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.UseAzureAppConfiguration();

app.MapControllers();

app.UseCors(builder => 
    builder.AllowAnyOrigin());

app.Run();
