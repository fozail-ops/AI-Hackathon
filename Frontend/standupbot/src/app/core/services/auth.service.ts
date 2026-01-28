import { Injectable, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { User, UserRole, isTeamLead } from '../models/user.model';

/**
 * Hardcoded sample users per PRD requirements.
 * Team: Product Engineering
 */
const SAMPLE_USERS: User[] = [
  {
    id: 1,
    name: 'Alice Johnson',
    email: 'alice.johnson@company.com',
    role: UserRole.Lead,
    teamId: 1,
    teamName: 'Product Engineering',
    avatarColor: '#6366f1' // indigo
  },
  {
    id: 2,
    name: 'Bob Smith',
    email: 'bob.smith@company.com',
    role: UserRole.Member,
    teamId: 1,
    teamName: 'Product Engineering',
    avatarColor: '#22c55e' // green
  },
  {
    id: 3,
    name: 'Carol White',
    email: 'carol.white@company.com',
    role: UserRole.Member,
    teamId: 1,
    teamName: 'Product Engineering',
    avatarColor: '#f59e0b' // amber
  },
  {
    id: 4,
    name: 'David Brown',
    email: 'david.brown@company.com',
    role: UserRole.Member,
    teamId: 1,
    teamName: 'Product Engineering',
    avatarColor: '#ef4444' // red
  },
  {
    id: 5,
    name: 'Eve Davis',
    email: 'eve.davis@company.com',
    role: UserRole.Member,
    teamId: 1,
    teamName: 'Product Engineering',
    avatarColor: '#8b5cf6' // violet
  }
];

const STORAGE_KEY = 'standupbot_current_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  // Private mutable state
  private readonly _currentUser = signal<User | null>(this.loadStoredUser());
  private readonly _availableUsers = signal<User[]>(SAMPLE_USERS);

  // Public readonly signals
  readonly currentUser = this._currentUser.asReadonly();
  readonly availableUsers = this._availableUsers.asReadonly();

  // Computed signals
  readonly isLoggedIn = computed(() => this._currentUser() !== null);
  readonly isTeamLead = computed(() => {
    const user = this._currentUser();
    return user ? isTeamLead(user) : false;
  });
  readonly userName = computed(() => this._currentUser()?.name ?? '');
  readonly userRole = computed(() => this._currentUser()?.role ?? null);

  constructor(private readonly router: Router) {}

  /**
   * Load user from localStorage on init.
   */
  private loadStoredUser(): User | null {
    try {
      const stored = localStorage.getItem(STORAGE_KEY);
      if (stored) {
        const userId = JSON.parse(stored) as number;
        return SAMPLE_USERS.find(u => u.id === userId) ?? null;
      }
    } catch {
      localStorage.removeItem(STORAGE_KEY);
    }
    return null;
  }

  /**
   * Select a user (simulated login).
   */
  login(user: User): void {
    this._currentUser.set(user);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(user.id));
    this.router.navigate(['/']);
  }

  /**
   * Clear current user (simulated logout).
   */
  logout(): void {
    this._currentUser.set(null);
    localStorage.removeItem(STORAGE_KEY);
    this.router.navigate(['/login']);
  }

  /**
   * Get a user by ID.
   */
  getUserById(id: number): User | undefined {
    return SAMPLE_USERS.find(u => u.id === id);
  }

  /**
   * Get all team members (excluding current user).
   */
  getTeamMembers(): User[] {
    const currentId = this._currentUser()?.id;
    return SAMPLE_USERS.filter(u => u.id !== currentId);
  }
}
