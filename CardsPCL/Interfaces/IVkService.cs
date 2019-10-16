using System;
using System.Threading.Tasks;
using CardsPCL.Models;

namespace CardsPCL.Interfaces
{
    public interface IVkService
    {
        Task<LoginResult> Login();
        void Logout();
    }
}
