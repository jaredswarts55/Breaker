using Breaker.Core.Services;
using Xunit;

namespace Breaker.Tests
{
    public class UserSettingsServiceTests
    {
        private UserSettingsService _userSettings;

        public UserSettingsServiceTests()
        {
            _userSettings = new UserSettingsService();
        }

        [Fact]
        public void GetUserSettings_RetrievesOrCreatesSettings()
        {
            var folder = _userSettings.GetUserSettings();
        }
    }
}
