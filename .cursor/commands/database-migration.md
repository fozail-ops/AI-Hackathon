# Database Migration Command

## Description
Generate and manage Entity Framework Core database migrations for .NET Core 10 backend with best practices.

## Tech Stack
- .NET Core 10
- Entity Framework Core
- SQL Server
- Code-First Migrations

## Instructions

When working with database migrations, follow these patterns and practices:

---

## 1. Migration Commands Reference

### Creating Migrations
```powershell
# Navigate to the project directory containing DbContext
cd Backend/standupbot-backend/standupbot-backend

# Add a new migration
dotnet ef migrations add {MigrationName}

# Add migration with specific context (if multiple contexts)
dotnet ef migrations add {MigrationName} --context StandupBotContext

# Add migration with output directory
dotnet ef migrations add {MigrationName} --output-dir Migrations/{Feature}
```

### Applying Migrations
```powershell
# Apply all pending migrations
dotnet ef database update

# Apply migrations up to a specific migration
dotnet ef database update {MigrationName}

# Apply to specific connection string
dotnet ef database update --connection "Server=...;Database=...;"
```

### Reverting Migrations
```powershell
# Revert to a specific migration
dotnet ef database update {PreviousMigrationName}

# Revert all migrations (reset database)
dotnet ef database update 0

# Remove the last migration (if not applied)
dotnet ef migrations remove
```

### Viewing Migrations
```powershell
# List all migrations
dotnet ef migrations list

# Generate SQL script for all migrations
dotnet ef migrations script

# Generate SQL script from specific migration
dotnet ef migrations script {FromMigration} {ToMigration}

# Generate idempotent script (safe to run multiple times)
dotnet ef migrations script --idempotent --output migrate.sql
```

---

## 2. Migration Naming Conventions

### Use Descriptive Names
```powershell
# Good naming examples
dotnet ef migrations add InitialCreate
dotnet ef migrations add AddUserTable
dotnet ef migrations add AddStandupEntity
dotnet ef migrations add AddBlockerStatusToStandup
dotnet ef migrations add CreateTeamUserRelationship
dotnet ef migrations add AddIndexOnStandupDate
dotnet ef migrations add RenameUserEmailColumn
dotnet ef migrations add RemoveDeprecatedFields

# Bad naming examples (avoid)
dotnet ef migrations add Migration1
dotnet ef migrations add Update
dotnet ef migrations add Fix
dotnet ef migrations add Changes
```

### Naming Pattern
```
{Action}{Entity/Table}{Detail}

Actions:
- Add       → Adding new entity/column/index
- Create    → Creating relationships/constraints
- Remove    → Removing entity/column
- Rename    → Renaming column/table
- Alter     → Modifying column type/constraints
- Update    → Updating seed data
- Add{Index/Constraint/FK} → Adding specific database objects
```

---

## 3. Entity Configuration Best Practices

### Fluent API Configuration
```csharp
// Infrastructure/Data/Configurations/{Entity}Configuration.cs
namespace StandupBot.Infrastructure.Data.Configurations;

public class StandupConfiguration : IEntityTypeConfiguration<Standup>
{
    public void Configure(EntityTypeBuilder<Standup> builder)
    {
        // Table name
        builder.ToTable("standups");
        
        // Primary key
        builder.HasKey(e => e.Id);
        
        // Properties with constraints
        builder.Property(e => e.JiraId)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("jira_id");
            
        builder.Property(e => e.TaskDescription)
            .IsRequired()
            .HasMaxLength(500)
            .HasColumnName("task_description");
            
        builder.Property(e => e.PercentageComplete)
            .IsRequired()
            .HasColumnName("percentage_complete");
            
        // Enum conversion
        builder.Property(e => e.BlockerStatus)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnName("blocker_status");
            
        // Default values
        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .HasColumnName("created_at");
            
        // Indexes
        builder.HasIndex(e => e.Date)
            .HasDatabaseName("IX_standups_date");
            
        builder.HasIndex(e => new { e.UserId, e.Date })
            .IsUnique()
            .HasDatabaseName("IX_standups_user_date");
            
        // Relationships
        builder.HasOne(e => e.User)
            .WithMany(u => u.Standups)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

### Registering Configurations in DbContext
```csharp
// Data/StandupBotContext.cs
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    // Apply all configurations from assembly
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(StandupBotContext).Assembly);
    
    // Or apply individually
    modelBuilder.ApplyConfiguration(new UserConfiguration());
    modelBuilder.ApplyConfiguration(new StandupConfiguration());
    modelBuilder.ApplyConfiguration(new TeamConfiguration());
}
```

---

## 4. Seed Data in Migrations

### Using HasData in Configuration
```csharp
// In entity configuration
public void Configure(EntityTypeBuilder<Team> builder)
{
    builder.HasData(
        new Team 
        { 
            Id = 1, 
            Name = "Product Engineering", 
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) 
        }
    );
}
```

### Using Migration for Complex Seed Data
```csharp
// In the migration Up method
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.InsertData(
        table: "users",
        columns: new[] { "id", "name", "email", "role", "team_id", "created_at" },
        values: new object[,]
        {
            { 1, "Alice Johnson", "alice@company.com", "Lead", 1, DateTime.UtcNow },
            { 2, "Bob Smith", "bob@company.com", "Member", 1, DateTime.UtcNow }
        });
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DeleteData(
        table: "users",
        keyColumn: "id",
        keyValues: new object[] { 1, 2 });
}
```

---

## 5. Common Migration Operations

### Adding a New Column
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<string>(
        name: "avatar_url",
        table: "users",
        type: "nvarchar(500)",
        maxLength: 500,
        nullable: true);
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropColumn(
        name: "avatar_url",
        table: "users");
}
```

### Adding an Index
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateIndex(
        name: "IX_standups_created_at",
        table: "standups",
        column: "created_at");
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropIndex(
        name: "IX_standups_created_at",
        table: "standups");
}
```

### Renaming a Column
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.RenameColumn(
        name: "old_column_name",
        table: "users",
        newName: "new_column_name");
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.RenameColumn(
        name: "new_column_name",
        table: "users",
        newName: "old_column_name");
}
```

### Adding a Foreign Key
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<int>(
        name: "team_id",
        table: "users",
        type: "int",
        nullable: false,
        defaultValue: 1);

    migrationBuilder.CreateIndex(
        name: "IX_users_team_id",
        table: "users",
        column: "team_id");

    migrationBuilder.AddForeignKey(
        name: "FK_users_teams_team_id",
        table: "users",
        column: "team_id",
        principalTable: "teams",
        principalColumn: "id",
        onDelete: ReferentialAction.Restrict);
}
```

### Creating a New Table
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateTable(
        name: "notifications",
        columns: table => new
        {
            id = table.Column<int>(type: "int", nullable: false)
                .Annotation("SqlServer:Identity", "1, 1"),
            user_id = table.Column<int>(type: "int", nullable: false),
            message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
            is_read = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
            created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_notifications", x => x.id);
            table.ForeignKey(
                name: "FK_notifications_users_user_id",
                column: x => x.user_id,
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        });

    migrationBuilder.CreateIndex(
        name: "IX_notifications_user_id",
        table: "notifications",
        column: "user_id");
}
```

---

## 6. Data Migration Patterns

### Migrating Data During Schema Change
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Step 1: Add new column
    migrationBuilder.AddColumn<string>(
        name: "full_name",
        table: "users",
        type: "nvarchar(200)",
        maxLength: 200,
        nullable: true);

    // Step 2: Migrate data
    migrationBuilder.Sql(@"
        UPDATE users 
        SET full_name = CONCAT(first_name, ' ', last_name)
        WHERE full_name IS NULL
    ");

    // Step 3: Make column required
    migrationBuilder.AlterColumn<string>(
        name: "full_name",
        table: "users",
        type: "nvarchar(200)",
        maxLength: 200,
        nullable: false,
        defaultValue: "");

    // Step 4: Drop old columns
    migrationBuilder.DropColumn(name: "first_name", table: "users");
    migrationBuilder.DropColumn(name: "last_name", table: "users");
}
```

### Safe Column Type Change
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // When changing column types, ensure data compatibility
    migrationBuilder.Sql(@"
        -- Ensure all values can be converted
        UPDATE users SET percentage = 0 WHERE percentage IS NULL OR percentage < 0;
        UPDATE users SET percentage = 100 WHERE percentage > 100;
    ");

    migrationBuilder.AlterColumn<int>(
        name: "percentage",
        table: "standups",
        type: "int",
        nullable: false,
        oldClrType: typeof(decimal),
        oldType: "decimal(5,2)");
}
```

---

## 7. Production Migration Checklist

### Before Deploying
- [ ] Test migration on a copy of production data
- [ ] Generate and review SQL script: `dotnet ef migrations script --idempotent`
- [ ] Check for potential data loss warnings
- [ ] Ensure rollback migration (Down) is implemented
- [ ] Back up the database before applying
- [ ] Plan for downtime if needed (schema locks)
- [ ] Verify foreign key constraints won't cause issues

### Migration Script for Production
```powershell
# Generate idempotent script for production deployment
dotnet ef migrations script --idempotent --output ./Scripts/migrate_$(Get-Date -Format "yyyyMMdd_HHmmss").sql

# Review the generated script before applying
```

### Applying in Production
```powershell
# Option 1: Apply via EF Core (development/staging)
dotnet ef database update --connection "ProductionConnectionString"

# Option 2: Apply SQL script directly (recommended for production)
sqlcmd -S server -d database -i migrate.sql
```

---

## 8. Troubleshooting

### Common Issues

#### Migration Already Applied
```powershell
# If migration exists in database but not in code
# Check __EFMigrationsHistory table
SELECT * FROM __EFMigrationsHistory;

# Remove entry if needed (be careful!)
DELETE FROM __EFMigrationsHistory WHERE MigrationId = 'YourMigration';
```

#### Pending Model Changes
```powershell
# Check if model differs from last migration
dotnet ef migrations has-pending-model-changes
```

#### Reset Development Database
```powershell
# Drop and recreate database
dotnet ef database drop --force
dotnet ef database update
```

#### Fix Broken Migrations
```powershell
# Remove last migration (only if not applied)
dotnet ef migrations remove

# If applied, create a new migration to fix issues
dotnet ef migrations add Fix{PreviousMigrationName}Issue
```

---

## 9. Project Setup

### Required Packages
```xml
<!-- In .csproj file -->
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.0">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
  </PackageReference>
  <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.0">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
  </PackageReference>
</ItemGroup>
```

### Global Tool Installation
```powershell
# Install EF Core tools globally
dotnet tool install --global dotnet-ef

# Update to latest version
dotnet tool update --global dotnet-ef

# Verify installation
dotnet ef --version
```

---

## Best Practices Checklist

### Migration Design
- [ ] One migration per logical change
- [ ] Descriptive migration names
- [ ] Always implement Down() method
- [ ] Test rollback scenarios
- [ ] Avoid breaking changes when possible

### Data Safety
- [ ] Never delete columns with data without backup
- [ ] Use nullable columns first, then migrate data
- [ ] Add default values for new required columns
- [ ] Validate data before type changes

### Performance
- [ ] Add indexes for frequently queried columns
- [ ] Consider composite indexes for multi-column queries
- [ ] Avoid table locks during data migrations
- [ ] Use batched updates for large data migrations

### Code Organization
- [ ] Use separate configuration files per entity
- [ ] Keep migrations folder organized
- [ ] Document complex migrations with comments
- [ ] Version control all migration files

## Usage
When working with migrations, specify:
1. Type of change (add entity, add column, add index, etc.)
2. Entity/table affected
3. Any data migration requirements
4. Rollback strategy needed
