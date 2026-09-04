using Zee.Domain.Entities;
using Zee.Domain.Enums;

namespace Zee.Domain.UnitTests.Entities;

/// <summary>
/// Invariants of <see cref="EmailVerificationCode"/> - ZEE's entire authentication state.
/// </summary>
///
/// To pick this up: implement EmailVerificationCode, then remove the Skip arguments.
/// Read the TODO on Verify() first; the ORDER of the state checks is part of the contract.
public sealed class EmailVerificationCodeTests
{
    private const string Todo = "TODO: implement EmailVerificationCode, then remove this Skip.";
    private const string RightHash = "hash-of-the-real-code";
    private const string WrongHash = "hash-of-a-wrong-guess";

    private static readonly Guid UniversityId = Guid.CreateVersion7();

    private static EmailVerificationCode Issued() =>
        EmailVerificationCode.Create("ada@mit.edu", UniversityId, RightHash);

    [Fact(Skip = Todo)]
    public void Create_normalises_the_email_and_sets_an_expiry()
    {
        var code = EmailVerificationCode.Create("Ada@MIT.edu", UniversityId, RightHash);

        code.Email.ShouldBe("ada@mit.edu");
        code.ExpiresAt.ShouldBeGreaterThan(DateTimeOffset.UtcNow);
        code.AttemptCount.ShouldBe(0);
        code.IsConsumed.ShouldBeFalse();
    }

    [Fact(Skip = Todo)]
    public void Verify_succeeds_and_consumes_the_code_on_a_matching_hash()
    {
        var code = Issued();

        code.Verify(RightHash).ShouldBe(OtpVerificationResult.Success);
        code.IsConsumed.ShouldBeTrue();
    }

    [Fact(Skip = Todo)]
    public void Verify_counts_the_attempt_on_a_wrong_hash()
    {
        var code = Issued();

        code.Verify(WrongHash).ShouldBe(OtpVerificationResult.IncorrectCode);
        code.AttemptCount.ShouldBe(1);
        code.IsConsumed.ShouldBeFalse();
    }

    [Fact(Skip = Todo)]
    public void Verify_rejects_reuse_of_a_consumed_code()
    {
        var code = Issued();
        code.Verify(RightHash);

        code.Verify(RightHash).ShouldBe(OtpVerificationResult.AlreadyUsed);
    }

    /// <summary>
    /// The attempt cap must burn the code outright: once the limit is reached, even the
    /// correct hash is refused. A cap that still lets a correct guess through is not a cap.
    /// </summary>
    [Fact(Skip = Todo)]
    public void Verify_refuses_the_correct_hash_once_the_attempt_cap_is_reached()
    {
        var code = Issued();

        for (var i = 0; i < EmailVerificationCode.MaxAttempts; i++)
        {
            code.Verify(WrongHash);
        }

        code.Verify(RightHash).ShouldBe(OtpVerificationResult.TooManyAttempts);
        code.IsConsumed.ShouldBeFalse();
    }

    [Fact(Skip = Todo)]
    public void Verify_treats_a_null_candidate_as_a_mismatch_rather_than_throwing()
    {
        Issued().Verify(null!).ShouldBe(OtpVerificationResult.IncorrectCode);
    }

    /// <remarks>
    /// Expiry is time-dependent, so it needs either a clock abstraction on the entity or
    /// TimeProvider. Introducing one is part of this task - see the TODO on Verify().
    /// </remarks>
    [Fact(Skip = "TODO: needs a clock abstraction (TimeProvider) to test expiry deterministically.")]
    public void Verify_rejects_an_expired_code()
    {
        // Arrange a code whose ExpiresAt is in the past, then assert OtpVerificationResult.Expired.
    }
}
