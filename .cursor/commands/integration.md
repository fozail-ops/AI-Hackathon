# Frontend-Backend Integration Command

## Description
Integrate Angular 20 frontend with .NET Core 10 backend APIs, ensuring proper data flow, error handling, and type safety.

## Tech Stack
- Angular 20 (Frontend)
- .NET Core 10 (Backend)
- Entity Framework Core
- TypeScript/C# type synchronization

## Instructions

When integrating frontend with backend, follow these patterns:

### 1. API Contract Definition

#### Backend DTO (C#)
```csharp
// Application/DTOs/{Entity}Dto.cs
namespace StandupBot.Application.DTOs;

public record {Entity}Dto(
    int Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record Create{Entity}Request(
    [Required] string Name,
    string? Description,
    bool IsActive = true
);

public record Update{Entity}Request(
    string? Name,
    string? Description,
    bool? IsActive
);
```

#### Frontend Model (TypeScript)
```typescript
// models/{entity}.model.ts
export interface {Entity} {
  id: number;
  name: string;
  description: string | null;
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date | null;
}

export interface Create{Entity}Request {
  name: string;
  description?: string;
  isActive?: boolean;
}

export interface Update{Entity}Request {
  name?: string;
  description?: string;
  isActive?: boolean;
}
```

### 2. CORS Configuration (Backend)

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Use CORS before routing
app.UseCors("AllowAngularApp");
```

### 3. API Response Wrapper Pattern

#### Backend Response Wrapper
```csharp
// Application/DTOs/ApiResponse.cs
namespace StandupBot.Application.DTOs;

public record ApiResponse<T>
{
    public bool Success { get; init; } = true;
    public T? Data { get; init; }
    public string? Message { get; init; }
    public List<string> Errors { get; init; } = [];
    
    public static ApiResponse<T> Ok(T data, string? message = null) 
        => new() { Data = data, Message = message };
    
    public static ApiResponse<T> Fail(string error) 
        => new() { Success = false, Errors = [error] };
    
    public static ApiResponse<T> Fail(List<string> errors) 
        => new() { Success = false, Errors = errors };
}

public record PaginatedResponse<T>
{
    public required IEnumerable<T> Items { get; init; }
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
```

#### Frontend Response Types
```typescript
// models/api-response.model.ts
export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  errors: string[];
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface PaginationParams {
  pageNumber: number;
  pageSize: number;
  sortBy?: string;
  sortDescending?: boolean;
}
```

### 4. Environment Configuration

#### Angular Environment
```typescript
// src/environments/environment.ts
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api'
};

// src/environments/environment.prod.ts
export const environment = {
  production: true,
  apiUrl: '/api'  // Relative for production
};
```

#### API Configuration Provider
```typescript
// core/config/api.config.ts
import { InjectionToken } from '@angular/core';
import { environment } from '../../../environments/environment';

export interface ApiConfig {
  baseUrl: string;
  timeout: number;
  retryAttempts: number;
}

export const API_CONFIG = new InjectionToken<ApiConfig>('API_CONFIG');

export const apiConfig: ApiConfig = {
  baseUrl: environment.apiUrl,
  timeout: 30000,
  retryAttempts: 3
};

// Provider in app.config.ts
{ provide: API_CONFIG, useValue: apiConfig }
```

### 5. Service Integration Pattern

```typescript
// features/{feature}/services/{entity}.service.ts
import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, map, tap, throwError, retry } from 'rxjs';
import { API_CONFIG } from '@core/config/api.config';
import { 
  {Entity}, 
  Create{Entity}Request, 
  Update{Entity}Request 
} from '../models/{entity}.model';
import { ApiResponse, PaginatedResponse, PaginationParams } from '@core/models/api-response.model';

@Injectable({ providedIn: 'root' })
export class {Entity}Service {
  private readonly http = inject(HttpClient);
  private readonly config = inject(API_CONFIG);
  
  private readonly endpoint = '{entities}';
  
  // Reactive state
  private readonly _items = signal<{Entity}[]>([]);
  private readonly _selectedItem = signal<{Entity} | null>(null);
  private readonly _loading = signal<boolean>(false);
  private readonly _error = signal<string | null>(null);
  
  readonly items = this._items.asReadonly();
  readonly selectedItem = this._selectedItem.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();
  
  private get baseUrl(): string {
    return `${this.config.baseUrl}/${this.endpoint}`;
  }
  
  getAll(params?: PaginationParams): Observable<{Entity}[]> {
    this._loading.set(true);
    this._error.set(null);
    
    let httpParams = new HttpParams();
    if (params) {
      httpParams = httpParams
        .set('pageNumber', params.pageNumber)
        .set('pageSize', params.pageSize);
      if (params.sortBy) {
        httpParams = httpParams.set('sortBy', params.sortBy);
        httpParams = httpParams.set('sortDescending', params.sortDescending ?? false);
      }
    }
    
    return this.http.get<{Entity}[]>(this.baseUrl, { params: httpParams }).pipe(
      retry(this.config.retryAttempts),
      tap(items => {
        this._items.set(items);
        this._loading.set(false);
      }),
      catchError(error => {
        this._error.set(error.message);
        this._loading.set(false);
        return throwError(() => error);
      })
    );
  }
  
  getPaginated(params: PaginationParams): Observable<PaginatedResponse<{Entity}>> {
    const httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber)
      .set('pageSize', params.pageSize);
    
    return this.http.get<PaginatedResponse<{Entity}>>(
      `${this.baseUrl}/paginated`, 
      { params: httpParams }
    );
  }
  
  getById(id: number): Observable<{Entity}> {
    return this.http.get<{Entity}>(`${this.baseUrl}/${id}`).pipe(
      tap(item => this._selectedItem.set(item)),
      catchError(this.handleError.bind(this))
    );
  }
  
  create(request: Create{Entity}Request): Observable<{Entity}> {
    return this.http.post<{Entity}>(this.baseUrl, request).pipe(
      tap(created => {
        this._items.update(items => [...items, created]);
      }),
      catchError(this.handleError.bind(this))
    );
  }
  
  update(id: number, request: Update{Entity}Request): Observable<{Entity}> {
    return this.http.put<{Entity}>(`${this.baseUrl}/${id}`, request).pipe(
      tap(updated => {
        this._items.update(items => 
          items.map(item => item.id === id ? updated : item)
        );
        if (this._selectedItem()?.id === id) {
          this._selectedItem.set(updated);
        }
      }),
      catchError(this.handleError.bind(this))
    );
  }
  
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`).pipe(
      tap(() => {
        this._items.update(items => items.filter(item => item.id !== id));
        if (this._selectedItem()?.id === id) {
          this._selectedItem.set(null);
        }
      }),
      catchError(this.handleError.bind(this))
    );
  }
  
  private handleError(error: any): Observable<never> {
    const errorMessage = error.error?.message || error.message || 'An unexpected error occurred';
    this._error.set(errorMessage);
    return throwError(() => new Error(errorMessage));
  }
}
```

### 6. HTTP Interceptors for Integration

#### Auth Token Interceptor
```typescript
// core/interceptors/auth.interceptor.ts
import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '@core/services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();
  
  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }
  
  return next(req);
};
```

#### Request/Response Logging Interceptor
```typescript
// core/interceptors/logging.interceptor.ts
import { HttpInterceptorFn, HttpResponse } from '@angular/common/http';
import { tap } from 'rxjs';

export const loggingInterceptor: HttpInterceptorFn = (req, next) => {
  const started = Date.now();
  
  return next(req).pipe(
    tap(event => {
      if (event instanceof HttpResponse) {
        const elapsed = Date.now() - started;
        console.log(`${req.method} ${req.url} - ${event.status} (${elapsed}ms)`);
      }
    })
  );
};
```

### 7. Date Handling

#### Backend Date Configuration
```csharp
// Program.cs
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
```

#### Frontend Date Transformation
```typescript
// core/interceptors/date-transform.interceptor.ts
import { HttpInterceptorFn, HttpResponse } from '@angular/common/http';
import { map } from 'rxjs';

const ISO_DATE_REGEX = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}/;

function transformDates(body: any): any {
  if (body === null || body === undefined) return body;
  
  if (typeof body === 'string' && ISO_DATE_REGEX.test(body)) {
    return new Date(body);
  }
  
  if (Array.isArray(body)) {
    return body.map(transformDates);
  }
  
  if (typeof body === 'object') {
    const transformed: any = {};
    for (const key of Object.keys(body)) {
      transformed[key] = transformDates(body[key]);
    }
    return transformed;
  }
  
  return body;
}

export const dateTransformInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    map(event => {
      if (event instanceof HttpResponse && event.body) {
        return event.clone({ body: transformDates(event.body) });
      }
      return event;
    })
  );
};
```

### 8. Error Handling Integration

#### Backend Problem Details
```csharp
// API/Middleware/ExceptionMiddleware.cs
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";
        
        var (statusCode, title) = exception switch
        {
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request"),
            _ => (StatusCodes.Status500InternalServerError, "An error occurred")
        };
        
        context.Response.StatusCode = statusCode;
        
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = context.Request.Path
        };
        
        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
```

#### Frontend Error Handler
```typescript
// core/interceptors/error.interceptor.ts
import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { NotificationService } from '@core/services/notification.service';

export interface ProblemDetails {
  status: number;
  title: string;
  detail: string;
  instance: string;
}

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const notifications = inject(NotificationService);
  
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const problemDetails = error.error as ProblemDetails;
      
      switch (error.status) {
        case 0:
          notifications.error('Unable to connect to server. Please check your connection.');
          break;
        case 401:
          notifications.error('Session expired. Please log in again.');
          router.navigate(['/login']);
          break;
        case 403:
          notifications.error('You do not have permission to perform this action.');
          break;
        case 404:
          notifications.error(problemDetails?.detail || 'Resource not found.');
          break;
        case 400:
          notifications.error(problemDetails?.detail || 'Invalid request.');
          break;
        case 500:
          notifications.error('Server error. Please try again later.');
          break;
        default:
          notifications.error(problemDetails?.detail || 'An unexpected error occurred.');
      }
      
      return throwError(() => error);
    })
  );
};
```

### 9. Integration Testing Setup

#### Backend Integration Test
```csharp
// Tests/Integration/{Entity}ControllerTests.cs
public class {Entity}ControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public {Entity}ControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsSuccessStatusCode()
    {
        var response = await _client.GetAsync("/api/{entities}");
        response.EnsureSuccessStatusCode();
    }
}
```

#### Frontend Integration Test
```typescript
// features/{feature}/services/{entity}.service.spec.ts
describe('{Entity}Service', () => {
  let service: {Entity}Service;
  let httpMock: HttpTestingController;
  
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_CONFIG, useValue: { baseUrl: 'http://test/api' } }
      ]
    });
    
    service = TestBed.inject({Entity}Service);
    httpMock = TestBed.inject(HttpTestingController);
  });
  
  it('should fetch all items', () => {
    const mockItems: {Entity}[] = [{ id: 1, name: 'Test' }];
    
    service.getAll().subscribe(items => {
      expect(items).toEqual(mockItems);
    });
    
    const req = httpMock.expectOne('http://test/api/{entities}');
    expect(req.request.method).toBe('GET');
    req.flush(mockItems);
  });
});
```

### 10. Proxy Configuration (Development)

```json
// proxy.conf.json
{
  "/api": {
    "target": "http://localhost:5000",
    "secure": false,
    "changeOrigin": true,
    "logLevel": "debug"
  }
}
```

```json
// angular.json - add to serve options
"serve": {
  "options": {
    "proxyConfig": "proxy.conf.json"
  }
}
```

## Integration Checklist
- [ ] Backend CORS configured for frontend origin
- [ ] DTOs match between C# and TypeScript
- [ ] API response wrapper implemented consistently
- [ ] Environment configuration for different stages
- [ ] HTTP interceptors for auth, errors, and logging
- [ ] Date serialization handling
- [ ] Error handling with Problem Details
- [ ] Proxy configuration for development
- [ ] Integration tests for both sides
- [ ] API versioning strategy defined

## Usage
When integrating features, specify:
1. Entity name and properties
2. Required API endpoints
3. Authentication requirements
4. Special data transformations needed
5. Error handling requirements
