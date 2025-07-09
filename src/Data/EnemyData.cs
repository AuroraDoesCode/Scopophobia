namespace Scopophobia.Data
{
    public class EnemyData
    {
        public string PlanetName { get; private set; }
        public EnemyConfigData ConfigData { get; private set; }

        public EnemyData(string planetName, EnemyConfigDataDefault defaultConfigValues = default)
        {
            PlanetName = planetName;
            ConfigData = new EnemyConfigData(defaultConfigValues);
        }

        public void BindConfigs()
        {
            if (ConfigData == null)
            {
                ConfigData = new EnemyConfigData();
            }

            ConfigData.BindConfigs(PlanetName);
        }
    }
}
