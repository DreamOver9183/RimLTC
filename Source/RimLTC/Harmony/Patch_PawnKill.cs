using HarmonyLib;
using Verse;

namespace RimLTC
{
    /// <summary>
    /// Reads the elder's care satisfaction just before death and, once the pawn is
    /// really dead, gives the colony the matching passing thought. Violent deaths
    /// are left to the vanilla death thoughts.
    /// </summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
    public static class Patch_PawnKill
    {
        public static void Prefix(Pawn __instance, DamageInfo? dinfo, out float? __state)
        {
            __state = null;
            if (__instance.Dead || !PassingUtility.IsColonyMember(__instance))
            {
                return;
            }
            HediffComp_SenescenceCare care = SenescenceUtility.GetSenescence(__instance)?.CareComp;
            if (care == null || (dinfo.HasValue && dinfo.Value.Def != null && dinfo.Value.Def.ExternalViolenceFor(__instance)))
            {
                return;
            }
            __state = care.careSatisfaction;
        }

        public static void Postfix(Pawn __instance, float? __state)
        {
            if (__state.HasValue && __instance.Dead)
            {
                PassingUtility.GivePassingThoughts(__instance, __state.Value);
            }
        }
    }
}
