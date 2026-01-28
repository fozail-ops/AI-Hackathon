# Frontend Angular Service Generation

## Description
Generate Angular 20 services to connect with backend .NET Core APIs following best practices and clean architecture.

## Tech Stack
- Angular 20
- RxJS
- TypeScript (strict mode)
- HttpClient with interceptors

## Instructions

When generating Angular services, follow these patterns:

### 1. Project Structure
```
src/app/
├── core/
│   ├── services/           # Singleton services (API, Auth, etc.)
│   ├── interceptors/       # HTTP interceptors
│   ├── guards/             # Route guards
│   └── models/             # Core interfaces/types
├── shared/
│   ├── services/           # Shared utility services
│   └── utils/              # Helper functions
└── features/
    └── {feature}/
        ├── services/       # Feature-specific services
        ├── models/         # Feature-specific interfaces
        └── components/     # Feature components
```

### 2. API Configuration
```typescript
// src/app/core/config/api.config.ts
import { InjectionToken } from '@angular/core';

export interface ApiConfig {
  baseUrl: string;
  timeout: number;
}

export const API_CONFIG = new InjectionToken<ApiConfig>('API_CONFIG');

export const apiConfig: ApiConfig = {
  baseUrl: 'http://localhost:5000/api',
  timeout: 30000
};
```

### 3. Base API Service Pattern
```typescript
// src/app/core/services/base-api.service.ts
import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, retry } from 'rxjs/operators';
import { API_CONFIG } from '../config/api.config';

@Injectable({ providedIn: 'root' })
export abstract class BaseApiService {
  protected readonly http = inject(HttpClient);
  protected readonly config = inject(API_CONFIG);
  
  protected abstract readonly endpoint: string;
  
  protected get baseUrl(): string {
    return `${this.config.baseUrl}/${this.endpoint}`;
  }
  
  protected handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An unexpected error occurred';
    
    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = error.error.message;
    } else {
      // Server-side error
      errorMessage = error.error?.message || error.message || errorMessage;
    }
    
    console.error('API Error:', error);
    return throwError(() => new Error(errorMessage));
  }
  
  protected buildParams(params: Record<string, string | number | boolean | undefined>): HttpParams {
    let httpParams = new HttpParams();
    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null) {
        httpParams = httpParams.set(key, String(value));
      }
    });
    return httpParams;
  }
}
```

### 4. Entity Service Pattern
```typescript
// src/app/features/{feature}/services/{entity}.service.ts
import { Injectable, inject, signal, computed } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { BaseApiService } from '@core/services/base-api.service';
import { {Entity}, Create{Entity}Request, Update{Entity}Request } from '../models/{entity}.model';

@Injectable({ providedIn: 'root' })
export class {Entity}Service extends BaseApiService {
  protected override readonly endpoint = '{entities}';
  
  // Signals for reactive state management (Angular 20)
  private readonly _items = signal<{Entity}[]>([]);
  private readonly _loading = signal<boolean>(false);
  private readonly _error = signal<string | null>(null);
  
  // Public readonly signals
  readonly items = this._items.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();
  
  // Computed signals
  readonly itemCount = computed(() => this._items().length);
  readonly hasItems = computed(() => this._items().length > 0);
  
  getAll(): Observable<{Entity}[]> {
    this._loading.set(true);
    this._error.set(null);
    
    return this.http.get<{Entity}[]>(this.baseUrl).pipe(
      tap({
        next: (items) => {
          this._items.set(items);
          this._loading.set(false);
        },
        error: (err) => {
          this._error.set(err.message);
          this._loading.set(false);
        }
      }),
      catchError(this.handleError.bind(this))
    );
  }
  
  getById(id: number): Observable<{Entity}> {
    return this.http.get<{Entity}>(`${this.baseUrl}/${id}`).pipe(
      catchError(this.handleError.bind(this))
    );
  }
  
  create(request: Create{Entity}Request): Observable<{Entity}> {
    return this.http.post<{Entity}>(this.baseUrl, request).pipe(
      tap((created) => {
        this._items.update(items => [...items, created]);
      }),
      catchError(this.handleError.bind(this))
    );
  }
  
  update(id: number, request: Update{Entity}Request): Observable<{Entity}> {
    return this.http.put<{Entity}>(`${this.baseUrl}/${id}`, request).pipe(
      tap((updated) => {
        this._items.update(items => 
          items.map(item => item.id === id ? updated : item)
        );
      }),
      catchError(this.handleError.bind(this))
    );
  }
  
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`).pipe(
      tap(() => {
        this._items.update(items => items.filter(item => item.id !== id));
      }),
      catchError(this.handleError.bind(this))
    );
  }
}
```

### 5. Model/Interface Pattern
```typescript
// src/app/features/{feature}/models/{entity}.model.ts
export interface {Entity} {
  id: number;
  {propertyName}: {type};
  createdAt: Date;
  updatedAt?: Date;
}

export interface Create{Entity}Request {
  {propertyName}: {type};
}

export interface Update{Entity}Request {
  {propertyName}: {type};
}

// Optional: API response wrapper
export interface ApiResponse<T> {
  data: T;
  message?: string;
  success: boolean;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}
```

### 6. HTTP Interceptor Pattern
```typescript
// src/app/core/interceptors/http-error.interceptor.ts
import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { NotificationService } from '@core/services/notification.service';

export const httpErrorInterceptor: HttpInterceptorFn = (req, next) => {
  const notificationService = inject(NotificationService);
  
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'An unexpected error occurred';
      
      switch (error.status) {
        case 0:
          errorMessage = 'Unable to connect to server';
          break;
        case 400:
          errorMessage = error.error?.message || 'Invalid request';
          break;
        case 401:
          errorMessage = 'Please log in to continue';
          break;
        case 403:
          errorMessage = 'You do not have permission to perform this action';
          break;
        case 404:
          errorMessage = 'Resource not found';
          break;
        case 500:
          errorMessage = 'Server error. Please try again later';
          break;
      }
      
      notificationService.showError(errorMessage);
      return throwError(() => error);
    })
  );
};
```

```typescript
// src/app/core/interceptors/loading.interceptor.ts
import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs';
import { LoadingService } from '@core/services/loading.service';

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const loadingService = inject(LoadingService);
  
  loadingService.show();
  
  return next(req).pipe(
    finalize(() => loadingService.hide())
  );
};
```

### 7. App Configuration
```typescript
// src/app/app.config.ts
import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';

import { routes } from './app.routes';
import { API_CONFIG, apiConfig } from '@core/config/api.config';
import { httpErrorInterceptor } from '@core/interceptors/http-error.interceptor';
import { loadingInterceptor } from '@core/interceptors/loading.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([
        loadingInterceptor,
        httpErrorInterceptor
      ])
    ),
    provideAnimationsAsync(),
    { provide: API_CONFIG, useValue: apiConfig }
  ]
};
```

### 8. Feature Module Service with Caching
```typescript
// src/app/features/{feature}/services/{entity}-cache.service.ts
import { Injectable, inject, signal, computed } from '@angular/core';
import { Observable, of, tap, shareReplay, timer, switchMap } from 'rxjs';
import { {Entity}Service } from './{entity}.service';
import { {Entity} } from '../models/{entity}.model';

const CACHE_DURATION_MS = 5 * 60 * 1000; // 5 minutes

@Injectable({ providedIn: 'root' })
export class {Entity}CacheService {
  private readonly {entity}Service = inject({Entity}Service);
  
  private cache$: Observable<{Entity}[]> | null = null;
  private lastFetchTime = 0;
  
  getAll(forceRefresh = false): Observable<{Entity}[]> {
    const now = Date.now();
    const cacheExpired = now - this.lastFetchTime > CACHE_DURATION_MS;
    
    if (!this.cache$ || forceRefresh || cacheExpired) {
      this.cache$ = this.{entity}Service.getAll().pipe(
        tap(() => this.lastFetchTime = Date.now()),
        shareReplay(1)
      );
    }
    
    return this.cache$;
  }
  
  invalidateCache(): void {
    this.cache$ = null;
    this.lastFetchTime = 0;
  }
}
```

### 9. Resource Service Pattern (Angular 20+)
```typescript
// Using Angular's new resource API (Angular 19+)
import { Injectable, inject, resource, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API_CONFIG } from '@core/config/api.config';
import { {Entity} } from '../models/{entity}.model';

@Injectable({ providedIn: 'root' })
export class {Entity}ResourceService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(API_CONFIG);
  
  private readonly endpoint = '{entities}';
  private readonly selectedId = signal<number | null>(null);
  
  // Resource for fetching all items
  readonly allItems = resource({
    loader: () => this.http.get<{Entity}[]>(
      `${this.config.baseUrl}/${this.endpoint}`
    ).toPromise()
  });
  
  // Resource for fetching single item by ID
  readonly selectedItem = resource({
    request: () => this.selectedId(),
    loader: ({ request: id }) => {
      if (!id) return Promise.resolve(null);
      return this.http.get<{Entity}>(
        `${this.config.baseUrl}/${this.endpoint}/${id}`
      ).toPromise();
    }
  });
  
  selectItem(id: number | null): void {
    this.selectedId.set(id);
  }
  
  refresh(): void {
    this.allItems.reload();
  }
}
```

## Best Practices Checklist
- [ ] Use signals for reactive state management (Angular 20)
- [ ] Implement proper error handling with user-friendly messages
- [ ] Use TypeScript strict mode with proper typing
- [ ] Extend BaseApiService for consistent API calls
- [ ] Use functional interceptors (Angular 15+)
- [ ] Implement caching where appropriate
- [ ] Use `inject()` function over constructor injection
- [ ] Provide services in root for tree-shaking
- [ ] Use `readonly` for signal exposure
- [ ] Handle loading and error states
- [ ] Use `catchError` for RxJS error handling
- [ ] Implement proper unsubscription (use `takeUntilDestroyed()`)

## Usage
When asked to create a service, specify:
1. Entity/feature name
2. API endpoints it should connect to
3. Required CRUD operations
4. Any caching requirements
5. Special error handling needs
