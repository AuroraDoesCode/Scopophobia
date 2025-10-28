using HarmonyLib;
using Scopophobia;

[HarmonyPatch(typeof(BeltBagItem))]
public static class BeltBagItemPatch
{
    [HarmonyPatch(typeof(BeltBagItem), nameof(BeltBagItem.PutObjectInBagLocalClient))]
    [HarmonyPrefix]
    static void Prefix_PutObjectInBagLocalClient(BeltBagItem __instance, GrabbableObject gObject)
    {
        if (gObject == null) return;

        var painting = gObject.GetComponent<ShyGuyPaintingProp>();
        if (painting != null && !painting.hasSpawned)
        {
            var player = __instance.playerHeldBy;
            ScopophobiaPlugin.Instance.LogInfoExtended($"[BeltBagPatch] Trigger painting on pickup: {painting.name}");
            painting.TriggerFromBeltBag(player);
        }
    }
}
