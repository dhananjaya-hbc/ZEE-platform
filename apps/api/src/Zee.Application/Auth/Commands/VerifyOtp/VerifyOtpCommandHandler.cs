using MediatR;
using Zee.Application.Auth.Dtos;
using Zee.Application.Common.Exceptions;
using Zee.Application.Common.Interfaces;
using Zee.Domain.Entities;
using Zee.Domain.Enums;
using Zee.Domain.Repositories;

namespace Zee.Application.Auth.Commands.VerifyOtp;

/// <summary>Handles <see cref="VerifyOtpCommand"/>.</summary>
public sealed class VerifyOtpCommandHandler(
    IEmailVerificationCodeRepository codes,
    IUserRepository users,
    IUnitOfWork unitOfWork,
    IOtpService otp,
    ITokenService tokens)
    : IRequestHandler<VerifyOtpCommand, AuthResultDto>
{
    private readonly IEmailVerificationCodeRepository _codes = codes;
    private readonly IUserRepository _users = users;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IOtpService _otp = otp;
    private readonly ITokenService _tokens = tokens;

    public async Task<AuthResultDto> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        var verification = await _codes.GetActiveByEmailAsync(request.Email, cancellationToken)
            ?? throw new InvalidOtpException();

        var result = verification.Verify(_otp.Hash(request.Code));

        if (result != OtpVerificationResult.Success)
        {
            // Verify() may have incremented AttemptCount even though it failed - persist
            // that before throwing, or the attempt cap never actually takes effect.
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            throw new InvalidOtpException();
        }

        var user = await _users.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null)
        {
            var displayName = DeriveDisplayName(request.Email);
            user = User.Create(verification.UniversityId, request.Email, displayName);
            _users.Add(user);
        }

        user.MarkSeen();

        // One save for both the consumed code and the user change - atomic, so a code is
        // never left marked "consumed" without a user actually existing for it.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessToken = _tokens.GenerateAccessToken(user);

        return new AuthResultDto(accessToken, user.Id, user.Name, user.Email, user.UniversityId);
    }

    /// <summary>
    /// Turns the local part of an address into a display name, e.g. "ada.smith@mit.edu"
    /// becomes "Ada Smith". Used only when a student signs in for the first time and no
    /// name has been collected yet - they can change it later from their profile.
    /// </summary>
    private static string DeriveDisplayName(string email)
    {
        var localPart = email.Split('@')[0].Split('+')[0];

        var words = localPart
            .Split(['.', '_', '-'], StringSplitOptions.RemoveEmptyEntries)
            .Select(word => char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant());

        var name = string.Join(' ', words);

        return string.IsNullOrWhiteSpace(name) ? "Student" : name;
    }
}
