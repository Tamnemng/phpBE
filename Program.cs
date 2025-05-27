using Dapr.Client;
using Microsoft.OpenApi.Models;
using MediatR;
using System.Text.Json.Serialization;
using Think4.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ApplicationName = typeof(Program).Assembly.FullName,
    WebRootPath = "wwwroot"
});

builder.WebHost.UseUrls("http://0.0.0.0:5000");
builder.Services.AddDaprClient(client =>
{
    // Dapr configuration
});

// Add authentication services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

// Setup JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(
                builder.Configuration["Jwt:Secret"] ?? "YourVeryLongSecretKeyHereForThink4ApiSecurity123!")),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Add MediatR
builder.Services.AddMediatR(typeof(Program));

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
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Add error handling middleware early in the pipeline
app.UseErrorHandlingMiddleware();

// Enable CORS
app.UseCors();

// Authentication
app.UseAuthentication();
app.UseAuthorization();

// Add active session checking middleware
app.UseMiddleware<ActiveSessionMiddleware>();

app.UseCloudEvents(); // Add middleware for Dapr
app.MapControllers();
app.MapSubscribeHandler(); // Add endpoint for Dapr pub/sub

app.Run();