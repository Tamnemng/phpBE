using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MediatR;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        // Thêm Dapr Client
        services.AddDaprClient();

        // Thêm MediatR
        services.AddMediatR(typeof(Startup).Assembly);

        // Cấu hình xác thực JWT
        var jwtSecret = Configuration["Jwt:Key"] ?? "YourSecretKeyHereMakeSureItIsLongEnoughForSecurity";
        var key = Encoding.ASCII.GetBytes(jwtSecret);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // true trong production
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = Configuration["Jwt:Issuer"] ?? "your_issuer",
                ValidAudience = Configuration["Jwt:Audience"] ?? "your_audience",
                ValidateLifetime = true
            };
        });

        services.AddAuthorization();
        services.AddControllers();

        // Thêm hỗ trợ cookie để lưu trữ thông tin phiên
        services.AddDistributedMemoryCache();
        services.AddSession(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.IdleTimeout = TimeSpan.FromMinutes(5); // Đồng bộ với thời gian phiên IP
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseRouting();

        // Sử dụng session
        app.UseSession();

        // Thêm middleware xác thực IP trước middleware xác thực JWT
        app.UseMiddleware<IpAuthMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}