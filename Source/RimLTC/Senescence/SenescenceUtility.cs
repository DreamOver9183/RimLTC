using RimWorld;
using UnityEngine;
using Verse;

namespace RimLTC
{
    public static class SenescenceUtility
    {
        private const int ThresholdSeedSalt = 0x4C5443;
        private const float ReferenceHumanLifeExpectancy = 80f;

        private static RimLTCSettings Settings => RimLTCMod.Settings;

        /// <summary>
        /// Whether the target filter covers this pawn, ignoring its age.
        /// </summary>
        public static bool IsCandidate(Pawn pawn)
        {
            if (pawn == null || pawn.Dead || pawn.ageTracker == null)
            {
                return false;
            }
            RaceProperties race = pawn.RaceProps;
            if (race == null || !race.IsFlesh || race.IsMechanoid || race.IsAnomalyEntity || pawn.IsMutant)
            {
                return false;
            }
            if (race.Humanlike)
            {
                if (pawn.IsPrisonerOfColony)
                {
                    return Settings.applyToPrisoners;
                }
                // Colonists and slaves.
                return pawn.Faction != null && pawn.Faction.IsPlayer;
            }
            if (race.Animal)
            {
                return Settings.applyToAnimals && pawn.Faction != null && pawn.Faction.IsPlayer;
            }
            return false;
        }

        /// <summary>
        /// Biological age at which this pawn develops senescence. The Gaussian offset
        /// is seeded by the pawn's id, so it is stable without being saved.
        /// </summary>
        public static float TriggerAge(Pawn pawn)
        {
            RaceProperties race = pawn.RaceProps;
            bool useLifeExpectancy = Settings.triggerMode == TriggerMode.LifeExpectancyPercent || !race.Humanlike;
            float baseAge = useLifeExpectancy
                ? race.lifeExpectancy * Settings.triggerLifeExpectancyFactor
                : Settings.triggerAge;

            float stdDev = Settings.ageStdDev;
            if (!race.Humanlike)
            {
                stdDev *= race.lifeExpectancy / ReferenceHumanLifeExpectancy;
            }

            float offset = 0f;
            if (stdDev > 0f)
            {
                Rand.PushState(Gen.HashCombineInt(pawn.thingIDNumber, ThresholdSeedSalt));
                offset = Mathf.Clamp(Rand.Gaussian(0f, stdDev), -3f * stdDev, 3f * stdDev);
                Rand.PopState();
            }
            return Mathf.Max(1f, baseAge + offset);
        }

        public static bool IsGeneImmune(Pawn pawn)
        {
            if (!Settings.respectAgelessGenes || pawn.genes == null)
            {
                return false;
            }
            SenescenceExtension ext = SenescenceExtension.Get;
            if (ext != null)
            {
                foreach (GeneDef gene in ext.immuneGenes)
                {
                    if (gene != null && pawn.genes.HasActiveGene(gene))
                    {
                        return true;
                    }
                }
            }
            // Respect any gene that explicitly blocks this hediff, including modded ones.
            foreach (Gene gene in pawn.genes.GenesListForReading)
            {
                if (gene.Active && gene.def.hediffGiversCannotGive != null
                    && gene.def.hediffGiversCannotGive.Contains(RimLTCDefOf.RimLTC_NaturalSenescence))
                {
                    return true;
                }
            }
            return false;
        }

        public static Hediff_Senescence GetSenescence(Pawn pawn)
        {
            return pawn?.health?.hediffSet?.GetFirstHediffOfDef(RimLTCDefOf.RimLTC_NaturalSenescence) as Hediff_Senescence;
        }

        public static bool ShouldDevelopSenescence(Pawn pawn)
        {
            return IsCandidate(pawn)
                && GetSenescence(pawn) == null
                && pawn.ageTracker.AgeBiologicalYearsFloat >= TriggerAge(pawn)
                && !IsGeneImmune(pawn);
        }

        public static void AddSenescence(Pawn pawn)
        {
            pawn.health.AddHediff(RimLTCDefOf.RimLTC_NaturalSenescence);
        }

        /// <summary>Maximum share of the progression that artificial organs can remove.</summary>
        public const float MaxBionicReduction = 0.5f;

        private static BodyPartDef liverDef;
        private static BodyPartDef kidneyDef;

        /// <summary>
        /// Each artificial heart, lung, liver or kidney slows progression by the
        /// configured share, up to 50%. The brain still ages, so death still comes.
        /// </summary>
        public static float BionicReduction(Pawn pawn)
        {
            if (liverDef == null)
            {
                liverDef = DefDatabase<BodyPartDef>.GetNamedSilentFail("Liver");
                kidneyDef = DefDatabase<BodyPartDef>.GetNamedSilentFail("Kidney");
            }
            int organs = 0;
            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                if (!(hediff is Hediff_AddedPart) || hediff.Part == null)
                {
                    continue;
                }
                BodyPartDef part = hediff.Part.def;
                if (part == BodyPartDefOf.Heart || part == BodyPartDefOf.Lung
                    || (part != null && (part == liverDef || part == kidneyDef)))
                {
                    organs++;
                }
            }
            return Mathf.Min(organs * Settings.bionicReductionPerOrgan, MaxBionicReduction);
        }

        /// <summary>Severity gained per day without any care.</summary>
        public static float BaseDailyProgression => 1f / Mathf.Max(1f, Settings.untendedSurvivalDays);
    }
}
