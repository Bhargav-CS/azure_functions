using System.Threading.Tasks;

namespace AzureFunctionsProject.Core.Interfaces
{
    public interface IAuthService
    {
        Task<string> AuthenticateAsync(string email, string password);
    }
}