# Angular UI Component Generation

## Description
Generate Angular 20 UI components using Tailwind CSS v3 and Angular Material Design with clean architecture principles.

## Tech Stack
- Angular 20
- Tailwind CSS v3
- Angular Material Design
- TypeScript (strict mode)
- Signals for reactivity

## Instructions

When generating UI components, follow these patterns:

### 1. Component Structure
```
src/app/features/{feature}/
├── components/
│   ├── {component}/
│   │   ├── {component}.ts           # Component class
│   │   ├── {component}.html         # Template
│   │   ├── {component}.scss         # Styles (if needed)
│   │   └── {component}.spec.ts      # Tests
│   └── index.ts                     # Public API exports
├── pages/
│   └── {page}/                      # Smart/container components
├── models/
└── services/
```

### 2. Standalone Component Pattern (Angular 20)
```typescript
// src/app/features/{feature}/components/{component}/{component}.ts
import { Component, ChangeDetectionStrategy, input, output, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

// Angular Material imports
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-{component}',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatCardModule,
    MatIconModule
  ],
  templateUrl: './{component}.html',
  styleUrl: './{component}.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class {Component}Component {
  // Input signals (Angular 17.1+)
  readonly title = input.required<string>();
  readonly subtitle = input<string>('');
  readonly disabled = input<boolean>(false);
  
  // Output signals
  readonly clicked = output<void>();
  readonly valueChange = output<string>();
  
  // Internal signals
  private readonly _isLoading = signal<boolean>(false);
  readonly isLoading = this._isLoading.asReadonly();
  
  // Computed signals
  readonly displayTitle = computed(() => 
    this.title().toUpperCase()
  );
  
  readonly isDisabled = computed(() => 
    this.disabled() || this._isLoading()
  );
  
  // Methods
  onClick(): void {
    if (!this.isDisabled()) {
      this.clicked.emit();
    }
  }
  
  setLoading(value: boolean): void {
    this._isLoading.set(value);
  }
}
```

### 3. Template Pattern with Tailwind & Material
```html
<!-- src/app/features/{feature}/components/{component}/{component}.html -->

<!-- Card Component Example -->
<mat-card class="w-full max-w-md mx-auto shadow-lg hover:shadow-xl transition-shadow duration-300">
  <mat-card-header class="bg-gradient-to-r from-primary-500 to-primary-600 text-white rounded-t-lg">
    <mat-card-title class="text-lg font-semibold">
      {{ displayTitle() }}
    </mat-card-title>
    @if (subtitle()) {
      <mat-card-subtitle class="text-primary-100">
        {{ subtitle() }}
      </mat-card-subtitle>
    }
  </mat-card-header>
  
  <mat-card-content class="p-4 space-y-4">
    <ng-content></ng-content>
  </mat-card-content>
  
  <mat-card-actions class="flex justify-end gap-2 p-4 bg-gray-50 rounded-b-lg">
    <button 
      mat-stroked-button 
      color="primary"
      [disabled]="isDisabled()"
      class="hover:bg-primary-50 transition-colors">
      Cancel
    </button>
    <button 
      mat-flat-button 
      color="primary"
      [disabled]="isDisabled()"
      (click)="onClick()"
      class="shadow-sm hover:shadow transition-shadow">
      @if (isLoading()) {
        <mat-icon class="animate-spin mr-2">refresh</mat-icon>
      }
      Submit
    </button>
  </mat-card-actions>
</mat-card>
```

### 4. Form Component Pattern
```typescript
// src/app/features/{feature}/components/{entity}-form/{entity}-form.ts
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

import { {Entity}, Create{Entity}Request } from '../../models/{entity}.model';

@Component({
  selector: 'app-{entity}-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './{entity}-form.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class {Entity}FormComponent {
  private readonly fb = inject(FormBuilder);
  
  // Inputs
  readonly entity = input<{Entity} | null>(null);
  readonly isLoading = input<boolean>(false);
  
  // Outputs
  readonly formSubmit = output<Create{Entity}Request>();
  readonly formCancel = output<void>();
  
  // Form
  readonly form: FormGroup = this.fb.group({
    {propertyName}: ['', [Validators.required, Validators.maxLength(500)]],
    description: ['', [Validators.maxLength(2000)]],
    isActive: [true]
  });
  
  // Populate form when entity changes
  constructor() {
    effect(() => {
      const entity = this.entity();
      if (entity) {
        this.form.patchValue(entity);
      } else {
        this.form.reset({ isActive: true });
      }
    });
  }
  
  get isEditMode(): boolean {
    return this.entity() !== null;
  }
  
  onSubmit(): void {
    if (this.form.valid) {
      this.formSubmit.emit(this.form.value as Create{Entity}Request);
    } else {
      this.form.markAllAsTouched();
    }
  }
  
  onCancel(): void {
    this.formCancel.emit();
  }
  
  // Helper for template error messages
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
```

### 5. Form Template Pattern
```html
<!-- src/app/features/{feature}/components/{entity}-form/{entity}-form.html -->
<form [formGroup]="form" (ngSubmit)="onSubmit()" class="space-y-6">
  
  <!-- Text Input -->
  <mat-form-field appearance="outline" class="w-full">
    <mat-label>{Property Name}</mat-label>
    <input 
      matInput 
      formControlName="{propertyName}"
      placeholder="Enter {property name}"
      class="text-gray-900">
    @if (form.get('{propertyName}')?.invalid && form.get('{propertyName}')?.touched) {
      <mat-error>{{ getErrorMessage('{propertyName}') }}</mat-error>
    }
    <mat-hint>Brief description of the field</mat-hint>
  </mat-form-field>
  
  <!-- Textarea -->
  <mat-form-field appearance="outline" class="w-full">
    <mat-label>Description</mat-label>
    <textarea 
      matInput 
      formControlName="description"
      rows="4"
      placeholder="Enter description"
      class="resize-none"></textarea>
    <mat-hint align="end">
      {{ form.get('description')?.value?.length || 0 }} / 2000
    </mat-hint>
  </mat-form-field>
  
  <!-- Checkbox -->
  <div class="flex items-center">
    <mat-checkbox formControlName="isActive" color="primary">
      Active
    </mat-checkbox>
  </div>
  
  <!-- Action Buttons -->
  <div class="flex justify-end gap-3 pt-4 border-t border-gray-200">
    <button 
      type="button"
      mat-stroked-button
      (click)="onCancel()"
      [disabled]="isLoading()"
      class="min-w-[100px]">
      Cancel
    </button>
    <button 
      type="submit"
      mat-flat-button 
      color="primary"
      [disabled]="form.invalid || isLoading()"
      class="min-w-[100px]">
      @if (isLoading()) {
        <mat-spinner diameter="20" class="inline-block mr-2"></mat-spinner>
      }
      {{ isEditMode ? 'Update' : 'Create' }}
    </button>
  </div>
</form>
```

### 6. List/Table Component Pattern
```typescript
// src/app/features/{feature}/components/{entity}-list/{entity}-list.ts
import { Component, ChangeDetectionStrategy, input, output, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatMenuModule } from '@angular/material/menu';

import { {Entity} } from '../../models/{entity}.model';

@Component({
  selector: 'app-{entity}-list',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatMenuModule
  ],
  templateUrl: './{entity}-list.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class {Entity}ListComponent {
  // Inputs
  readonly items = input.required<{Entity}[]>();
  readonly isLoading = input<boolean>(false);
  
  // Outputs
  readonly itemSelected = output<{Entity}>();
  readonly itemEdit = output<{Entity}>();
  readonly itemDelete = output<{Entity}>();
  
  // Table columns
  readonly displayedColumns = ['id', '{propertyName}', 'createdAt', 'actions'];
  
  // Computed
  readonly hasItems = computed(() => this.items().length > 0);
  
  onRowClick(item: {Entity}): void {
    this.itemSelected.emit(item);
  }
  
  onEdit(event: Event, item: {Entity}): void {
    event.stopPropagation();
    this.itemEdit.emit(item);
  }
  
  onDelete(event: Event, item: {Entity}): void {
    event.stopPropagation();
    this.itemDelete.emit(item);
  }
}
```

### 7. List Template Pattern
```html
<!-- src/app/features/{feature}/components/{entity}-list/{entity}-list.html -->
<div class="bg-white rounded-lg shadow overflow-hidden">
  
  @if (isLoading()) {
    <div class="flex justify-center items-center py-12">
      <mat-spinner diameter="40"></mat-spinner>
    </div>
  } @else if (!hasItems()) {
    <!-- Empty State -->
    <div class="flex flex-col items-center justify-center py-12 text-gray-500">
      <mat-icon class="text-6xl mb-4 text-gray-300">inbox</mat-icon>
      <p class="text-lg font-medium">No items found</p>
      <p class="text-sm">Create your first item to get started</p>
    </div>
  } @else {
    <!-- Table -->
    <table mat-table [dataSource]="items()" class="w-full">
      
      <!-- ID Column -->
      <ng-container matColumnDef="id">
        <th mat-header-cell *matHeaderCellDef class="bg-gray-50 font-semibold">ID</th>
        <td mat-cell *matCellDef="let item" class="text-gray-600">{{ item.id }}</td>
      </ng-container>
      
      <!-- Property Column -->
      <ng-container matColumnDef="{propertyName}">
        <th mat-header-cell *matHeaderCellDef class="bg-gray-50 font-semibold">{Property Name}</th>
        <td mat-cell *matCellDef="let item" class="font-medium text-gray-900">
          {{ item.{propertyName} }}
        </td>
      </ng-container>
      
      <!-- Created At Column -->
      <ng-container matColumnDef="createdAt">
        <th mat-header-cell *matHeaderCellDef class="bg-gray-50 font-semibold">Created</th>
        <td mat-cell *matCellDef="let item" class="text-gray-500">
          {{ item.createdAt | date:'medium' }}
        </td>
      </ng-container>
      
      <!-- Actions Column -->
      <ng-container matColumnDef="actions">
        <th mat-header-cell *matHeaderCellDef class="bg-gray-50 w-16"></th>
        <td mat-cell *matCellDef="let item">
          <button mat-icon-button [matMenuTriggerFor]="menu" class="text-gray-400 hover:text-gray-600">
            <mat-icon>more_vert</mat-icon>
          </button>
          <mat-menu #menu="matMenu">
            <button mat-menu-item (click)="onEdit($event, item)">
              <mat-icon class="text-blue-500">edit</mat-icon>
              <span>Edit</span>
            </button>
            <button mat-menu-item (click)="onDelete($event, item)" class="text-red-500">
              <mat-icon>delete</mat-icon>
              <span>Delete</span>
            </button>
          </mat-menu>
        </td>
      </ng-container>
      
      <tr mat-header-row *matHeaderRowDef="displayedColumns" class="bg-gray-50"></tr>
      <tr mat-row 
          *matRowDef="let row; columns: displayedColumns" 
          (click)="onRowClick(row)"
          class="cursor-pointer hover:bg-gray-50 transition-colors"></tr>
    </table>
    
    <mat-paginator 
      [pageSizeOptions]="[5, 10, 25, 50]"
      showFirstLastButtons
      class="border-t border-gray-200">
    </mat-paginator>
  }
</div>
```

### 8. Page/Smart Component Pattern
```typescript
// src/app/features/{feature}/pages/{entity}-page/{entity}-page.ts
import { Component, ChangeDetectionStrategy, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

import { {Entity}Service } from '../../services/{entity}.service';
import { {Entity}ListComponent } from '../../components/{entity}-list/{entity}-list';
import { {Entity}FormDialogComponent } from '../../components/{entity}-form-dialog/{entity}-form-dialog';
import { {Entity} } from '../../models/{entity}.model';

@Component({
  selector: 'app-{entity}-page',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatSnackBarModule,
    MatButtonModule,
    MatIconModule,
    {Entity}ListComponent
  ],
  templateUrl: './{entity}-page.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class {Entity}PageComponent implements OnInit {
  private readonly {entity}Service = inject({Entity}Service);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly router = inject(Router);
  
  // Use service signals directly
  readonly items = this.{entity}Service.items;
  readonly loading = this.{entity}Service.loading;
  readonly error = this.{entity}Service.error;
  
  ngOnInit(): void {
    this.loadItems();
  }
  
  loadItems(): void {
    this.{entity}Service.getAll().subscribe();
  }
  
  openCreateDialog(): void {
    const dialogRef = this.dialog.open({Entity}FormDialogComponent, {
      width: '500px',
      data: { entity: null }
    });
    
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.showSuccess('Item created successfully');
      }
    });
  }
  
  onItemSelected(item: {Entity}): void {
    this.router.navigate(['/{entities}', item.id]);
  }
  
  onItemEdit(item: {Entity}): void {
    const dialogRef = this.dialog.open({Entity}FormDialogComponent, {
      width: '500px',
      data: { entity: item }
    });
    
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.showSuccess('Item updated successfully');
      }
    });
  }
  
  onItemDelete(item: {Entity}): void {
    if (confirm('Are you sure you want to delete this item?')) {
      this.{entity}Service.delete(item.id).subscribe({
        next: () => this.showSuccess('Item deleted successfully'),
        error: () => this.showError('Failed to delete item')
      });
    }
  }
  
  private showSuccess(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 3000,
      horizontalPosition: 'end',
      verticalPosition: 'top',
      panelClass: ['bg-green-500', 'text-white']
    });
  }
  
  private showError(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 5000,
      horizontalPosition: 'end',
      verticalPosition: 'top',
      panelClass: ['bg-red-500', 'text-white']
    });
  }
}
```

### 9. Tailwind Utilities for Material Design
```scss
// src/styles.scss - Custom Tailwind utilities
@layer utilities {
  .mat-elevation-hover {
    @apply transition-shadow duration-200 hover:shadow-lg;
  }
  
  .card-interactive {
    @apply cursor-pointer hover:bg-gray-50 transition-colors;
  }
  
  .badge-primary {
    @apply bg-primary-100 text-primary-800 px-2 py-0.5 rounded-full text-xs font-medium;
  }
  
  .badge-warning {
    @apply bg-amber-100 text-amber-800 px-2 py-0.5 rounded-full text-xs font-medium;
  }
  
  .badge-success {
    @apply bg-green-100 text-green-800 px-2 py-0.5 rounded-full text-xs font-medium;
  }
  
  .badge-danger {
    @apply bg-red-100 text-red-800 px-2 py-0.5 rounded-full text-xs font-medium;
  }
}
```

### 10. Common Component Patterns

#### Loading Skeleton
```html
<div class="animate-pulse space-y-4">
  <div class="h-4 bg-gray-200 rounded w-3/4"></div>
  <div class="h-4 bg-gray-200 rounded w-1/2"></div>
  <div class="h-32 bg-gray-200 rounded"></div>
</div>
```

#### Status Badge
```html
@switch (status) {
  @case ('active') {
    <span class="badge-success">Active</span>
  }
  @case ('pending') {
    <span class="badge-warning">Pending</span>
  }
  @case ('error') {
    <span class="badge-danger">Error</span>
  }
}
```

#### Card Grid Layout
```html
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
  @for (item of items(); track item.id) {
    <mat-card class="mat-elevation-hover">
      <!-- Card content -->
    </mat-card>
  }
</div>
```

## Best Practices Checklist
- [ ] Use standalone components (Angular 20)
- [ ] Use `input()` and `output()` signal-based APIs
- [ ] Apply `ChangeDetectionStrategy.OnPush`
- [ ] Use `inject()` function for dependency injection
- [ ] Combine Tailwind utilities with Material components
- [ ] Use Angular's new control flow (`@if`, `@for`, `@switch`)
- [ ] Track items in `@for` loops with unique identifiers
- [ ] Implement loading and empty states
- [ ] Use computed signals for derived state
- [ ] Keep templates clean and readable
- [ ] Follow accessibility best practices (ARIA labels, focus management)
- [ ] Use semantic HTML elements

## Usage
When asked to create a component, specify:
1. Component type (form, list, card, dialog, etc.)
2. Required inputs and outputs
3. Material Design components needed
4. Any specific styling requirements
5. Form validation rules (if applicable)
