using MediatR;
using Zee.Application.Common.Exceptions;
using Zee.Application.Common.Interfaces;
using Zee.Domain.Entities;
using Zee.Domain.Repositories;

namespace Zee.Application.Auth.Commands.RequestOtp;

/// <summary>Handles <see cref="RequestOtpCommand"/>.</summary>
/// <remarks>
/// Order of operations matters here for two reasons: correctness (a code must exist in
/// the database before it can be verified) and cost (rate limiting and the university
/// lookup both happen before any code is generated, so a rejected request does no
/// wasted work).
/// </remarks>
public sealed class RequestOtpCommandHandler(
    IUniversityRepository universities,
    IEmailVerificationCodeRepository codes,
    IUnitOfWork unitOfWork,
    IOtpService otp,
    IEmailSender email)
    : IRequestHandler<RequestOtpCommand>
{
    /// <summary>Codes allowed per address in <see cref="RateLimitWindow"/>.</summary>
    private const int RateLimitMax = 3;

    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(15);

    private readonly IUniversityRepository _universities = universities;
    private readonly IEmailVerificationCodeRepository _codes = codes;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IOtpService _otp = otp;
    private readonly IEmailSender _email = email;

    public async Task Handle(RequestOtpCommand request, CancellationToken cancellationToken)
    {
        var university = await _universities.FindByEmailDomainAsync(request.Email, cancellationToken)
            ?? throw new NotFoundException(
                $"No university is onboarded for the domain of '{request.Email}'.");

        var since = DateTimeOffset.UtcNow.Subtract(RateLimitWindow);
        var recentCount = await _codes.CountIssuedSinceAsync(request.Email, since, cancellationToken);

        if (recentCount >= RateLimitMax)
        {
            throw new TooManyRequestsException(
                "Too many codes requested for this address. Please wait before trying again.");
        }

        var code = _otp.GenerateCode();
        var hash = _otp.Hash(code);

        var verification = EmailVerificationCode.Create(request.Email, university.Id, hash);

        _codes.Add(verification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Plaintext, never the hash - this is the only place the code exists outside the
        // student's inbox, and it is never persisted or logged in production.
        await _email.SendOtpCodeAsync(request.Email, code, cancellationToken);
    }
}

