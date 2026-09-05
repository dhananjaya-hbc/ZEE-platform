using System.Runtime.CompilerServices;

// Guard and the internal entity factories (GroupMembership.Create, Rsvp.Create) are not
// part of the public API, but they carry real invariants and deserve direct tests. Granting
// the test assembly access is preferable to widening their visibility for testing's sake.
[assembly: InternalsVisibleTo("Zee.Domain.UnitTests")]
