using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Zee.Api.Middleware;
using Zee.Api.Services;
using Zee.Application;
using Zee.Application.Common.Interfaces;
using Zee.Infrastructure;
using Zee.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Services
//
// Each layer registers itself. Program.cs names layers, not individual types,
// so adding a repository or a handler never means editing this file.
// ---------------------------------------------------------------------------
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

// ICurrentUser is implemented in this layer because it reads HTTP claims.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// ---------------------------------------------------------------------------
// Authentication
//
// Bearer JWTs issued by this API after a student redeems an emailed OTP.
// There are no passwords anywhere in ZEE - see docs/Architecture.md.
// ---------------------------------------------------------------------------
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "zee";

if (string.IsNullOrWhiteSpace(jwtKey))
{
    // Fail at startup rather than accepting unsigned tokens later. A missing signing key
    // is not a condition to degrade gracefully through.
    throw new InvalidOperationException(
        "Jwt:Key is not configured. Copy .env.example to .env and set JWT__KEY.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtIssuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,

            // Default is 5 minutes of leeway on expiry. Tighten it: these tokens are
            // issued and consumed by the same system, so there is no clock skew to absorb.
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services.AddAuthorization();

// ---------------------------------------------------------------------------
// CORS
//
// The web app is a separate origin, so it needs explicit permission. The allowed
// origins come from configuration - never AllowAnyOrigin, which cannot be combined
// with credentials and would let any site call this API on a student's behalf.
// ---------------------------------------------------------------------------
const string WebAppCorsPolicy = "web-app";

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:3000"];

builder.Services.AddCors(options =>
    options.AddPolicy(WebAppCorsPolicy, policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("postgres");

var app = builder.Build();

// ---------------------------------------------------------------------------
// Pipeline
//
// Order matters. Exception handling goes FIRST so it wraps everything after it;
// registered later, it would not catch faults in the middleware ahead of it.
// ---------------------------------------------------------------------------
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Applying migrations on startup is a development convenience only. In production
    // this is a deployment step: two instances starting together would otherwise race
    // to migrate the same database.
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseCors(WebAppCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

await app.RunAsync();

/// <summary>
/// Exposed so the integration test project can reference this entry point with
/// <c>WebApplicationFactory&lt;Program&gt;</c>. Top-level statements generate an internal
/// class, which the test assembly can only see via InternalsVisibleTo in the csproj.
/// </summary>
public partial class Program;
