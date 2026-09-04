using Zee.Domain.Entities;

namespace Zee.Application.Common.Interfaces;

/// <summary>
/// Issues signed access tokens for authenticated students.
/// </summary>
/// <remarks>
/// Declared here, implemented in Infrastructure — same pattern as every other interface in
/// this folder. The claims this produces are a contract with <c>CurrentUser</c> in the Api
/// layer: it reads <c>ClaimTypes.NameIdentifier</c> for the user id and
/// <c>"zee:university_id"</c> for the campus. Both sides must agree on those claim types,
/// or a token verifies fine but <c>ICurrentUser.UserId</c> comes back null anyway.
/// </remarks>
public interface ITokenService
{
    /// <summary>Issues a signed JWT for the given student.</summary>
    string GenerateAccessToken(User user);
}
