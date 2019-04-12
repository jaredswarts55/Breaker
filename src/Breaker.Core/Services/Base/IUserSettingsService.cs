using Breaker.Core.Models.Settings;

namespace Breaker.Core.Services.Base
{
    public interface IUserSettingsService
    {
        UserSettings GetUserSettings();
    }
}