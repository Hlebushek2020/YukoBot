using System.Threading.Tasks;

namespace YukoBot.Services;

public interface IBotNotificationsService
{
    Task SendReadyNotifications();
    Task SendShutdownNotifications(string reason);
}