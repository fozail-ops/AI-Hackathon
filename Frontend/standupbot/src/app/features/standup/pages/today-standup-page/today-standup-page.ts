import { Component, ChangeDetectionStrategy, inject, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

// Angular Material
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

// Core
import { AuthService } from '../../../../core/services/auth.service';

// Feature
import { StandupService } from '../../services/standup.service';
import { StandupFormComponent } from '../../components/standup-form/standup-form';
import { CreateStandupRequest } from '../../models/standup.model';

// Shared
import { HeaderComponent } from '../../../../shared/components/header/header';

@Component({
  selector: 'app-today-standup-page',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    StandupFormComponent,
    HeaderComponent
  ],
  templateUrl: './today-standup-page.html',
  styleUrl: './today-standup-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TodayStandupPageComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly standupService = inject(StandupService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly router = inject(Router);
  
  // From services
  readonly currentUser = this.authService.currentUser;
  readonly todayStandup = this.standupService.todayStandup;
  readonly loading = this.standupService.loading;
  readonly error = this.standupService.error;
  readonly isEditMode = this.standupService.isEditMode;
  
  // Computed
  readonly today = computed(() => new Date().toLocaleDateString('en-US', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  }));
  
  ngOnInit(): void {
    this.loadTodayStandup();
  }
  
  private loadTodayStandup(): void {
    const user = this.currentUser();
    if (user) {
      this.standupService.getTodayStandup(user.id).subscribe({
        error: () => {
          // 404 is expected if no standup yet - handled in service
        }
      });
    }
  }
  
  onFormSubmit(request: CreateStandupRequest): void {
    const user = this.currentUser();
    if (!user) return;
    
    const isEdit = this.isEditMode();
    
    const operation = isEdit 
      ? this.standupService.update(user.id, request)
      : this.standupService.create(user.id, request);
      
    operation.subscribe({
      next: () => {
        this.showSuccess(isEdit ? 'Standup updated successfully!' : 'Standup submitted successfully!');
      },
      error: (err) => {
        // Error is already set in service
      }
    });
  }
  
  onFormCancel(): void {
    this.router.navigate(['/']);
  }
  
  private showSuccess(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 3000,
      horizontalPosition: 'end',
      verticalPosition: 'top',
      panelClass: ['bg-green-500', 'text-white']
    });
  }
}
