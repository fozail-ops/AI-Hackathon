using standupbot_backend.Data.Enums;

namespace standupbot_backend.DTOs;

/// <summary>
/// DTO for returning user data.
/// </summary>
public record UserDto(
    int Id,
    string Name,
    string Email,
    UserRole Role,
    int TeamId,
    string TeamName
);

/// <summary>
/// Summary DTO for team member lists.
/// </summary>
public record UserSummaryDto(
    int Id,
    string Name,
    UserRole Role
);
