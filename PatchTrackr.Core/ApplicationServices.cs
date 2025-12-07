using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PatchTrackr.Core.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace PatchTrackr.Core;

public static class ApplicationServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<ICommonService, CommonService>();
        services.AddScoped<IUsersService, UsersService>();
        services.AddScoped<ICryptoService, CryptoService>();

        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromHours(1);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(options =>
                    {
                        options.LoginPath = "/Home/Login";
                        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);   // Auto logout after 15 minutes
                        options.SlidingExpiration = true;                    // Extend if user is active
                        options.Cookie.HttpOnly = true;
                    });

        services.AddHttpContextAccessor();
        services.AddDbContext<AppDbContext>(options =>
        {
            string connStr = config.GetConnectionString("PatchTrackrConn")!;
            options.UseSqlServer(connStr);
        });

        return services;
    }
}
