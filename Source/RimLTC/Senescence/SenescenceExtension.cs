using System.Collections.Generic;
using Verse;

namespace RimLTC
{
    /// <summary>
    /// XML-tunable references for the senescence hediff, kept out of code so
    /// other mods can patch them.
    /// </summary>
    public class SenescenceExtension : DefModExtension
    {
        public ThingDef filthDef;
        public List<HediffDef> cascadeTriggerHediffs = new List<HediffDef>();
        public HediffDef cascadeHediff;
        public float cascadeMinSeverity = 0.5f;
        public List<GeneDef> immuneGenes = new List<GeneDef>();

        public static SenescenceExtension Get =>
            RimLTCDefOf.RimLTC_NaturalSenescence.GetModExtension<SenescenceExtension>();
    }
}
