using Gamestore.Services.Dto.AuthDto;

namespace Gamestore.Services.Interfaces;
public interface IRoleManagementService
{
    Task<IEnumerable<RoleDto>> GetAllRolesForManagementAsync();
    Task<RoleDto> GetRoleForManagementAsync(string id);
    Task<object> CreateRoleForManagementAsync(AddRoleRequest request);
    Task<object> UpdateRoleForManagementAsync(UpdateRoleRequest request);
    Task<object> DeleteRoleForManagementAsync(string id);
    Task<IEnumerable<string>> GetAllPermissionsForManagementAsync();
    Task<IEnumerable<string>> GetRolePermissionsForManagementAsync(string id);
}
