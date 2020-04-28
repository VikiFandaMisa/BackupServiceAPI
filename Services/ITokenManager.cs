using System.Threading.Tasks;

namespace BackupServiceAPI.Services
{
    public interface ITokenManager
    {
        Task<bool> IsCurrentTokenActive();
        Task InvalidateCurrentToken();
        Task<bool> IsTokenActive(string token);
        Task InvalidateToken(string token);
    }
}