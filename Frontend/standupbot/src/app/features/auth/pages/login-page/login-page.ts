import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

// Angular Material
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatRippleModule } from '@angular/material/core';
import { MatChipsModule } from '@angular/material/chips';

// Core
import { AuthService } from '../../../../core/services/auth.service';
import { User, UserRole, getUserInitials, UserRoleLabels } from '../../../../core/models/user.model';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatRippleModule,
    MatChipsModule
  ],
  templateUrl: './login-page.html',
  styleUrl: './login-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoginPageComponent {
  private readonly authService = inject(AuthService);

  // Signals
  readonly users = this.authService.availableUsers;
  readonly selectedUser = signal<User | null>(null);
  readonly isLoading = signal<boolean>(false);

  // Helper methods for template
  getUserInitials = getUserInitials;
  readonly UserRole = UserRole;
  readonly UserRoleLabels = UserRoleLabels;

  selectUser(user: User): void {
    this.selectedUser.set(user);
  }

  login(): void {
    const user = this.selectedUser();
    if (!user) return;

    this.isLoading.set(true);

    // Simulate a brief loading state for UX
    setTimeout(() => {
      this.authService.login(user);
      this.isLoading.set(false);
    }, 300);
  }

  isSelected(user: User): boolean {
    return this.selectedUser()?.id === user.id;
  }
}
