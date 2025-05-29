using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Services.Interfaces
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(int userId, string permission);
        Task<bool> HasRoleAsync(int userId, string role);
        Task<bool> HasAnyPermissionAsync(int userId, params string[] permissions);
        Task<bool> HasAllPermissionsAsync(int userId, params string[] permissions);
        Task<List<string>> GetUserPermissionsAsync(int userId);
        Task<List<string>> GetUserRolesAsync(int userId);
    }
}