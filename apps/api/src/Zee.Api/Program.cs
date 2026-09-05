using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
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
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

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

// The database check is TAGGED rather than unconditional, so it can be excluded
// from the cheap liveness probe below. See the MapHealthChecks calls for why.
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("postgres", tags: ["ready"]);

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

// ---------------------------------------------------------------------------
// Health endpoints — deliberately two of them.
//
// Neon scales its compute to zero after a few minutes of inactivity. A health
// check that touches the database on every poll would keep waking it, so the
// compute never idles down and burns hours continuously for no benefit.
//
//   /health        liveness  — "is this process up". Runs NO checks, touches
//                              nothing. This is what an orchestrator or uptime
//                              monitor should poll frequently.
//   /health/ready  readiness — "can this process serve traffic". Runs the
//                              tagged database check. Poll this rarely, or on
//                              deploy only.
//
// Predicate: _ => false means "run none of the registered checks" and report
// Healthy if the pipeline responds at all.
// ---------------------------------------------------------------------------
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => false,
}).AllowAnonymous();

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
}).AllowAnonymous();

await app.RunAsync();

/// <summary>
/// Exposed so the integration test project can reference this entry point with
/// <c>WebApplicationFactory&lt;Program&gt;</c>. Top-level statements generate an internal
/// class, which the test assembly can only see via InternalsVisibleTo in the csproj.
/// </summary>
public partial class Program;
