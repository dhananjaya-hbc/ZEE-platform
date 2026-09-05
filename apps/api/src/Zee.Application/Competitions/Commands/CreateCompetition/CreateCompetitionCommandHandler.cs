using MediatR;
using Zee.Application.Common.Interfaces;
using Zee.Application.Competitions.Dtos;
using Zee.Domain.Repositories;

namespace Zee.Application.Competitions.Commands.CreateCompetition;

/// <summary>Handles <see cref="CreateCompetitionCommand"/>.</summary>
///
/// TODO: Implement Handle.
/// Acceptance criteria:
///   1. Read userId and universityId from _currentUser; throw ForbiddenAccessException if
///      either is null.
///   2. Translate the scope flag:
///        OpenToAllCampuses == true  -> universityId argument is null
///        OpenToAllCampuses == false -> universityId argument is the CALLER's university
///      Never take a university id from the request.
///   3. Build with Competition.Create(...), passing organizerId: userId.
///   4. _competitions.Add(...), then await _unitOfWork.SaveChangesAsync(cancellationToken).
///   5. Return CompetitionDto.FromEntity(competition).
///
/// Tests in tests/Zee.Application.UnitTests/Competitions/CreateCompetitionCommandHandlerTests.cs.
/// Cover both scope branches - that mapping is the easiest thing here to get backwards.
public sealed class CreateCompetitionCommandHandler(
    ICompetitionRepository competitions,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<CreateCompetitionCommand, CompetitionDto>
{
    private readonly ICompetitionRepository _competitions = competitions;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUser _currentUser = currentUser;

    public Task<CompetitionDto> Handle(CreateCompetitionCommand request, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}
