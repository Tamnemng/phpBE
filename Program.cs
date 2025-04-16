using Dapr.Client;
using Microsoft.OpenApi.Models;
using MediatR;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ApplicationName = typeof(Program).Assembly.FullName,
    WebRootPath = "wwwroot"
});

builder.WebHost.UseUrls("http://localhost:5000");
builder.Services.AddDaprClient(client =>
{
    // Cấu hình Dapr
});

// Thêm MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationErrorFilter>();
})
.ConfigureApiBehaviorOptions(options =>
{
    options.SuppressModelStateInvalidFilter = true;
})
// Configure JSON serialization to use string values for enums
.AddJsonOptions(options => 
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
})

.AddDapr();
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Think4 API", Version = "v1" });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Add error handling middleware early in the pipeline
app.UseErrorHandlingMiddleware(); 

app.UseAuthorization();
app.UseCloudEvents(); // Thêm middleware cho Dapr
app.MapControllers();
app.MapSubscribeHandler(); // Thêm endpoint cho Dapr pub/sub

app.Run();