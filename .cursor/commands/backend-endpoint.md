# Backend .NET Core 10 API Endpoint Generation

## Description
Generate a clean architecture .NET Core 10 Web API endpoint with Entity Framework Core integration.

## Tech Stack
- .NET Core 10
- Entity Framework Core
- Clean Architecture Pattern
- Repository Pattern
- CQRS (Command Query Responsibility Segregation) when applicable

## Instructions

When generating a backend endpoint, follow these clean architecture principles:

### 1. Project Structure
```
Backend/
├── Domain/
│   ├── Entities/           # Domain entities (pure C# classes)
│   ├── Enums/              # Domain enumerations
│   └── Interfaces/         # Domain interfaces (IRepository, etc.)
├── Application/
│   ├── DTOs/               # Data Transfer Objects
│   ├── Interfaces/         # Application service interfaces
│   ├── Services/           # Application services
│   ├── Validators/         # FluentValidation validators
│   └── Mappings/           # AutoMapper profiles
├── Infrastructure/
│   ├── Data/               # EF Core DbContext, configurations
│   ├── Repositories/       # Repository implementations
│   └── Services/           # External service implementations
└── API/
    ├── Controllers/        # API Controllers
    ├── Middleware/         # Custom middleware
    └── Filters/            # Action filters
```

### 2. Entity Creation Pattern
```csharp
// Domain/Entities/{EntityName}.cs
namespace StandupBot.Domain.Entities;

public class {EntityName} : BaseEntity
{
    public required {Type} {PropertyName} { get; set; }
    
    // Navigation properties
    public virtual ICollection<{RelatedEntity}> {RelatedEntities} { get; set; } = [];
}

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
```

### 3. DTO Pattern
```csharp
// Application/DTOs/{EntityName}Dto.cs
namespace StandupBot.Application.DTOs;

public record {EntityName}Dto(
    int Id,
    {Type} {PropertyName},
    DateTime CreatedAt
);

public record Create{EntityName}Request(
    {Type} {PropertyName}
);

public record Update{EntityName}Request(
    {Type} {PropertyName}
);
```

### 4. Repository Interface Pattern
```csharp
// Domain/Interfaces/I{EntityName}Repository.cs
namespace StandupBot.Domain.Interfaces;

public interface I{EntityName}Repository : IRepository<{EntityName}>
{
    Task<{EntityName}?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<{EntityName}>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
}
```

### 5. Repository Implementation Pattern
```csharp
// Infrastructure/Repositories/{EntityName}Repository.cs
namespace StandupBot.Infrastructure.Repositories;

public class {EntityName}Repository : Repository<{EntityName}>, I{EntityName}Repository
{
    public {EntityName}Repository(StandupBotContext context) : base(context) { }

    public async Task<{EntityName}?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.{EntityName}s
            .Include(e => e.RelatedEntity)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }
}
```

### 6. Service Interface Pattern
```csharp
// Application/Interfaces/I{EntityName}Service.cs
namespace StandupBot.Application.Interfaces;

public interface I{EntityName}Service
{
    Task<{EntityName}Dto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<{EntityName}Dto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<{EntityName}Dto> CreateAsync(Create{EntityName}Request request, CancellationToken cancellationToken = default);
    Task<{EntityName}Dto?> UpdateAsync(int id, Update{EntityName}Request request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
```

### 7. Service Implementation Pattern
```csharp
// Application/Services/{EntityName}Service.cs
namespace StandupBot.Application.Services;

public class {EntityName}Service : I{EntityName}Service
{
    private readonly I{EntityName}Repository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<{EntityName}Service> _logger;

    public {EntityName}Service(
        I{EntityName}Repository repository,
        IMapper mapper,
        ILogger<{EntityName}Service> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<{EntityName}Dto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity is null ? null : _mapper.Map<{EntityName}Dto>(entity);
    }

    public async Task<{EntityName}Dto> CreateAsync(Create{EntityName}Request request, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<{EntityName}>(request);
        var created = await _repository.AddAsync(entity, cancellationToken);
        _logger.LogInformation("{EntityName} created with Id: {Id}", created.Id);
        return _mapper.Map<{EntityName}Dto>(created);
    }
}
```

### 8. Controller Pattern
```csharp
// API/Controllers/{EntityName}Controller.cs
namespace StandupBot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class {EntityName}Controller : ControllerBase
{
    private readonly I{EntityName}Service _service;
    private readonly ILogger<{EntityName}Controller> _logger;

    public {EntityName}Controller(I{EntityName}Service service, ILogger<{EntityName}Controller> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Gets all {EntityName} records
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<{EntityName}Dto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<{EntityName}Dto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a {EntityName} by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof({EntityName}Dto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<{EntityName}Dto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Creates a new {EntityName}
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof({EntityName}Dto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<{EntityName}Dto>> Create([FromBody] Create{EntityName}Request request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing {EntityName}
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof({EntityName}Dto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<{EntityName}Dto>> Update(int id, [FromBody] Update{EntityName}Request request, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, request, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Deletes a {EntityName}
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
```

### 9. Dependency Injection Registration
```csharp
// In Program.cs or ServiceCollectionExtensions
builder.Services.AddScoped<I{EntityName}Repository, {EntityName}Repository>();
builder.Services.AddScoped<I{EntityName}Service, {EntityName}Service>();
```

### 10. EF Core Configuration
```csharp
// Infrastructure/Data/Configurations/{EntityName}Configuration.cs
namespace StandupBot.Infrastructure.Data.Configurations;

public class {EntityName}Configuration : IEntityTypeConfiguration<{EntityName}>
{
    public void Configure(EntityTypeBuilder<{EntityName}> builder)
    {
        builder.ToTable("{EntityName}s");
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.{PropertyName})
            .IsRequired()
            .HasMaxLength(500);
            
        builder.HasIndex(e => e.{PropertyName});
    }
}
```

## Best Practices Checklist
- [ ] Use async/await with CancellationToken throughout
- [ ] Implement proper exception handling with Problem Details
- [ ] Use DTOs, never expose domain entities directly
- [ ] Apply validation using FluentValidation or Data Annotations
- [ ] Use ILogger for structured logging
- [ ] Apply proper HTTP status codes
- [ ] Document endpoints with XML comments for Swagger
- [ ] Use repository pattern for data access
- [ ] Register dependencies with appropriate lifetime (Scoped for EF Core)
- [ ] Use primary constructors where applicable (.NET 10)
- [ ] Enable nullable reference types

## Usage
When asked to create an endpoint, specify:
1. Entity name
2. Properties and their types
3. Relationships with other entities
4. Any custom business logic requirements
5. Validation rules
