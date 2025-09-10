using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
using MeAjudaAi.Modules.Users.Application.Services;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValuleObjects;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Shared.Common;
using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Infrastructure.Services;

public sealed class UserService : IUserService
{
    private readonly UsersDbContext _context;
    private readonly IKeycloakService _keycloakService;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UsersDbContext context,
        IKeycloakService keycloakService,
        IEventDispatcher eventDispatcher,
        ILogger<UserService> logger)
    {
        _context = context;
        _keycloakService = keycloakService;
        _eventDispatcher = eventDispatcher;
        _logger = logger;
    }

    public async Task<Result<UserDto>> RegisterUserAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Check if user already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.Value == request.Email, cancellationToken);

            if (existingUser is not null)
                return Result<UserDto>.Failure("User with this email already exists");

            // 2. Create user in Keycloak
            var keycloakResult = await _keycloakService.CreateUserAsync(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                cancellationToken);

            if (keycloakResult.IsFailure)
                return Result<UserDto>.Failure(keycloakResult.Error);

            // 3. Create user in our database
            var userId = new UserId(Guid.NewGuid());
            var email = new Email(request.Email);
            var userProfile = new UserProfile(request.FirstName, request.LastName);

            var user = new User(userId, email, userProfile, keycloakResult.Value.UserId);
            user.AssignRole("Customer"); // Default role

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            // 4. Dispatch domain events
            foreach (var domainEvent in user.DomainEvents)
            {
                await _eventDispatcher.PublishAsync(domainEvent, cancellationToken);
            }
            user.ClearDomainEvents();

            return Result<UserDto>.Success(MapToUserDto(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user with email {Email}", request.Email);
            return Result<UserDto>.Failure("An error occurred while registering the user");
        }
    }

    public async Task<Result<UserDto>> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.ServiceProvider)
                .FirstOrDefaultAsync(u => u.Id.Value == id, cancellationToken);

            if (user is null)
                return Result<UserDto>.Failure("User not found");

            return Result<UserDto>.Success(MapToUserDto(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", id);
            return Result<UserDto>.Failure("An error occurred while retrieving the user");
        }
    }

    public async Task<Result<UserDto>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.ServiceProvider)
                .FirstOrDefaultAsync(u => u.Email.Value == email, cancellationToken);

            if (user is null)
                return Result<UserDto>.Failure("User not found");

            return Result<UserDto>.Success(MapToUserDto(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email {Email}", email);
            return Result<UserDto>.Failure("An error occurred while retrieving the user");
        }
    }

    public async Task<Result<UserDto>> UpdateUserAsync(Guid id, UpdateUserProfileRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id.Value == id, cancellationToken);

            if (user is null)
                return Result<UserDto>.Failure("User not found");

            var newProfile = new UserProfile(request.FirstName, request.LastName);
            user.UpdateProfile(newProfile);

            await _context.SaveChangesAsync(cancellationToken);

            // Dispatch domain events
            foreach (var domainEvent in user.DomainEvents)
            {
                await _eventDispatcher.PublishAsync(domainEvent, cancellationToken);
            }
            user.ClearDomainEvents();

            return Result<UserDto>.Success(MapToUserDto(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return Result<UserDto>.Failure("An error occurred while updating the user");
        }
    }

    public async Task<Result<bool>> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id.Value == id, cancellationToken);

            if (user is null)
                return Result<bool>.Failure("User not found");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return Result<bool>.Failure("An error occurred while deleting the user");
        }
    }

    public async Task<Result<bool>> ActivateUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id.Value == id, cancellationToken);

            if (user is null)
                return Result<bool>.Failure("User not found");

            user.Activate();
            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating user {UserId}", id);
            return Result<bool>.Failure("An error occurred while activating the user");
        }
    }

    public async Task<Result<bool>> DeactivateUserAsync(Guid id, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id.Value == id, cancellationToken);

            if (user is null)
                return Result<bool>.Failure("User not found");

            user.Deactivate(reason);
            await _context.SaveChangesAsync(cancellationToken);

            // Dispatch domain events
            foreach (var domainEvent in user.DomainEvents)
            {
                await _eventDispatcher.PublishAsync(domainEvent, cancellationToken);
            }
            user.ClearDomainEvents();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user {UserId}", id);
            return Result<bool>.Failure("An error occurred while deactivating the user");
        }
    }

    public async Task<Result<PagedResponse<IEnumerable<UserDto>>>> GetUsersAsync(GetUsersRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Users
                .Include(u => u.ServiceProvider)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                query = query.Where(u => u.Email.Value.Contains(request.Email));
            }

            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                query = query.Where(u => u.Roles.Contains(request.Role));
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(u => u.Status.ToString() == request.Status);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var users = await query
                .OrderBy(u => u.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var userDtos = users.Select(MapToUserDto);
            var pagedResponse = new PagedResponse<IEnumerable<UserDto>>(
                userDtos,
                request.PageNumber,
                request.PageSize,
                totalCount);

            return Result<PagedResponse<IEnumerable<UserDto>>>.Success(pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return Result<PagedResponse<IEnumerable<UserDto>>>.Failure("An error occurred while retrieving users");
        }
    }

    public async Task<Result<int>> GetTotalUsersCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var count = await _context.Users.CountAsync(cancellationToken);
            return Result<int>.Success(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total users count");
            return Result<int>.Failure("An error occurred while counting users");
        }
    }

    public async Task<Result<bool>> AssignRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id.Value == userId, cancellationToken);

            if (user is null)
                return Result<bool>.Failure("User not found");

            user.AssignRole(role);
            await _context.SaveChangesAsync(cancellationToken);

            // Dispatch domain events
            foreach (var domainEvent in user.DomainEvents)
            {
                await _eventDispatcher.PublishAsync(domainEvent, cancellationToken);
            }
            user.ClearDomainEvents();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role {Role} to user {UserId}", role, userId);
            return Result<bool>.Failure("An error occurred while assigning role");
        }
    }

    public async Task<Result<bool>> RemoveRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id.Value == userId, cancellationToken);

            if (user is null)
                return Result<bool>.Failure("User not found");

            user.Roles.Remove(role);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role {Role} from user {UserId}", role, userId);
            return Result<bool>.Failure("An error occurred while removing role");
        }
    }

    private static UserDto MapToUserDto(User user) => new(
        user.Id.Value,
        user.Email.Value,
        user.Profile.FirstName,
        user.Profile.LastName,
        user.Profile.PhoneNumber?.Value,
        user.Status.ToString(),
        user.KeycloakId,
        user.Roles,
        user.LastLoginAt,
        user.IsServiceProvider,
        user.CreatedAt,
        user.UpdatedAt
    );
}