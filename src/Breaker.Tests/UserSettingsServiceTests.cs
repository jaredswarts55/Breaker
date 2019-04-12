using System;
using Breaker.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Breaker.Tests
{
    [TestClass]
    public class UserSettingsServiceTests
    {
        private UserSettingsService _userSettings;

        public UserSettingsServiceTests()
        {
            _userSettings = new UserSettingsService();
        }

        [TestMethod]
        public void TestMethod1()
        {
            var folder = _userSettings.GetUserSettings();
        }
    }
}
