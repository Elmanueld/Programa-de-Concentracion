using System.Threading.RateLimiting;
using Backend.Data;
using Backend.Email;
using Backend.PasswordRecovery;
using Backend.Registration;
using Backend.Verification;
using Backend.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);
ValidateRequiredConfiguration(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ApiTokenService>();
builder.Services.AddAuthentication("Bearer")
    .AddScheme<AuthenticationSchemeOptions, ApiTokenAuthenticationHandler>("Bearer", _ => { });
builder.Services.AddAuthorization();

builder.Services.AddDbContext<VerificationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!));
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.Configure<VerificationOptions>(builder.Configuration.GetSection("Verification"));
builder.Services.AddScoped<IVerificationRepository, SqlVerificationRepository>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<IVerificationService, VerificationService>();
builder.Services.AddScoped<IAccountRegistrationRepository, SqlAccountRegistrationRepository>();
builder.Services.AddScoped<IAccountRegistrationService, AccountRegistrationService>();
builder.Services.AddScoped<IPasswordRecoveryRepository, SqlPasswordRecoveryRepository>();
builder.Services.AddScoped<IPasswordRecoveryService, PasswordRecoveryService>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("send-code", context => RateLimitPartition.GetFixedWindowLimiter(
        GetClientKey(context), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(10),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
    options.AddPolicy("confirm-code", context => RateLimitPartition.GetFixedWindowLimiter(
        GetClientKey(context), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 20,
            Window = TimeSpan.FromMinutes(10),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
    options.AddPolicy("register-account", context => RateLimitPartition.GetFixedWindowLimiter(
        GetClientKey(context), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(10),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
    options.AddPolicy("password-recovery-send", context => RateLimitPartition.GetFixedWindowLimiter(
        GetClientKey(context), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(10),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
    options.AddPolicy("password-recovery-confirm", context => RateLimitPartition.GetFixedWindowLimiter(
        GetClientKey(context), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 20,
            Window = TimeSpan.FromMinutes(10),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
});

var app = builder.Build();

// All clients use this UTC sample to align task start/end decisions across PCs.
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        context.Response.Headers["X-Server-Utc"] = DateTimeOffset.UtcNow.ToString("O");
        return Task.CompletedTask;
    });
    await next();
});

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

    var (status, message) = error switch
    {
        VerificationRateLimitException => (StatusCodes.Status429TooManyRequests,
            "Espera un minuto antes de solicitar otro código."),
        EmailDeliveryException => (StatusCodes.Status503ServiceUnavailable,
            "No se pudo enviar el correo. Inténtalo más tarde."),
        PasswordRecoveryEmailDeliveryException => (StatusCodes.Status503ServiceUnavailable,
            "No se pudo enviar el correo. Inténtalo más tarde."),
        SqlException => (StatusCodes.Status503ServiceUnavailable,
            "La base de datos remota no está disponible temporalmente."),
        _ => (StatusCodes.Status500InternalServerError,
            "Ocurrió un error procesando la solicitud.")
    };

    if (status >= StatusCodes.Status500InternalServerError)
    {
        logger.LogError(error,
            "Backend API request failed. Status: {Status}; TraceId: {TraceId}; Method: {Method}; Path: {Path}",
            status,
            context.TraceIdentifier,
            context.Request.Method,
            context.Request.Path.Value);
        context.Response.Headers["X-Error-Reference"] = context.TraceIdentifier;
    }

    context.Response.StatusCode = status;
    if (status == StatusCodes.Status500InternalServerError)
        await context.Response.WriteAsJsonAsync(new { message, reference = context.TraceIdentifier }, context.RequestAborted);
    else
        await context.Response.WriteAsJsonAsync(new { message }, context.RequestAborted);
}));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapGet("/health/live", () => Results.Ok(new { status = "ok" }))
    .WithName("Liveness");
app.MapControllers();

app.Run();

static string GetClientKey(HttpContext context) =>
    context.Connection.RemoteIpAddress?.MapToIPv6().ToString() ?? "unknown";

static void ValidateRequiredConfiguration(IConfiguration configuration)
{
    var requiredSettings = new (string ConfigurationKey, string EnvironmentVariable)[]
    {
        ("ConnectionStrings:DefaultConnection", "ConnectionStrings__DefaultConnection"),
        ("Smtp:FromEmail", "Smtp__FromEmail"),
        ("Smtp:Username", "Smtp__Username"),
        ("Smtp:Password", "Smtp__Password"),
        ("Verification:HashKey", "Verification__HashKey"),
        ("Authentication:TokenSigningKey", "Authentication__TokenSigningKey")
    };

    var missingVariables = requiredSettings
        .Where(setting => string.IsNullOrWhiteSpace(configuration[setting.ConfigurationKey]))
        .Select(setting => setting.EnvironmentVariable)
        .ToArray();

    if (missingVariables.Length > 0)
    {
        throw new InvalidOperationException(
            $"Falta configuración obligatoria. Configure: {string.Join(", ", missingVariables)}.");
    }

    if (configuration["Authentication:TokenSigningKey"]!.Length < 32)
        throw new InvalidOperationException("Authentication__TokenSigningKey debe tener al menos 32 caracteres.");
}

public partial class Program { }
