using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Breaker.Core.Models.Settings;
using Breaker.Core.Services.Base;
using Newtonsoft.Json;

namespace Breaker.Core.Services
{
    public class UserSettingsService : IUserSettingsService
    {
        private const string BreakerFolder = "Breaker";
        public UserSettingsService()
        {

        }

        public string GetUserSettingsFolder()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), BreakerFolder);
            if(!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        public void SaveDefaults()
        {
            var defaultSettingsJson = JsonConvert.SerializeObject(UserSettings.Defaults, Formatting.Indented);
            File.WriteAllText(GetUserSettingsFileFullPath(), defaultSettingsJson);
        }

        public string GetUserSettingsFileFullPath()
        {
            return Path.Combine(GetUserSettingsFolder(), "userSettings.json");
        }

        public UserSettings GetUserSettings()
        {
            var userSettingsFileFullPath = GetUserSettingsFileFullPath();
            if (!File.Exists(userSettingsFileFullPath))
                SaveDefaults();

            return ReadSettingsFromFile(userSettingsFileFullPath);
        }

        private static UserSettings ReadSettingsFromFile(string userSettingsFileFullPath)
        {
            var text = File.ReadAllText(userSettingsFileFullPath);
            return JsonConvert.DeserializeObject<UserSettings>(text);
        }
    }
}
