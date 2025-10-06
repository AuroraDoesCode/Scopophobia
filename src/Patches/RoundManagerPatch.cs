using HarmonyLib;
using Scopophobia;
using System.Linq;

[HarmonyPatch(typeof(RoundManager))]
internal static class RoundManagerPatch
{
    [HarmonyPatch(nameof(RoundManager.LoadNewLevel))]
    [HarmonyPostfix]
    private static void LoadNewLevelPatch()
    {
        EnemyDataManager.SetEnemyDataForCurrentLevel();
    }
}