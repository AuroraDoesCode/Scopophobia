using UnityEngine;
using Unity.Netcode;
using HarmonyLib;
namespace Scopophobia.Patches
{
    public class AudioSpatializerDisabler
    {
        [HarmonyPatch(typeof(NetworkSceneManager), "OnSceneLoaded")]
        [HarmonyPostfix]
        public static void DisableSpacializer(NetworkSceneManager __instance)
        {

            string pluginName = AudioSettings.GetSpatializerPluginName();
            if (string.IsNullOrEmpty(pluginName))
            {
                foreach (AudioSource audioSource in Resources.FindObjectsOfTypeAll<AudioSource>())
                {
                    audioSource.spatialize = false;
                }
            }
            ScopophobiaPlugin.logger.LogInfo("Scopophobia disabled Audio Spacializer errors!");
        }
    }
}