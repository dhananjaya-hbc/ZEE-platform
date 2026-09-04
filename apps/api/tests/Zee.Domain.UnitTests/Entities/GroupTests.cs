using Zee.Domain.Common;
using Zee.Domain.Entities;
using Zee.Domain.Enums;

namespace Zee.Domain.UnitTests.Entities;

/// <summary>Invariants of <see cref="Group"/>, chiefly the campus-scoping rule.</summary>
public sealed class GroupTests
{
    private const string Todo = "TODO: implement Group, then remove this Skip.";

    private static readonly Guid UniversityId = Guid.CreateVersion7();
    private static readonly Guid CreatorId = Guid.CreateVersion7();

    [Theory(Skip = Todo)]
    [InlineData(GroupType.Course)]
    [InlineData(GroupType.Club)]
    [InlineData(GroupType.Dorm)]
    public void Create_requires_a_university_for_campus_scoped_types(GroupType type)
    {
        Should.Throw<DomainException>(
            () => Group.Create("Name", type, CreatorId, universityId: null));
    }

    [Fact(Skip = Todo)]
    public void Create_forbids_a_university_on_a_global_interest_group()
    {
        Should.Throw<DomainException>(
            () => Group.Create("Robotics", GroupType.GlobalInterest, CreatorId, UniversityId));
    }

    [Fact(Skip = Todo)]
    public void Create_enrols_the_creator_as_a_moderator()
    {
        var group = Group.Create("CS3300", GroupType.Course, CreatorId, UniversityId);

        group.HasMember(CreatorId).ShouldBeTrue();
        group.Members.ShouldHaveSingleItem().IsModerator.ShouldBeTrue();
    }

    [Fact(Skip = Todo)]
    public void AddMember_is_idempotent()
    {
        var group = Group.Create("CS3300", GroupType.Course, CreatorId, UniversityId);
        var student = Guid.CreateVersion7();

        group.AddMember(student);
        group.AddMember(student);

        group.Members.Count(m => m.UserId == student).ShouldBe(1);
    }

    [Fact(Skip = Todo)]
    public void RemoveMember_refuses_to_remove_the_last_moderator()
    {
        var group = Group.Create("CS3300", GroupType.Course, CreatorId, UniversityId);

        Should.Throw<DomainException>(() => group.RemoveMember(CreatorId));
    }
}
