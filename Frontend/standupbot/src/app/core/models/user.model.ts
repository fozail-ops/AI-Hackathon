/**
 * User roles in the StandupBot system.
 */
export enum UserRole {
  Member = 'Member',
  Lead = 'Lead'
}

/**
 * User role display labels.
 */
export const UserRoleLabels: Record<UserRole, string> = {
  [UserRole.Member]: 'Team Member',
  [UserRole.Lead]: 'Team Lead'
};

/**
 * Represents a User in the StandupBot system.
 */
export interface User {
  id: number;
  name: string;
  email: string;
  role: UserRole;
  teamId: number;
  teamName: string;
  avatarColor: string;
}

/**
 * Get user initials for avatar display.
 */
export function getUserInitials(user: User): string {
  return user.name
    .split(' ')
    .map(part => part[0])
    .join('')
    .toUpperCase()
    .slice(0, 2);
}

/**
 * Check if user is a team lead.
 */
export function isTeamLead(user: User): boolean {
  return user.role === UserRole.Lead;
}
