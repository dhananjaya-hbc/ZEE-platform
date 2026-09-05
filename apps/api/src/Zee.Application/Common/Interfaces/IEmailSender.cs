namespace Zee.Application.Common.Interfaces;

/// <summary>
/// Delivers the one-time code to a student's institutional inbox.
/// </summary>
/// <remarks>
/// Declared here, implemented in Infrastructure - same pattern as every other interface in
/// this folder. In Development, the implementation logs the code instead of sending real
/// mail, so signing in locally needs no email provider configured. Production needs a real
/// implementation (SendGrid, SES, etc.) wired in behind this same interface - nothing above
/// it would need to change.
/// </remarks>
public interface IEmailSender
{
    /// <summary>Sends a one-time code to <paramref name="email"/>.</summary>
    Task SendOtpCodeAsync(string email, string code, CancellationToken cancellationToken = default);
}
