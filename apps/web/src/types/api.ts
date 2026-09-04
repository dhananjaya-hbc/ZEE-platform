/**
 * TypeScript mirrors of the .NET API's DTOs.
 *
 * These correspond 1:1 to the C# records in
 * `apps/api/src/Zee.Application/**\/Dtos/`. They are hand-maintained in Phase 1.
 *
 * TODO: Generate these from the API's OpenAPI document instead of hand-writing
 * them. The API already serves /openapi/v1.json in development, so something like
 * `openapi-typescript` would keep the two sides from drifting — which they will,
 * silently, the first time someone renames a field on the C# side.
 */

export type PostVisibility = 'Global' | 'University' | 'Group';

export type GroupType = 'Course' | 'Club' | 'Dorm' | 'GlobalInterest';

export type CompetitionCategory = 'Hackathon' | 'Robotics' | 'CaseCompetition' | 'Other';

export type RsvpStatus = 'Going' | 'Interested';

export interface PostDto {
  id: string;
  authorId: string;
  authorName: string;
  content: string;
  groupId: string | null;
  groupName: string | null;
  visibility: PostVisibility;
  createdAt: string;
  editedAt: string | null;
}

export interface EventDto {
  id: string;
  title: string;
  universityId: string;
  location: string;
  startTime: string;
  endTime: string;
  description: string | null;
  createdBy: string;
  creatorName: string;
  goingCount: number;
  interestedCount: number;
  viewerStatus: RsvpStatus | null;
}

export interface CompetitionDto {
  id: string;
  title: string;
  category: CompetitionCategory;
  organizerId: string;
  organizerName: string;
  universityId: string | null;
  isOpenToAllCampuses: boolean;
  startDate: string;
  endDate: string;
  description: string;
  recruitingTeams: boolean;
}

export interface ChatbotSource {
  id: string;
  title: string;
  url: string | null;
}

export interface ChatbotAnswer {
  answer: string;
  sources: ChatbotSource[];
}

/**
 * One page of a keyset-paginated listing.
 *
 * `nextCursor` is OPAQUE — pass it back verbatim and never try to parse or
 * construct one. Its format is an implementation detail of the API and will
 * change when Phase 2 adds ranked feeds.
 */
export interface CursorPage<T> {
  items: T[];
  nextCursor: string | null;
  hasMore: boolean;
}

/** RFC 9457 problem document, as returned by the API's exception middleware. */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  traceId?: string;
  /** Present on validation failures: messages keyed by field name. */
  errors?: Record<string, string[]>;
}
