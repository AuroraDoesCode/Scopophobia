using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Scopophobia.Dependencies;
using Scopophobia.Patches;
using UnityEngine;
using Unity.Netcode;
using LethalLib;
using LethalLib.Modules;

namespace Scopophobia
{
    [BepInPlugin("Scopophobia", "Scopophobia", "1.2.91")]
    [BepInDependency(LethalConfigProxy.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class ScopophobiaPlugin : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("Scopophobia");
       // public static class ScopophobiaKeys
       // {
           // public static readonly NamespacedKey<DawnItemInfo> Painting = NamespacedKey<DawnItemInfo>.From("scopophobia", "painting");
           // public static readonly NamespacedKey<DawnEnemyInfo> ShyGuy = NamespacedKey<DawnEnemyInfo>.From("scopophobia", "shyguy");
        //}//comment dawnlib temporarily for a 1.3.0 release
        public static EnemyType shyGuy;

        public static AssetBundle Assets;
        internal static ScopophobiaPlugin Instance;

        public static SpawnableEnemyWithRarity maskedPrefab;
        public static SpawnableEnemyWithRarity shyEnemy;
        public static Item ShyGuyPainting1;
        public static SpawnableItemWithRarity shyPainting1Prefab;
        public static ManualLogSource logger;
        public static float ShyGuyVolume;
        public static SpawnableEnemyWithRarity shyPrefab;

        public static Config MyConfig { get; internal set; }

        internal Assembly assembly => Assembly.GetExecutingAssembly();

        internal string GetFilePath(string path)
        {
            return assembly.Location.Replace(assembly.GetName().Name + ".dll", path);
        }

        private void LoadAssets()
        {
            try
            {
                Assets = AssetBundle.LoadFromFile(GetFilePath("scp096"));
            }
            catch (Exception arg)
            {
                logger.LogError($"Failed to load asset bundle! {arg}");
            }
        }
        private void Awake()
        {
            if (Instance == null) Instance = this;
            NetcodePatchAwake();
            LoadAssets();
            logger = base.Logger;
            MyConfig = new Config(base.Config);
            base.Config.TryGetEntry("General", "Enable the Shy Guy", out ConfigEntry<bool> shyGuyEnabled);
            if (!shyGuyEnabled.Value)
            {
                return;
            }
            ShyGuyVolume = Scopophobia.Config.VolumeConfig.Value;
            shyGuy = Assets.LoadAsset<EnemyType>("ShyGuyDef.asset");
            TerminalNode val = Assets.LoadAsset<TerminalNode>("ShyGuyTerminal.asset");
            TerminalKeyword val2 = Assets.LoadAsset<TerminalKeyword>("ShyGuyKeyword.asset");
            ShyGuyPainting1 = Assets.LoadAsset<Item>("ShyGuyPainting.asset");
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(shyGuy.enemyPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(ShyGuyPainting1.spawnPrefab);
            Enemies.RegisterEnemy(shyGuy, 15, Levels.LevelTypes.All, val, val2);
            Items.RegisterScrap(ShyGuyPainting1, Scopophobia.Config.paintingSpawnRateConfig.Value, Levels.LevelTypes.All);
            //DawnLib.RegisterNetworkPrefab(shyGuy.enemyPrefab);//temp comment out until DawnLib gets an official update.
            //DawnLib.RegisterNetworkPrefab(ShyGuyPainting1.spawnPrefab);
            //DawnLib.DefineItem(ScopophobiaKeys.Painting, ShyGuyPainting1, builder => { builder.DefineScrap(scrapBuilder => { scrapBuilder.SetWeights(scrapBuilder => { scrapBuilder.SetGlobalWeight(Scopophobia.Config.PaintingSpawnRate); }); }); });
            logger.LogInfo("Scopophobia | SCP-096 has entered the facility. All remaining personnel proceed with caution.");
            harmony.PatchAll(typeof(GetShyGuyPrefabForLaterUse));
            harmony.PatchAll(typeof(AudioSpatializerDisabler));//disable annoying audiospacializer issue globally
            harmony.PatchAll(typeof(RoundManagerPatch));//credit Crit / Zehs
            harmony.PatchAll(typeof(StartOfRoundPatch));//credit Crit / Zehs
            harmony.PatchAll(typeof(BeltBagItemPatch));

        }
        private static void NetcodePatchAwake()
        {
            // See https://github.com/EvaisaDev/UnityNetcodePatcher?tab=readme-ov-file#preparing-mods-for-patching
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }


        public void LogInfoExtended(object data)
        {
            if (Scopophobia.Config.ExtendedLogging)
            {
                logger.LogInfo(data);
            }
        }
        public void LogErrorExtended(object data)
        {
            if (Scopophobia.Config.ExtendedLogging)
            {
                logger.LogError(data);
            }
        }

        public void LogWarningExtended(object data)
        {
            if (Scopophobia.Config.ExtendedLogging)
            {
                logger.LogWarning(data);
            }
        }
    }
}