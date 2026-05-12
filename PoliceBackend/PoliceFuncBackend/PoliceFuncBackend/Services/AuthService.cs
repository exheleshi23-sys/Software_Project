using PoliceFuncBackend.Models;

namespace PoliceFuncBackend.Services
{
    public class AuthService
    {
        public bool IsAdmin(string role) =>
            role == RolePermissions.Admin;

        public bool IsOfficerOrHigher(string role) =>
            role == RolePermissions.Admin ||
            role == RolePermissions.Officer ||
            role == RolePermissions.Detective;

        public bool IsOfficer(string role) =>
            role == RolePermissions.Admin ||
            role == RolePermissions.Officer;
    }
}