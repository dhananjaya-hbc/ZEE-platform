using Microsoft.Extensions.Logging;
using Zee.Application.Common.Interfaces;

namespace Zee.Infrastructure.Auth;

/// <summary>
/// Development-only <see cref="IEmailSender"/> that writes the code to the logs instead of
/// sending real mail.
/// </summary>
/// <remarks>
/// Registered only when <c>IHostEnvironment.IsDevelopment()</c> is true - see
/// <c>DependencyInjection.AddAuth</c>. A production deployment must supply a real
/// implementation (SendGrid, SES, etc.); this one intentionally has no fallback for that
/// case, so misconfiguring it in production fails loudly rather than silently discarding
/// every login code.
/// </remarks>
public sealed class DevConsoleEmailSender(ILogger<DevConsoleEmailSender> logger) : IEmailSender
{
    private readonly ILogger<DevConsoleEmailSender> _logger = logger;

        public Task SendOtpCodeAsync(string email, string code, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "[DEV] OTP code for {Email}: {Code} (not actually emailed - see docs/Architecture.md#authentication)",
                email,
                code);
        }

        return Task.CompletedTask;
    }

}
