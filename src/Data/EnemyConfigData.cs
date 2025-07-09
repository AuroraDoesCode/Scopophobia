using BepInEx.Configuration;

namespace Scopophobia.Data
{
    public class EnemyConfigData
    {
        public EnemyConfigDataDefault DefaultValues { get; private set; }

        public ConfigEntry<int> SpawnWeight { get; private set; }
        public ConfigEntry<int> MaxSpawnCount { get; private set; }
        public ConfigEntry<bool> SpawnInside { get; private set; }
        public ConfigEntry<bool> SpawnOutside { get; private set; }

        private string _planetName;

        public EnemyConfigData()
        {
            DefaultValues = new EnemyConfigDataDefault();
        }

        public EnemyConfigData(EnemyConfigDataDefault defaultValues)
        {
            DefaultValues = defaultValues;
        }

        public void BindConfigs(string planetName)
        {
            DefaultValues ??= new EnemyConfigDataDefault();

            _planetName = planetName;

            string section = $"{planetName} Spawn Settings";

            SpawnWeight = Config.Bind(section, "SpawnWeight", defaultValue: DefaultValues.SpawnWeight, requiresRestart: false, $"The spawn weight of {EnemyDataManager.EnemyName}.");
            MaxSpawnCount = Config.Bind(section, "MaxSpawnCount", defaultValue: DefaultValues.MaxSpawnCount, requiresRestart: false, $"The max amount of {EnemyDataManager.EnemyName} that can spawn.");
            SpawnInside = Config.Bind(section, "SpawnInside", defaultValue: DefaultValues.SpawnInside, requiresRestart: false, $"If enabled, {EnemyDataManager.EnemyName} will be able to spawn inside.");
            SpawnOutside = Config.Bind(section, "SpawnOutside", defaultValue: DefaultValues.SpawnOutside, requiresRestart: false, $"If enabled, {EnemyDataManager.EnemyName} will be able to spawn outside.");

            SpawnWeight.SettingChanged += SpawnWeight_SettingChanged;
            MaxSpawnCount.SettingChanged += MaxSpawnCount_SettingChanged;
            SpawnInside.SettingChanged += SpawnInside_SettingChanged;
            SpawnOutside.SettingChanged += SpawnOutside_SettingChanged;
        }

        private void SpawnWeight_SettingChanged(object sender, System.EventArgs e)
        {
            if (SpawnInside.Value)
            {
                EnemyHelper.SetSpawnWeight(EnemyDataManager.EnemyName, SpawnWeight.Value, EnemyListType.Inside, _planetName);
            }

            if (SpawnOutside.Value)
            {
                EnemyHelper.SetSpawnWeight(EnemyDataManager.EnemyName, SpawnWeight.Value, EnemyListType.Outside, _planetName);
            }
        }

        private void MaxSpawnCount_SettingChanged(object sender, System.EventArgs e)
        {
            EnemyHelper.SetMaxSpawnCount(EnemyDataManager.EnemyName, MaxSpawnCount.Value, _planetName);
        }

        private void SpawnInside_SettingChanged(object sender, System.EventArgs e)
        {
            if (SpawnInside.Value)
            {
                LevelHelper.AddEnemyToLevel(_planetName, EnemyDataManager.EnemyName, SpawnWeight.Value, EnemyListType.Inside);
            }
            else
            {
                LevelHelper.RemoveEnemyFromLevel(_planetName, EnemyDataManager.EnemyName, EnemyListType.Inside);
            }
        }

        private void SpawnOutside_SettingChanged(object sender, System.EventArgs e)
        {
            if (SpawnOutside.Value)
            {
                LevelHelper.AddEnemyToLevel(_planetName, EnemyDataManager.EnemyName, SpawnWeight.Value, EnemyListType.Outside);
            }
            else
            {
                LevelHelper.RemoveEnemyFromLevel(_planetName, EnemyDataManager.EnemyName, EnemyListType.Outside);
            }
        }
    }

    public class EnemyConfigDataDefault
    {
        public int SpawnWeight = 1;
        public int MaxSpawnCount = 1;
        public bool SpawnInside = true;
        public bool SpawnOutside = true;

        public EnemyConfigDataDefault()
        {

        }

        public EnemyConfigDataDefault(int spawnWeight, int maxSpawnCount, bool spawnInside, bool spawnOutside)
        {
            SpawnWeight = spawnWeight;
            MaxSpawnCount = maxSpawnCount;
            SpawnInside = spawnInside;
            SpawnOutside = spawnOutside;
        }
    }
}
