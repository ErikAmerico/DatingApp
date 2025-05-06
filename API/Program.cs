using API.Extensions;

//“Bootstrap” your app: sets up configuration (appsettings.json, ENV vars),
//logging, DI container, Kestrel server, etc.
var builder = WebApplication.CreateBuilder(args);

//Add services to the container

// 🧩 Here’s where you “plug in” all your app-specific services:
builder.Services.AddApplicationServices(
    builder.Configuration // pass in IConfiguration so your extension can read ConnectionStrings, TokenKey, etc.
);

// 🔐 Now add JWT-based auth from our IdentityServiceExtensions
builder.Services.AddIdentityServices(builder.Configuration);

// 6) Build the app (this compiles your service collection + middleware into a runnable pipeline)
var app = builder.Build();

// 7) Middleware pipeline (order matters!)
// Enables CORS for your Angular dev server: any header, any HTTP verb, only from those two URLs.
// 7a) CORS: allow your Angular dev server at localhost:4200 to call this API
// CORS, AuthN, AuthZ, Controllers—order matters!
app.UseCors(x =>
    x.AllowAnyHeader()
        .AllowAnyMethod()
        .WithOrigins("http://localhost:4200", "https://localhost:4200")
);

// 7.1) Authentication middleware
// Examine incoming requests for an Authorization header,
// validate the JWT, and set HttpContext.User accordingly.
//     - Reads the Authorization header
//     - Validates the JWT (signature, expiry) per your TokenValidationParameters
//     - Populates HttpContext.User with the token’s claims

app.UseAuthentication();

// 7.2) Authorization middleware
// After authentication, apply your [Authorize] attributes
// or policy rules—deny access if the user isn’t allowed.
//     - Runs after authentication
//     - Enforces any [Authorize] attributes or policy-based rules in your controllers
app.UseAuthorization();

//“Map” all your [ApiController] routes so they actually respond to HTTP requests.
// 8) Map your API controllers (@ApiController routes) so they start handling requests
app.MapControllers();

//Start listening on the default ports (usually 5000 for HTTP, 5001 for HTTPS),
//and never return—kills the thread only when you shut down the app.
app.Run();
