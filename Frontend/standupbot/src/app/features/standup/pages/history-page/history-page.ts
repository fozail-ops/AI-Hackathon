import { Component, ChangeDetectionStrategy, inject, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';

// Angular Material
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';

// Core
import { AuthService } from '../../../../core/services/auth.service';

// Feature
import { StandupService } from '../../services/standup.service';
import { StandupSummary, BlockerStatus } from '../../models/standup.model';

// Shared
import { HeaderComponent } from '../../../../shared/components/header/header';

@Component({
  selector: 'app-history-page',
  standalone: true,
  imports: [
    CommonModule,
    DatePipe,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    HeaderComponent
  ],
  templateUrl: './history-page.html',
  styleUrl: './history-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HistoryPageComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly standupService = inject(StandupService);
  
  readonly currentUser = this.authService.currentUser;
  readonly history = this.standupService.history;
  readonly loading = this.standupService.loading;
  
  readonly BlockerStatus = BlockerStatus;
  
  ngOnInit(): void {
    this.loadHistory();
  }
  
  private loadHistory(): void {
    const user = this.currentUser();
    if (user) {
      this.standupService.getHistory(user.id, 10).subscribe();
    }
  }
  
  getProgressColor(percentage: number): string {
    if (percentage >= 80) return 'bg-green-500';
    if (percentage >= 50) return 'bg-amber-500';
    return 'bg-indigo-500';
  }
}
