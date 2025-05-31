using Azure.Identity;
using Luna.Api.Services;
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

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.UseAzureAppConfiguration();

app.MapControllers();

app.UseCors(builder => 
    builder.AllowAnyOrigin());

app.Run();
