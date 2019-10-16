using System;
using System.Threading.Tasks;
using CardsPCL.Models;

namespace CardsPCL.Interfaces
{
    public interface IOAuthService
    {
        Task<LoginResult> Login();
        void Logout();
    }
}
