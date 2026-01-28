# Clean Angular Code Command

## Description
Refactor and clean Angular 20 code to follow best practices, improve performance, and maintain code quality.

## Tech Stack
- Angular 20
- TypeScript (strict mode)
- RxJS
- Signals

## Instructions

When cleaning Angular code, apply these patterns and practices:

### 1. Component Modernization

#### Before (Legacy Pattern)
```typescript
@Component({
  selector: 'app-example',
  templateUrl: './example.component.html',
  styleUrls: ['./example.component.scss']
})
export class ExampleComponent implements OnInit, OnDestroy {
  @Input() title: string = '';
  @Output() clicked = new EventEmitter<void>();
  
  items: Item[] = [];
  loading = false;
  private destroy$ = new Subject<void>();
  
  constructor(private itemService: ItemService) {}
  
  ngOnInit() {
    this.itemService.getItems()
      .pipe(takeUntil(this.destroy$))
      .subscribe(items => this.items = items);
  }
  
  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
```

#### After (Angular 20 Pattern)
```typescript
@Component({
  selector: 'app-example',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './example.component.html',
  styleUrl: './example.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ExampleComponent {
  private readonly itemService = inject(ItemService);
  private readonly destroyRef = inject(DestroyRef);
  
  // Signal-based inputs
  readonly title = input<string>('');
  
  // Signal-based outputs
  readonly clicked = output<void>();
  
  // Internal signals
  readonly items = signal<Item[]>([]);
  readonly loading = signal<boolean>(false);
  
  constructor() {
    // Use effect for side effects
    effect(() => {
      this.loadItems();
    });
  }
  
  private loadItems(): void {
    this.loading.set(true);
    this.itemService.getItems()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: items => {
          this.items.set(items);
          this.loading.set(false);
        },
        error: () => this.loading.set(false)
      });
  }
}
```

### 2. Template Modernization

#### Before (Legacy Syntax)
```html
<div *ngIf="loading">Loading...</div>
<div *ngIf="!loading && items.length === 0">No items found</div>
<ul *ngIf="!loading && items.length > 0">
  <li *ngFor="let item of items; trackBy: trackById">
    {{ item.name }}
  </li>
</ul>

<div [ngSwitch]="status">
  <span *ngSwitchCase="'active'">Active</span>
  <span *ngSwitchCase="'pending'">Pending</span>
  <span *ngSwitchDefault>Unknown</span>
</div>
```

#### After (Angular 20 Control Flow)
```html
@if (loading()) {
  <div>Loading...</div>
} @else if (items().length === 0) {
  <div>No items found</div>
} @else {
  <ul>
    @for (item of items(); track item.id) {
      <li>{{ item.name }}</li>
    } @empty {
      <li>List is empty</li>
    }
  </ul>
}

@switch (status()) {
  @case ('active') {
    <span>Active</span>
  }
  @case ('pending') {
    <span>Pending</span>
  }
  @default {
    <span>Unknown</span>
  }
}
```

### 3. Dependency Injection Modernization

#### Before
```typescript
constructor(
  private readonly userService: UserService,
  private readonly router: Router,
  private readonly activatedRoute: ActivatedRoute,
  @Inject(API_CONFIG) private readonly config: ApiConfig
) {}
```

#### After
```typescript
private readonly userService = inject(UserService);
private readonly router = inject(Router);
private readonly activatedRoute = inject(ActivatedRoute);
private readonly config = inject(API_CONFIG);
```

### 4. Signal Conversion Patterns

#### Converting BehaviorSubject to Signal
```typescript
// Before
private itemsSubject = new BehaviorSubject<Item[]>([]);
items$ = this.itemsSubject.asObservable();

setItems(items: Item[]) {
  this.itemsSubject.next(items);
}

// After
private readonly _items = signal<Item[]>([]);
readonly items = this._items.asReadonly();

setItems(items: Item[]): void {
  this._items.set(items);
}
```

#### Converting Computed Properties
```typescript
// Before
get itemCount(): number {
  return this.items.length;
}

get hasItems(): boolean {
  return this.items.length > 0;
}

// After
readonly itemCount = computed(() => this.items().length);
readonly hasItems = computed(() => this.items().length > 0);
```

### 5. RxJS Best Practices

#### Avoid Nested Subscriptions
```typescript
// Before (Bad)
this.userService.getUser().subscribe(user => {
  this.orderService.getOrders(user.id).subscribe(orders => {
    this.items = orders;
  });
});

// After (Good)
this.userService.getUser().pipe(
  switchMap(user => this.orderService.getOrders(user.id)),
  takeUntilDestroyed()
).subscribe(orders => {
  this.items.set(orders);
});
```

#### Use Proper Error Handling
```typescript
// Before (Bad)
this.service.getData().subscribe(data => this.data = data);

// After (Good)
this.service.getData().pipe(
  takeUntilDestroyed(),
  catchError(error => {
    this.error.set(error.message);
    return EMPTY;
  })
).subscribe(data => this.data.set(data));
```

### 6. Clean Service Patterns

```typescript
// Clean service with signals
@Injectable({ providedIn: 'root' })
export class ItemService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(API_CONFIG);
  
  // Private mutable state
  private readonly _items = signal<Item[]>([]);
  private readonly _loading = signal<boolean>(false);
  private readonly _error = signal<string | null>(null);
  
  // Public readonly signals
  readonly items = this._items.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();
  
  // Computed signals
  readonly itemCount = computed(() => this._items().length);
  readonly activeItems = computed(() => 
    this._items().filter(item => item.isActive)
  );
  
  loadItems(): Observable<Item[]> {
    this._loading.set(true);
    this._error.set(null);
    
    return this.http.get<Item[]>(`${this.config.baseUrl}/items`).pipe(
      tap({
        next: items => {
          this._items.set(items);
          this._loading.set(false);
        },
        error: err => {
          this._error.set(err.message);
          this._loading.set(false);
        }
      })
    );
  }
  
  addItem(item: CreateItemRequest): Observable<Item> {
    return this.http.post<Item>(`${this.config.baseUrl}/items`, item).pipe(
      tap(created => this._items.update(items => [...items, created]))
    );
  }
  
  updateItem(id: number, updates: Partial<Item>): Observable<Item> {
    return this.http.patch<Item>(`${this.config.baseUrl}/items/${id}`, updates).pipe(
      tap(updated => 
        this._items.update(items => 
          items.map(item => item.id === id ? updated : item)
        )
      )
    );
  }
  
  deleteItem(id: number): Observable<void> {
    return this.http.delete<void>(`${this.config.baseUrl}/items/${id}`).pipe(
      tap(() => this._items.update(items => items.filter(item => item.id !== id)))
    );
  }
}
```

### 7. Form Cleanup

#### Before
```typescript
form = new FormGroup({
  name: new FormControl('', [Validators.required]),
  email: new FormControl('', [Validators.required, Validators.email]),
  age: new FormControl(null, [Validators.min(0), Validators.max(120)])
});

onSubmit() {
  if (this.form.valid) {
    const data = this.form.value;
    // process
  }
}
```

#### After (Typed Forms)
```typescript
interface UserForm {
  name: FormControl<string>;
  email: FormControl<string>;
  age: FormControl<number | null>;
}

private readonly fb = inject(NonNullableFormBuilder);

readonly form = this.fb.group<UserForm>({
  name: this.fb.control('', [Validators.required]),
  email: this.fb.control('', [Validators.required, Validators.email]),
  age: this.fb.control<number | null>(null, [Validators.min(0), Validators.max(120)])
});

onSubmit(): void {
  if (this.form.invalid) {
    this.form.markAllAsTouched();
    return;
  }
  
  const data = this.form.getRawValue();
  // process typed data
}
```

### 8. Route Configuration Cleanup

#### Before
```typescript
const routes: Routes = [
  { path: '', component: HomeComponent },
  { 
    path: 'items', 
    loadChildren: () => import('./items/items.module').then(m => m.ItemsModule)
  }
];
```

#### After (Standalone)
```typescript
export const routes: Routes = [
  { 
    path: '', 
    component: HomeComponent 
  },
  { 
    path: 'items',
    loadComponent: () => import('./items/items-page.component'),
    children: [
      {
        path: '',
        loadComponent: () => import('./items/item-list.component')
      },
      {
        path: ':id',
        loadComponent: () => import('./items/item-detail.component')
      }
    ]
  }
];
```

### 9. Pipe Optimization

#### Create Pure Computed Pipes
```typescript
// Instead of impure pipe, use computed in component
// Before (Pipe)
@Pipe({ name: 'filterActive', pure: false })
export class FilterActivePipe implements PipeTransform {
  transform(items: Item[]): Item[] {
    return items.filter(item => item.isActive);
  }
}

// After (Computed signal in component)
readonly activeItems = computed(() => 
  this.items().filter(item => item.isActive)
);
```

### 10. Performance Optimizations

#### Lazy Load Heavy Components
```typescript
// In template
@defer (on viewport) {
  <app-heavy-chart [data]="chartData()" />
} @placeholder {
  <div class="h-64 bg-gray-100 animate-pulse rounded-lg"></div>
} @loading (minimum 500ms) {
  <app-loading-spinner />
}
```

#### Virtual Scrolling for Large Lists
```typescript
import { ScrollingModule } from '@angular/cdk/scrolling';

@Component({
  imports: [ScrollingModule],
  template: `
    <cdk-virtual-scroll-viewport itemSize="50" class="h-96">
      <div *cdkVirtualFor="let item of items()" class="h-12">
        {{ item.name }}
      </div>
    </cdk-virtual-scroll-viewport>
  `
})
```

### 11. Code Organization

#### Barrel Exports
```typescript
// features/items/index.ts
export * from './components/item-list/item-list.component';
export * from './components/item-form/item-form.component';
export * from './services/item.service';
export * from './models/item.model';
```

#### Feature Structure
```
features/
└── items/
    ├── components/
    │   ├── item-list/
    │   ├── item-form/
    │   └── item-card/
    ├── pages/
    │   ├── items-page/
    │   └── item-detail-page/
    ├── services/
    │   └── item.service.ts
    ├── models/
    │   └── item.model.ts
    ├── guards/
    ├── resolvers/
    └── index.ts
```

## Cleanup Checklist

### Component Level
- [ ] Convert to standalone components
- [ ] Use signal-based inputs/outputs
- [ ] Apply `ChangeDetectionStrategy.OnPush`
- [ ] Use `inject()` function
- [ ] Replace `ngIf`, `ngFor`, `ngSwitch` with new control flow
- [ ] Use `takeUntilDestroyed()` for subscriptions
- [ ] Convert class properties to signals where appropriate

### Template Level
- [ ] Use new control flow syntax (`@if`, `@for`, `@switch`)
- [ ] Add `track` to all `@for` loops
- [ ] Use `@defer` for heavy components
- [ ] Remove unnecessary async pipes (use signals)

### Service Level
- [ ] Use signals for state management
- [ ] Implement proper error handling
- [ ] Use `inject()` function
- [ ] Provide in root for tree-shaking

### RxJS Level
- [ ] Remove nested subscriptions
- [ ] Use proper operators (switchMap, mergeMap, etc.)
- [ ] Implement error handling
- [ ] Use `takeUntilDestroyed()`

### Forms Level
- [ ] Use typed reactive forms
- [ ] Use `NonNullableFormBuilder`
- [ ] Implement proper validation

### General
- [ ] Remove unused imports
- [ ] Remove dead code
- [ ] Fix ESLint/TSLint warnings
- [ ] Update to latest Angular patterns
- [ ] Ensure strict TypeScript compliance

## Usage
When cleaning Angular code, specify:
1. Component/service to clean
2. Specific patterns to apply
3. Performance concerns
4. Migration requirements (signals, standalone, etc.)
