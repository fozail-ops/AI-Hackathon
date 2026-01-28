# Models Generation Command

## Description
Generate synchronized models/entities for both .NET Core 10 backend and Angular 20 frontend with proper typing, validation, and mapping.

## Tech Stack
- .NET Core 10 (C# 13)
- Entity Framework Core
- Angular 20 (TypeScript)
- AutoMapper

## Instructions

When generating models, create synchronized definitions for both backend and frontend.

---

## Backend Models (.NET Core)

### 1. Domain Entity Pattern
```csharp
// Domain/Entities/{Entity}.cs
namespace StandupBot.Domain.Entities;

/// <summary>
/// Represents a {Entity} in the system.
/// </summary>
public class {Entity} : BaseEntity
{
    // Required properties with 'required' modifier
    public required string Name { get; set; }
    
    // Optional properties (nullable)
    public string? Description { get; set; }
    
    // Value types with defaults
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    
    // Enum properties
    public {Entity}Status Status { get; set; } = {Entity}Status.Active;
    
    // Foreign keys
    public int {RelatedEntity}Id { get; set; }
    
    // Navigation properties (use virtual for lazy loading, init for immutability)
    public virtual {RelatedEntity}? {RelatedEntity} { get; set; }
    public virtual ICollection<{ChildEntity}> {ChildEntities} { get; init; } = [];
}
```

### 2. Base Entity Pattern
```csharp
// Domain/Entities/BaseEntity.cs
namespace StandupBot.Domain.Entities;

public abstract class BaseEntity
{
    public int Id { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; init; }
    public string? UpdatedBy { get; set; }
}

// For soft delete support
public abstract class SoftDeleteEntity : BaseEntity
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
```

### 3. Enum Pattern
```csharp
// Domain/Enums/{Entity}Status.cs
namespace StandupBot.Domain.Enums;

public enum {Entity}Status
{
    Draft = 0,
    Active = 1,
    Inactive = 2,
    Archived = 3
}

// With display attributes for UI
public enum BlockerStatus
{
    [Display(Name = "New", Description = "Newly reported blocker")]
    New = 0,
    
    [Display(Name = "Critical", Description = "Marked as critical by team lead")]
    Critical = 1,
    
    [Display(Name = "Resolved", Description = "Blocker has been resolved")]
    Resolved = 2
}
```

### 4. DTO Patterns
```csharp
// Application/DTOs/{Entity}Dto.cs
namespace StandupBot.Application.DTOs;

/// <summary>
/// Data transfer object for {Entity}.
/// </summary>
public record {Entity}Dto(
    int Id,
    string Name,
    string? Description,
    bool IsActive,
    {Entity}Status Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    // Nested DTOs for related entities
    {RelatedEntity}Dto? {RelatedEntity}
);

/// <summary>
/// Request model for creating a new {Entity}.
/// </summary>
public record Create{Entity}Request(
    [property: Required]
    [property: StringLength(200, MinimumLength = 1)]
    string Name,
    
    [property: StringLength(2000)]
    string? Description,
    
    bool IsActive = true,
    
    [property: Required]
    int {RelatedEntity}Id
);

/// <summary>
/// Request model for updating an existing {Entity}.
/// </summary>
public record Update{Entity}Request(
    [property: StringLength(200, MinimumLength = 1)]
    string? Name,
    
    [property: StringLength(2000)]
    string? Description,
    
    bool? IsActive,
    
    {Entity}Status? Status
);

/// <summary>
/// Summary DTO for list views (lighter than full DTO).
/// </summary>
public record {Entity}SummaryDto(
    int Id,
    string Name,
    bool IsActive,
    {Entity}Status Status,
    DateTime CreatedAt
);
```

### 5. Value Objects Pattern
```csharp
// Domain/ValueObjects/DateRange.cs
namespace StandupBot.Domain.ValueObjects;

public record DateRange
{
    public DateTime Start { get; init; }
    public DateTime End { get; init; }
    
    public DateRange(DateTime start, DateTime end)
    {
        if (end < start)
            throw new ArgumentException("End date must be after start date");
            
        Start = start;
        End = end;
    }
    
    public bool Contains(DateTime date) => date >= Start && date <= End;
    public bool Overlaps(DateRange other) => Start < other.End && End > other.Start;
}

// Domain/ValueObjects/Percentage.cs
public record Percentage
{
    public int Value { get; init; }
    
    public Percentage(int value)
    {
        if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException(nameof(value), "Must be between 0 and 100");
        Value = value;
    }
    
    public static implicit operator int(Percentage p) => p.Value;
    public static explicit operator Percentage(int v) => new(v);
}
```

### 6. EF Core Configuration
```csharp
// Infrastructure/Data/Configurations/{Entity}Configuration.cs
namespace StandupBot.Infrastructure.Data.Configurations;

public class {Entity}Configuration : IEntityTypeConfiguration<{Entity}>
{
    public void Configure(EntityTypeBuilder<{Entity}> builder)
    {
        builder.ToTable("{Entity}s");
        
        // Primary key
        builder.HasKey(e => e.Id);
        
        // Properties
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(e => e.Description)
            .HasMaxLength(2000);
            
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(50);
            
        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
        
        // Indexes
        builder.HasIndex(e => e.Name);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => new { e.{RelatedEntity}Id, e.CreatedAt });
        
        // Relationships
        builder.HasOne(e => e.{RelatedEntity})
            .WithMany(r => r.{Entities})
            .HasForeignKey(e => e.{RelatedEntity}Id)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(e => e.{ChildEntities})
            .WithOne(c => c.{Entity})
            .HasForeignKey(c => c.{Entity}Id)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Query filters (soft delete)
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
```

### 7. AutoMapper Profile
```csharp
// Application/Mappings/{Entity}MappingProfile.cs
namespace StandupBot.Application.Mappings;

public class {Entity}MappingProfile : Profile
{
    public {Entity}MappingProfile()
    {
        // Entity to DTO
        CreateMap<{Entity}, {Entity}Dto>();
        CreateMap<{Entity}, {Entity}SummaryDto>();
        
        // Create request to Entity
        CreateMap<Create{Entity}Request, {Entity}>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        
        // Update request to Entity (only non-null values)
        CreateMap<Update{Entity}Request, {Entity}>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
```

---

## Frontend Models (Angular/TypeScript)

### 1. Interface Pattern
```typescript
// src/app/features/{feature}/models/{entity}.model.ts

/**
 * Represents a {Entity} in the system.
 */
export interface {Entity} {
  id: number;
  name: string;
  description: string | null;
  isActive: boolean;
  status: {Entity}Status;
  createdAt: Date;
  updatedAt: Date | null;
  
  // Related entities
  {relatedEntity}Id: number;
  {relatedEntity}?: {RelatedEntity};
  {childEntities}?: {ChildEntity}[];
}

/**
 * Summary model for list views.
 */
export interface {Entity}Summary {
  id: number;
  name: string;
  isActive: boolean;
  status: {Entity}Status;
  createdAt: Date;
}
```

### 2. Enum Pattern
```typescript
// src/app/features/{feature}/models/{entity}-status.enum.ts

export enum {Entity}Status {
  Draft = 'Draft',
  Active = 'Active',
  Inactive = 'Inactive',
  Archived = 'Archived'
}

// With display labels
export const {Entity}StatusLabels: Record<{Entity}Status, string> = {
  [{Entity}Status.Draft]: 'Draft',
  [{Entity}Status.Active]: 'Active',
  [{Entity}Status.Inactive]: 'Inactive',
  [{Entity}Status.Archived]: 'Archived'
};

// Helper function
export function get{Entity}StatusLabel(status: {Entity}Status): string {
  return {Entity}StatusLabels[status] ?? 'Unknown';
}

// For dropdowns/selects
export const {Entity}StatusOptions = Object.entries({Entity}StatusLabels).map(
  ([value, label]) => ({ value: value as {Entity}Status, label })
);
```

### 3. Request Models Pattern
```typescript
// src/app/features/{feature}/models/{entity}-request.model.ts

/**
 * Request model for creating a new {Entity}.
 */
export interface Create{Entity}Request {
  name: string;
  description?: string;
  isActive?: boolean;
  {relatedEntity}Id: number;
}

/**
 * Request model for updating an existing {Entity}.
 */
export interface Update{Entity}Request {
  name?: string;
  description?: string;
  isActive?: boolean;
  status?: {Entity}Status;
}

/**
 * Query parameters for filtering {Entity} list.
 */
export interface {Entity}QueryParams {
  search?: string;
  status?: {Entity}Status;
  {relatedEntity}Id?: number;
  isActive?: boolean;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDescending?: boolean;
}
```

### 4. API Response Models
```typescript
// src/app/core/models/api-response.model.ts

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

export interface ValidationError {
  field: string;
  message: string;
}
```

### 5. Form Models Pattern
```typescript
// src/app/features/{feature}/models/{entity}-form.model.ts
import { FormControl } from '@angular/forms';

/**
 * Typed form model for {Entity} form.
 */
export interface {Entity}FormModel {
  name: FormControl<string>;
  description: FormControl<string | null>;
  isActive: FormControl<boolean>;
  status: FormControl<{Entity}Status>;
  {relatedEntity}Id: FormControl<number | null>;
}

/**
 * Form value type (for submission).
 */
export type {Entity}FormValue = {
  [K in keyof {Entity}FormModel]: {Entity}FormModel[K]['value'];
};
```

### 6. Type Guards
```typescript
// src/app/features/{feature}/models/{entity}.guards.ts

import { {Entity}, {Entity}Summary } from './{entity}.model';
import { {Entity}Status } from './{entity}-status.enum';

/**
 * Type guard to check if value is a valid {Entity}.
 */
export function is{Entity}(value: unknown): value is {Entity} {
  return (
    typeof value === 'object' &&
    value !== null &&
    'id' in value &&
    'name' in value &&
    typeof (value as {Entity}).id === 'number' &&
    typeof (value as {Entity}).name === 'string'
  );
}

/**
 * Type guard for {Entity}Status.
 */
export function is{Entity}Status(value: unknown): value is {Entity}Status {
  return Object.values({Entity}Status).includes(value as {Entity}Status);
}

/**
 * Check if entity is in active state.
 */
export function isActive{Entity}(entity: {Entity}): boolean {
  return entity.isActive && entity.status === {Entity}Status.Active;
}
```

### 7. Mapper Utilities
```typescript
// src/app/features/{feature}/utils/{entity}.mapper.ts

import { {Entity}, {Entity}Summary } from '../models/{entity}.model';
import { Create{Entity}Request } from '../models/{entity}-request.model';

/**
 * Maps API response to {Entity} with proper date parsing.
 */
export function map{Entity}(data: any): {Entity} {
  return {
    ...data,
    createdAt: new Date(data.createdAt),
    updatedAt: data.updatedAt ? new Date(data.updatedAt) : null
  };
}

/**
 * Maps array of API responses to {Entity} array.
 */
export function map{Entity}Array(data: any[]): {Entity}[] {
  return data.map(map{Entity});
}

/**
 * Creates a blank {Entity} for new forms.
 */
export function createEmpty{Entity}(): Partial<{Entity}> {
  return {
    name: '',
    description: null,
    isActive: true,
    status: {Entity}Status.Draft
  };
}

/**
 * Converts {Entity} to Create request.
 */
export function toCreate{Entity}Request(entity: Partial<{Entity}>): Create{Entity}Request {
  return {
    name: entity.name!,
    description: entity.description ?? undefined,
    isActive: entity.isActive,
    {relatedEntity}Id: entity.{relatedEntity}Id!
  };
}
```

### 8. Barrel Export
```typescript
// src/app/features/{feature}/models/index.ts

export * from './{entity}.model';
export * from './{entity}-status.enum';
export * from './{entity}-request.model';
export * from './{entity}-form.model';
export * from './{entity}.guards';
```

---

## StandupBot-Specific Models

### User Entity
```csharp
// Backend
public class User : BaseEntity
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public UserRole Role { get; set; } = UserRole.Member;
    public int TeamId { get; set; }
    public virtual Team? Team { get; set; }
    public virtual ICollection<Standup> Standups { get; init; } = [];
}

public enum UserRole { Member = 0, Lead = 1 }
```

```typescript
// Frontend
export interface User {
  id: number;
  name: string;
  email: string;
  role: UserRole;
  teamId: number;
  team?: Team;
}

export enum UserRole {
  Member = 'Member',
  Lead = 'Lead'
}
```

### Standup Entity
```csharp
// Backend
public class Standup : BaseEntity
{
    public int UserId { get; set; }
    public DateOnly Date { get; set; }
    public required string JiraId { get; set; }
    public required string TaskDescription { get; set; }
    public int PercentageComplete { get; set; }
    public bool HasBlocker { get; set; }
    public string? BlockerDescription { get; set; }
    public BlockerStatus? BlockerStatus { get; set; }
    public required string NextTask { get; set; }
    
    public virtual User? User { get; set; }
}

public enum BlockerStatus { New = 0, Critical = 1, Resolved = 2 }
```

```typescript
// Frontend
export interface Standup {
  id: number;
  userId: number;
  date: Date;
  jiraId: string;
  taskDescription: string;
  percentageComplete: number;
  hasBlocker: boolean;
  blockerDescription: string | null;
  blockerStatus: BlockerStatus | null;
  nextTask: string;
  createdAt: Date;
  updatedAt: Date | null;
  user?: User;
}

export enum BlockerStatus {
  New = 'New',
  Critical = 'Critical',
  Resolved = 'Resolved'
}
```

---

## Best Practices Checklist

### Backend
- [ ] Use `required` modifier for non-nullable properties
- [ ] Use `init` for immutable properties
- [ ] Use records for DTOs
- [ ] Use collection expressions `[]` for navigation properties
- [ ] Apply validation attributes on request models
- [ ] Configure relationships in EF Core configurations
- [ ] Use enums with string conversion for readability
- [ ] Create separate Summary DTOs for list views

### Frontend
- [ ] Use interfaces for data models
- [ ] Use enums with string values (matches backend JSON)
- [ ] Create type guards for runtime validation
- [ ] Separate request models from response models
- [ ] Create typed form models
- [ ] Use barrel exports for clean imports
- [ ] Handle null/undefined properly
- [ ] Create mapper utilities for date parsing

### Synchronization
- [ ] Property names match (camelCase in TS, PascalCase in C#)
- [ ] Enum values match between backend and frontend
- [ ] Nullable types aligned (string? ↔ string | null)
- [ ] Date types handled (DateTime ↔ Date with parsing)
- [ ] Nested objects match structure

## Usage
When asked to create models, specify:
1. Entity name
2. Properties with types
3. Relationships (one-to-many, many-to-one)
4. Validation rules
5. Enum definitions needed
6. Whether Summary/List DTOs are needed
