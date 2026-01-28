# Clean .NET Core Code Command

## Description
Refactor and clean .NET Core 10 code to follow best practices, improve performance, and maintain code quality.

## Tech Stack
- .NET Core 10
- Entity Framework Core
- C# 13
- Clean Architecture

## Instructions

When cleaning .NET Core code, apply these patterns and practices:

### 1. Primary Constructor Pattern (C# 12+)

#### Before
```csharp
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;
    
    public UserService(
        IUserRepository userRepository, 
        IMapper mapper, 
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }
}
```

#### After
```csharp
public class UserService(
    IUserRepository userRepository, 
    IMapper mapper, 
    ILogger<UserService> logger) : IUserService
{
    public async Task<UserDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(id, ct);
        return user is null ? null : mapper.Map<UserDto>(user);
    }
}
```

### 2. Record Types for DTOs

#### Before
```csharp
public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateUserRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
```

#### After
```csharp
public record UserDto(
    int Id,
    string Name,
    string Email,
    DateTime CreatedAt
);

public record CreateUserRequest(
    [property: Required] string Name,
    [property: Required, EmailAddress] string Email
);

public record UpdateUserRequest(
    string? Name,
    string? Email
);
```

### 3. Required Members and Init-Only Properties

#### Before
```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
```

#### After
```csharp
public class User
{
    public int Id { get; init; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<Order> Orders { get; init; } = [];
}
```

### 4. Pattern Matching

#### Before
```csharp
public string GetStatusMessage(Status status)
{
    if (status == Status.Active)
        return "User is active";
    else if (status == Status.Pending)
        return "User is pending approval";
    else if (status == Status.Inactive)
        return "User is inactive";
    else
        return "Unknown status";
}

public decimal CalculateDiscount(Customer customer)
{
    if (customer == null)
        throw new ArgumentNullException(nameof(customer));
        
    if (customer.Type == CustomerType.Premium)
        return 0.2m;
    else if (customer.Type == CustomerType.Gold)
        return 0.15m;
    else if (customer.Type == CustomerType.Silver)
        return 0.1m;
    else
        return 0;
}
```

#### After
```csharp
public string GetStatusMessage(Status status) => status switch
{
    Status.Active => "User is active",
    Status.Pending => "User is pending approval",
    Status.Inactive => "User is inactive",
    _ => "Unknown status"
};

public decimal CalculateDiscount(Customer customer) => customer switch
{
    null => throw new ArgumentNullException(nameof(customer)),
    { Type: CustomerType.Premium } => 0.2m,
    { Type: CustomerType.Gold } => 0.15m,
    { Type: CustomerType.Silver } => 0.1m,
    _ => 0m
};

// Property patterns
public bool IsEligible(User user) => user is 
{ 
    IsActive: true, 
    Age: >= 18, 
    Email: not null 
};
```

### 5. Null Handling

#### Before
```csharp
public User? GetUser(int id)
{
    var user = _repository.GetById(id);
    if (user == null)
        return null;
    return user;
}

public string GetDisplayName(User? user)
{
    if (user != null && user.Name != null)
        return user.Name;
    return "Unknown";
}
```

#### After
```csharp
public User? GetUser(int id) 
    => _repository.GetById(id);

public string GetDisplayName(User? user) 
    => user?.Name ?? "Unknown";

// Null coalescing assignment
public void EnsureCreated(ref List<Item>? items) 
    => items ??= [];

// Null-forgiving when certain
var userName = user!.Name; // Only when you're 100% sure
```

### 6. Collection Expressions (C# 12+)

#### Before
```csharp
var list = new List<int> { 1, 2, 3, 4, 5 };
var array = new int[] { 1, 2, 3, 4, 5 };
var empty = new List<string>();
var combined = list1.Concat(list2).ToList();
```

#### After
```csharp
List<int> list = [1, 2, 3, 4, 5];
int[] array = [1, 2, 3, 4, 5];
List<string> empty = [];
List<int> combined = [..list1, ..list2];

// In class properties
public ICollection<Order> Orders { get; init; } = [];
```

### 7. LINQ Optimization

#### Before
```csharp
var result = users
    .Where(u => u.IsActive)
    .Where(u => u.Age >= 18)
    .Select(u => u.Name)
    .ToList();

var first = users.Where(u => u.Id == id).FirstOrDefault();
var any = users.Where(u => u.IsActive).Any();
var count = users.Where(u => u.IsActive).Count();
```

#### After
```csharp
var result = users
    .Where(u => u.IsActive && u.Age >= 18)
    .Select(u => u.Name)
    .ToList();

var first = users.FirstOrDefault(u => u.Id == id);
var any = users.Any(u => u.IsActive);
var count = users.Count(u => u.IsActive);

// Use AsNoTracking for read-only queries
var users = await _context.Users
    .AsNoTracking()
    .Where(u => u.IsActive)
    .ToListAsync(ct);
```

### 8. Async/Await Best Practices

#### Before
```csharp
public async Task<User> GetUserAsync(int id)
{
    var user = await _repository.GetByIdAsync(id);
    if (user == null)
    {
        throw new Exception("User not found");
    }
    return user;
}

public Task SaveUserAsync(User user)
{
    return _repository.SaveAsync(user);
}
```

#### After
```csharp
public async Task<User> GetUserAsync(int id, CancellationToken ct = default)
{
    var user = await _repository.GetByIdAsync(id, ct);
    return user ?? throw new KeyNotFoundException($"User with ID {id} not found");
}

// No async/await needed when just returning
public Task SaveUserAsync(User user, CancellationToken ct = default) 
    => _repository.SaveAsync(user, ct);

// Use ValueTask for hot paths
public ValueTask<User?> GetCachedUserAsync(int id)
{
    if (_cache.TryGetValue(id, out var user))
        return ValueTask.FromResult(user);
    
    return new ValueTask<User?>(LoadUserAsync(id));
}
```

### 9. Entity Framework Core Optimization

#### Before
```csharp
public async Task<List<Order>> GetOrdersWithItems(int userId)
{
    var orders = await _context.Orders.ToListAsync();
    foreach (var order in orders.Where(o => o.UserId == userId))
    {
        order.Items = await _context.OrderItems
            .Where(i => i.OrderId == order.Id)
            .ToListAsync();
    }
    return orders.Where(o => o.UserId == userId).ToList();
}
```

#### After
```csharp
public async Task<List<Order>> GetOrdersWithItems(
    int userId, 
    CancellationToken ct = default)
{
    return await _context.Orders
        .AsNoTracking()
        .Include(o => o.Items)
        .Where(o => o.UserId == userId)
        .AsSplitQuery()  // For large includes
        .ToListAsync(ct);
}

// Projection for better performance
public async Task<List<OrderSummaryDto>> GetOrderSummaries(
    int userId, 
    CancellationToken ct = default)
{
    return await _context.Orders
        .AsNoTracking()
        .Where(o => o.UserId == userId)
        .Select(o => new OrderSummaryDto(
            o.Id,
            o.OrderDate,
            o.Items.Count,
            o.Items.Sum(i => i.Price * i.Quantity)
        ))
        .ToListAsync(ct);
}
```

### 10. Result Pattern for Error Handling

```csharp
// Generic Result type
public record Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public string? Error { get; init; }
    public List<string> Errors { get; init; } = [];
    
    public static Result<T> Success(T value) => new() 
    { 
        IsSuccess = true, 
        Value = value 
    };
    
    public static Result<T> Failure(string error) => new() 
    { 
        IsSuccess = false, 
        Error = error 
    };
    
    public static Result<T> Failure(List<string> errors) => new() 
    { 
        IsSuccess = false, 
        Errors = errors 
    };
}

// Usage in service
public async Task<Result<UserDto>> CreateUserAsync(
    CreateUserRequest request, 
    CancellationToken ct = default)
{
    if (await _repository.EmailExistsAsync(request.Email, ct))
        return Result<UserDto>.Failure("Email already exists");
    
    var user = _mapper.Map<User>(request);
    await _repository.AddAsync(user, ct);
    
    return Result<UserDto>.Success(_mapper.Map<UserDto>(user));
}

// Usage in controller
[HttpPost]
public async Task<IActionResult> Create(CreateUserRequest request, CancellationToken ct)
{
    var result = await _userService.CreateUserAsync(request, ct);
    
    return result.IsSuccess 
        ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
        : BadRequest(new { error = result.Error });
}
```

### 11. Minimal API Patterns

#### Before (Controller-based)
```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        return user == null ? NotFound() : Ok(user);
    }
}
```

#### After (Minimal API with Carter/Endpoints)
```csharp
// Endpoints/UserEndpoints.cs
public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .WithOpenApi();
        
        group.MapGet("/{id:int}", GetById)
            .WithName("GetUserById")
            .Produces<UserDto>(200)
            .Produces(404);
        
        group.MapPost("/", Create)
            .WithName("CreateUser")
            .Produces<UserDto>(201)
            .ProducesValidationProblem();
    }
    
    private static async Task<IResult> GetById(
        int id, 
        IUserService userService, 
        CancellationToken ct)
    {
        var user = await userService.GetByIdAsync(id, ct);
        return user is null 
            ? Results.NotFound() 
            : Results.Ok(user);
    }
    
    private static async Task<IResult> Create(
        CreateUserRequest request,
        IUserService userService,
        CancellationToken ct)
    {
        var result = await userService.CreateAsync(request, ct);
        return Results.CreatedAtRoute("GetUserById", new { id = result.Id }, result);
    }
}

// In Program.cs
app.MapUserEndpoints();
```

### 12. Dependency Injection Best Practices

```csharp
// Program.cs - Clean registration
var builder = WebApplication.CreateBuilder(args);

// Extension method for clean organization
builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddApplication()
    .AddApi();

// Infrastructure/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Default")));
        
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        return services;
    }
    
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddAutoMapper(typeof(MappingProfile));
        
        return services;
    }
}
```

### 13. Configuration Best Practices

```csharp
// Strongly typed settings
public record DatabaseSettings
{
    public required string ConnectionString { get; init; }
    public int CommandTimeout { get; init; } = 30;
    public bool EnableSensitiveDataLogging { get; init; }
}

// Registration
builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection("Database"));

// Usage with primary constructor
public class UserRepository(
    AppDbContext context, 
    IOptions<DatabaseSettings> settings) : IUserRepository
{
    private readonly DatabaseSettings _settings = settings.Value;
}

// Or use IOptionsSnapshot for reloadable settings
public class FeatureService(IOptionsSnapshot<FeatureSettings> settings)
{
    public bool IsEnabled => settings.Value.EnableNewFeature;
}
```

### 14. File-Scoped Namespaces

#### Before
```csharp
namespace StandupBot.Application.Services
{
    public class UserService : IUserService
    {
        // ...
    }
}
```

#### After
```csharp
namespace StandupBot.Application.Services;

public class UserService : IUserService
{
    // ...
}
```

## Cleanup Checklist

### Language Features
- [ ] Use primary constructors
- [ ] Use records for DTOs
- [ ] Use file-scoped namespaces
- [ ] Use collection expressions `[]`
- [ ] Use pattern matching
- [ ] Use null-coalescing operators
- [ ] Use required and init properties

### Async/Await
- [ ] Add CancellationToken parameters
- [ ] Use ValueTask for hot paths
- [ ] Remove unnecessary async/await
- [ ] Use ConfigureAwait(false) in libraries

### Entity Framework
- [ ] Use AsNoTracking for read queries
- [ ] Use projections (Select) when possible
- [ ] Use AsSplitQuery for large includes
- [ ] Avoid N+1 queries
- [ ] Use explicit loading judiciously

### General
- [ ] Remove unused usings
- [ ] Remove dead code
- [ ] Apply consistent formatting
- [ ] Use meaningful names
- [ ] Extract magic numbers to constants
- [ ] Apply single responsibility principle
- [ ] Use Result pattern for error handling

## Usage
When cleaning .NET code, specify:
1. File/class to clean
2. Specific patterns to apply
3. Performance concerns
4. Migration requirements
