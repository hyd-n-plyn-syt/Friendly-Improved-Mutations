using HarmonyLib;
using XRL.World.Parts.Mutation;

namespace XRL.World
{
    [HarmonyPatch(typeof(DarkVision))]
    class Improved_DarkVision
    {
        [HarmonyPrefix]
        [HarmonyPatch("GetLevelText")]
        public static bool Prefix(ref DarkVision __instance, ref string __result, int Level)
        {
            __result = "You can see " + (Level + 4) + " squares away in the dark.";
            return false;
        }
        
        [HarmonyPrefix]
        [HarmonyPatch("ChangeLevel")]
        public static void Prefix(ref DarkVision __instance, int NewLevel)
        {
            __instance.Radius = NewLevel + 4;
        }

        [HarmonyPrefix]
        [HarmonyPatch("Mutate")]
        public static void Prefix(ref DarkVision __instance, GameObject GO, int Level)
        {
            __instance.Radius = Level + 4;
        }

        [HarmonyPrefix]
        [HarmonyPatch("CanLevel")]
        public static bool Prefix(ref DarkVision __instance, ref bool __result)
        {
            __result = true;
            return false;
        }
    }
}