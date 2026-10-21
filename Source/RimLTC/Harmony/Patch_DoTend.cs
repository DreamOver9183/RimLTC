using HarmonyLib;
using RimWorld;
using Verse;

namespace RimLTC
{
    /// <summary>
    /// Remembers which medicine the current tend uses, so the senescence comp can
    /// tell which hospice tier it actually received.
    /// </summary>
    [HarmonyPatch(typeof(TendUtility), nameof(TendUtility.DoTend))]
    public static class Patch_DoTend
    {
        public static ThingDef CurrentMedicine { get; private set; }

        public static void Prefix(Medicine medicine)
        {
            CurrentMedicine = medicine?.def;
        }

        public static void Finalizer()
        {
            CurrentMedicine = null;
        }
    }
}
