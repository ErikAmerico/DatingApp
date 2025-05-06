using System;
using API.Data; // ↳ Your EF Core DbContext sits here
using API.Interfaces; // ↳ Your ITokenService interface
using API.Services; // ↳ Your TokenService implementation
using Microsoft.EntityFrameworkCore; // ↳ EF Core methods like UseSqlite()

namespace API.Extensions;

// The big picture
// ApplicationServiceExtensions groups all your “business-specific” service registrations in one place (controllers, EF Core, CORS, your own services).
// Program.cs stays lean: call that one helper, then wire up authentication, middleware, and run.
// Extension methods (this IServiceCollection …) are just C# syntax sugar for static helper methods—you still get two parameters, but one is hidden in the “object you call it on.”

// ① static class to hold your “add all our services” helper
public static class ApplicationServiceExtensions
{
    // ② Extension method signature:
    //    - `this IServiceCollection services` is the object you call this on
    //    - `IConfiguration config` is the second input you *must* pass
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration config // ← this is the TokenKey + ConnectionStrings, etc.
    )
    {
        // 1) Register Controllers—for Web API endpoints
        //Tells ASP .NET Core you’ll be using [ApiController] + controller classes.
        //This wires up model binding, validation, attribute routing, etc.
        services.AddControllers();

        // 2) Configure Entity Framework Core + SQLite
        services.AddDbContext<DataContext>(opt =>
        {
            // Read your connection string from appsettings.json under "DefaultConnection"
            opt.UseSqlite(config.GetConnectionString("DefaultConnection"));
            //Behind the scenes: DataContext gets injected into controllers/services,
            // EF Core knows how to open your SQLite DB file and run migrations/queries.
        });

        // 3) Enable CORS (Cross-Origin Resource Sharing)
        //Later, we’ll specify which front-end origins (Angular on localhost:4200) can talk to this API.
        services.AddCors();

        // 4) Dependency Injection: your own services
        // 4) Wire up your own token service so you can inject ITokenService
        //“Whenever someone asks for ITokenService, give them a new TokenService instance.”
        //Scoped = one instance per HTTP request, ideal for stateless services.
        services.AddScoped<ITokenService, TokenService>();

        // 5) Return the same IServiceCollection so you can chain other calls
        return services;
    }
}
