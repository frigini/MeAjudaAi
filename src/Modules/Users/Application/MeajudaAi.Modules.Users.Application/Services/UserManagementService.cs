//using MeAjudaAi.Modules.Users.Application.DTOs;
//using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
//using MeAjudaAi.Modules.Users.Application.Interfaces;
//using MeAjudaAi.Modules.Users.Domain.Repositories;
//using MeAjudaAi.Shared.Common;

//namespace MeAjudaAi.Modules.Users.Application.Services;

//public class UserManagementService(IUserRepository userRepository) : IUserManagementService
//{
//    public async Task<Result<IEnumerable<UserDto>>> GetUsersAsync(
//        GetUsersRequest request,
//        CancellationToken cancellationToken = default)
//    {
//        try
//        {
//            //criar mapping entre request e domain
//            // Implementação
//            return Result<IEnumerable<UserDto>>.Success(users);
//        }
//        catch (Exception ex)
//        {
//            return Result<IEnumerable<UserDto>>.Failure(
//                Error.Internal($"Error getting users: {ex.Message}"));
//        }
//    }

//    public async Task<Result<UserDto>> GetUserByIdAsync(
//        Guid id,
//        CancellationToken cancellationToken = default)
//    {
//        try
//        {
//            var user = await userRepository.GetByIdAsync(id, cancellationToken);
//            if (user == null)
//                return Result<UserDto>.Failure(
//                    Error.NotFound($"User with id {id} not found"));

//            return Result<UserDto>.Success(/*_mapper.Map<UserDto>(user)*/);
//        }
//        catch (Exception ex)
//        {
//            return Result<UserDto>.Failure(
//                Error.Internal($"Error getting user: {ex.Message}"));
//        }
//    }

//    Task<Result<IEnumerable<UserDto>>> IUserManagementService.GetUsersAsync(GetUsersRequest request, CancellationToken cancellationToken)
//    {
//        throw new NotImplementedException();
//    }

//    Task<Result<int>> IUserManagementService.GetTotalUsersCountAsync(CancellationToken cancellationToken)
//    {
//        throw new NotImplementedException();
//    }

//    Task<Result<UserDto>> IUserManagementService.GetUserByIdAsync(Guid id, CancellationToken cancellationToken)
//    {
//        throw new NotImplementedException();
//    }

//    Task<Result<UserDto>> IUserManagementService.CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
//    {
//        throw new NotImplementedException();
//    }

//    Task<Result<UserDto>> IUserManagementService.UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken)
//    {
//        throw new NotImplementedException();
//    }

//    Task<Result<bool>> IUserManagementService.DeleteUserAsync(Guid id, CancellationToken cancellationToken)
//    {
//        throw new NotImplementedException();
//    }
//}