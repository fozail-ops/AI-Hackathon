import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

// Angular Material
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';

// Core
import { AuthService } from '../../../core/services/auth.service';
import { getUserInitials, UserRole, UserRoleLabels } from '../../../core/models/user.model';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatDividerModule
  ],
  templateUrl: './header.html',
  styleUrl: './header.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HeaderComponent {
  private readonly authService = inject(AuthService);
  
  readonly currentUser = this.authService.currentUser;
  readonly isTeamLead = this.authService.isTeamLead;
  
  getUserInitials = getUserInitials;
  readonly UserRole = UserRole;
  readonly UserRoleLabels = UserRoleLabels;
  
  logout(): void {
    this.authService.logout();
  }
}
