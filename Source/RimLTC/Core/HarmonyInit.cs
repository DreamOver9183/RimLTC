using HarmonyLib;
using Verse;

namespace RimLTC
{
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        public const string HarmonyId = "DreamOver.RimLTC";

        static HarmonyInit()
        {
            new Harmony(HarmonyId).PatchAll();
        }
    }
}
