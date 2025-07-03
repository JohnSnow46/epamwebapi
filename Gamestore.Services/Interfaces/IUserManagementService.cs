using Gamestore.Services.Dto.AuthDto;

namespace Gamestore.Services.Interfaces;
public interface IUserManagementService
{
    Task<IEnumerable<UserDto>> GetAllUsersForManagementAsync();
    Task<UserDto> GetUserForManagementAsync(string id);
    Task<object> CreateUserForManagementAsync(CreateUserRequest request);
    Task<object> UpdateUserForManagementAsync(string id, UpdateUserRequest request);
    Task<object> DeleteUserForManagementAsync(string id, string currentUserEmail);
    Task<IEnumerable<RoleDto>> GetUserRolesForManagementAsync(string id);
}
