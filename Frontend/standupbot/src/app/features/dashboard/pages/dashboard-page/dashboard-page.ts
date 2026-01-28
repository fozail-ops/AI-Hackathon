import { Component, ChangeDetectionStrategy, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

// Angular Material
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

// Core
import { AuthService } from '../../../../core/services/auth.service';
import { getUserInitials, UserRole, UserRoleLabels } from '../../../../core/models/user.model';

// Shared
import { HeaderComponent } from '../../../../shared/components/header/header';

// Feature services
import { StandupService } from '../../../standup/services/standup.service';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    HeaderComponent
  ],
  templateUrl: './dashboard-page.html',
  styleUrl: './dashboard-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardPageComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly standupService = inject(StandupService);
  private readonly router = inject(Router);

  readonly currentUser = this.authService.currentUser;
  readonly isTeamLead = this.authService.isTeamLead;
  readonly hasSubmittedToday = this.standupService.hasSubmittedToday;
  readonly loading = this.standupService.loading;

  getUserInitials = getUserInitials;
  readonly UserRole = UserRole;
  readonly UserRoleLabels = UserRoleLabels;
  
  ngOnInit(): void {
    this.checkSubmissionStatus();
  }
  
  private checkSubmissionStatus(): void {
    const user = this.currentUser();
    if (user) {
      this.standupService.checkSubmissionStatus(user.id).subscribe();
    }
  }

  navigateToStandup(): void {
    this.router.navigate(['/standup']);
  }
  
  navigateToHistory(): void {
    this.router.navigate(['/history']);
  }
  
  navigateToTeam(): void {
    this.router.navigate(['/team']);
  }
}
