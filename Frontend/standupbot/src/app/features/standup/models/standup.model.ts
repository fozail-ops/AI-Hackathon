/**
 * Blocker status enum.
 */
export enum BlockerStatus {
  New = 'New',
  Critical = 'Critical',
  Resolved = 'Resolved'
}

/**
 * Blocker status labels for display.
 */
export const BlockerStatusLabels: Record<BlockerStatus, string> = {
  [BlockerStatus.New]: 'New',
  [BlockerStatus.Critical]: 'Critical',
  [BlockerStatus.Resolved]: 'Resolved'
};

/**
 * Represents a standup entry.
 */
export interface Standup {
  id: number;
  userId: number;
  userName: string;
  date: Date;
  jiraId: string;
  taskDescription: string;
  percentageComplete: number;
  hasBlocker: boolean;
  blockerDescription: string | null;
  blockerStatus: BlockerStatus | null;
  nextTask: string;
  createdAt: Date;
  updatedAt: Date | null;
}

/**
 * Summary standup for list views.
 */
export interface StandupSummary {
  id: number;
  userId: number;
  userName: string;
  date: Date;
  jiraId: string;
  percentageComplete: number;
  hasBlocker: boolean;
  blockerStatus: BlockerStatus | null;
  createdAt: Date;
}

/**
 * Request for creating a standup.
 */
export interface CreateStandupRequest {
  jiraId: string;
  taskDescription: string;
  percentageComplete: number;
  hasBlocker: boolean;
  blockerDescription?: string;
  nextTask: string;
}

/**
 * Request for updating a standup.
 */
export interface UpdateStandupRequest {
  jiraId?: string;
  taskDescription?: string;
  percentageComplete?: number;
  hasBlocker?: boolean;
  blockerDescription?: string;
  nextTask?: string;
}

/**
 * Submission status response.
 */
export interface SubmissionStatus {
  hasSubmittedToday: boolean;
}

/**
 * Percentage options for dropdown.
 */
export const PERCENTAGE_OPTIONS = [
  { value: 0, label: '0%' },
  { value: 10, label: '10%' },
  { value: 20, label: '20%' },
  { value: 30, label: '30%' },
  { value: 40, label: '40%' },
  { value: 50, label: '50%' },
  { value: 60, label: '60%' },
  { value: 70, label: '70%' },
  { value: 80, label: '80%' },
  { value: 90, label: '90%' },
  { value: 100, label: '100%' }
];
