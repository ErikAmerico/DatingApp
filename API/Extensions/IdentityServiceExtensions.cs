using System.Text; // ↳ Needed to turn your TokenKey string into a byte[] for crypto
using Microsoft.AspNetCore.Authentication.JwtBearer; // ↳ Brings in the JWT “Bearer” auth handler
using Microsoft.IdentityModel.Tokens; // ↳ TokenValidationParameters, SymmetricSecurityKey // ↳ Types for defining how tokens get validated

namespace API.Extensions;

// How these two files connect
// Extension methods (AddIdentityServices & AddApplicationServices) are just static helpers that take an IServiceCollection + IConfiguration and register a bunch of services in one go.
// In Program.cs you only pass one argument (builder.Configuration) because the other (builder.Services) is implied by the this IServiceCollection services syntax.
// Under the hood, services.AddIdentityServices(config) is compiled to
// csharp
// Copy
// Edit
// IdentityServiceExtensions.AddIdentityServices(services, config);
// This keeps Program.cs super-readable—just a couple of high-level calls—while the real wiring lives in your Extensions classes.

// ① Static class to hold extension methods for IServiceCollection.
//   This keeps your Program.cs nice and tidy.
public static class IdentityServiceExtensions
{
    // ② Extension method signature:
    //    - `this IServiceCollection services` means you can call AddIdentityServices() on any IServiceCollection instance.
    //    - `IConfiguration config` is your app’s settings (appsettings.json, ENV vars, etc.).
    public static IServiceCollection AddIdentityServices(
        this IServiceCollection services,
        IConfiguration config
    )
    {
        // 5) Configure JWT Bearer Authentication
        // 1) Tell ASP.NET Core “use JWT Bearer auth” as our default authentication scheme.
        // Next up: authentication bits that aren’t in your extensions:
        services
            // From now on, incoming requests will look for "Authorization: Bearer <token>" headers.
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //Sets the **default** auth scheme to “Bearer” (i.e. Authorization: Bearer <token>)
            .AddJwtBearer(options =>
            {
                // 2) Grab the secret key you store in appsettings.json under "TokenKey"
                // Grab your secret for signing/verifying JWTs
                var tokenKey = config["TokenKey"] ?? throw new Exception("Token key not found");

                // 5b) Tell ASP.NET how to validate incoming tokens:
                // 3) Configure how the middleware should validate tokens:
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // 3a) Ensure the token’s signature matches using our symmetric key
                    ValidateIssuerSigningKey = true, //Make sure the signature is valid
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)), //The same key you used in TokenService

                    // 3b) In this simple setup, we’re not verifying who issued the token
                    ValidateIssuer = false, //We’re not checking “who issued” the token
                    //     or who the intended audience is (e.g. same domain)
                    ValidateAudience = false, //We’re not checking “who’s allowed” to use it
                    // (You could lock these down in production for extra security)

                    // (In production you might set ValidIssuer and ValidAudience to lock things down.)
                };
            });

        // 4) Return the same IServiceCollection so calls can be chained fluently.
        return services;
    }
}
