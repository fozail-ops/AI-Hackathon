import { Component, ChangeDetectionStrategy, input, output, inject, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

// Angular Material
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatSliderModule } from '@angular/material/slider';

import { Standup, CreateStandupRequest, PERCENTAGE_OPTIONS } from '../../models/standup.model';

@Component({
  selector: 'app-standup-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatSliderModule
  ],
  templateUrl: './standup-form.html',
  styleUrl: './standup-form.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class StandupFormComponent {
  private readonly fb = inject(FormBuilder);
  
  // Inputs
  readonly standup = input<Standup | null>(null);
  readonly isLoading = input<boolean>(false);
  readonly errorMessage = input<string | null>(null);
  
  // Outputs
  readonly formSubmit = output<CreateStandupRequest>();
  readonly formCancel = output<void>();
  
  // Percentage options
  readonly percentageOptions = PERCENTAGE_OPTIONS;
  
  // Form
  readonly form: FormGroup = this.fb.group({
    jiraId: ['', [Validators.required, Validators.maxLength(50)]],
    taskDescription: ['', [Validators.required, Validators.maxLength(500)]],
    percentageComplete: [0, [Validators.required, Validators.min(0), Validators.max(100)]],
    hasBlocker: [false],
    blockerDescription: ['', [Validators.maxLength(1000)]],
    nextTask: ['', [Validators.required, Validators.maxLength(500)]]
  });
  
  // Populate form when standup changes
  constructor() {
    effect(() => {
      const standup = this.standup();
      if (standup) {
        this.form.patchValue({
          jiraId: standup.jiraId,
          taskDescription: standup.taskDescription,
          percentageComplete: standup.percentageComplete,
          hasBlocker: standup.hasBlocker,
          blockerDescription: standup.blockerDescription ?? '',
          nextTask: standup.nextTask
        });
      } else {
        this.form.reset({
          percentageComplete: 0,
          hasBlocker: false
        });
      }
    });
    
    // Watch hasBlocker to validate blocker description
    this.form.get('hasBlocker')?.valueChanges.subscribe(hasBlocker => {
      const blockerControl = this.form.get('blockerDescription');
      if (hasBlocker) {
        blockerControl?.setValidators([Validators.required, Validators.maxLength(1000)]);
      } else {
        blockerControl?.setValidators([Validators.maxLength(1000)]);
        blockerControl?.setValue('');
      }
      blockerControl?.updateValueAndValidity();
    });
  }
  
  get isEditMode(): boolean {
    return this.standup() !== null;
  }
  
  get hasBlocker(): boolean {
    return this.form.get('hasBlocker')?.value ?? false;
  }
  
  onSubmit(): void {
    if (this.form.valid) {
      const value = this.form.value;
      const request: CreateStandupRequest = {
        jiraId: value.jiraId,
        taskDescription: value.taskDescription,
        percentageComplete: value.percentageComplete,
        hasBlocker: value.hasBlocker,
        blockerDescription: value.hasBlocker ? value.blockerDescription : undefined,
        nextTask: value.nextTask
      };
      this.formSubmit.emit(request);
    } else {
      this.form.markAllAsTouched();
    }
  }
  
  onCancel(): void {
    this.formCancel.emit();
  }
  
  getErrorMessage(controlName: string): string {
    const control = this.form.get(controlName);
    if (control?.hasError('required')) {
      return 'This field is required';
    }
    if (control?.hasError('maxlength')) {
      const maxLength = control.errors?.['maxlength'].requiredLength;
      return `Maximum ${maxLength} characters allowed`;
    }
    return '';
  }
}
