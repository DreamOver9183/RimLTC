using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimLTC
{
    /// <summary>
    /// A completed biosculpter age reversal cycle also rolls back part of the
    /// elder's senescence. The hediff itself stays: the end still comes.
    /// </summary>
    [HarmonyPatch(typeof(CompBiosculpterPod_AgeReversalCycle), nameof(CompBiosculpterPod_AgeReversalCycle.CycleCompleted))]
    public static class Patch_BiosculpterAgeReversal
    {
        public const float SeverityReduction = 0.15f;
        private const float MinSeverity = 0.01f;

        public static void Postfix(Pawn pawn)
        {
            Hediff_Senescence senescence = SenescenceUtility.GetSenescence(pawn);
            if (senescence == null)
            {
                return;
            }
            senescence.Severity = Mathf.Max(MinSeverity, senescence.Severity - SeverityReduction);
            if (PawnUtility.ShouldSendNotificationAbout(pawn))
            {
                Messages.Message("RimLTC_BiosculpterReducedSenescence".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.PositiveEvent);
            }
        }
    }
}
