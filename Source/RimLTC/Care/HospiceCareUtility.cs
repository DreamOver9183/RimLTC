using RimWorld;
using UnityEngine;
using Verse;

namespace RimLTC
{
    /// <summary>
    /// Hospice care tiers reuse the vanilla <see cref="MedicalCareCategory"/> enum,
    /// limited to NoCare (none), NoMeds (hands only), HerbalOrWorse (herbal) and
    /// NormalOrWorse (industrial). Glitterworld medicine is never a hospice tier.
    /// </summary>
    public static class HospiceCareUtility
    {
        /// <summary>How much of the base progression full (100%) efficacy removes.</summary>
        public const float CareStrength = 0.8f;

        /// <summary>Medicines below this potency count as the herbal tier.</summary>
        private const float IndustrialPotencyThreshold = 0.8f;

        private static RimLTCSettings Settings => RimLTCMod.Settings;

        public static MedicalCareCategory Clamp(MedicalCareCategory tier)
        {
            return tier > MedicalCareCategory.NormalOrWorse ? MedicalCareCategory.NormalOrWorse : tier;
        }

        public static MedicalCareCategory GetHospiceCare(Pawn pawn)
        {
            CompHospiceSettings comp = pawn?.TryGetComp<CompHospiceSettings>();
            return comp != null ? comp.HospiceCare : Clamp(Settings.defaultHospiceCare);
        }

        public static void SetHospiceCare(Pawn pawn, MedicalCareCategory tier)
        {
            CompHospiceSettings comp = pawn?.TryGetComp<CompHospiceSettings>();
            if (comp != null)
            {
                comp.HospiceCare = tier;
            }
        }

        /// <summary>
        /// True when senescence needs tending and nothing else does (no wounds,
        /// diseases or other tendable conditions).
        /// </summary>
        public static bool OnlyNeedsHospiceCare(Pawn patient, out MedicalCareCategory hospiceCare)
        {
            hospiceCare = MedicalCareCategory.NoCare;
            if (patient?.health?.hediffSet == null)
            {
                return false;
            }
            bool senescenceNeedsCare = false;
            foreach (Hediff hediff in patient.health.hediffSet.hediffs)
            {
                if (!hediff.TendableNow())
                {
                    continue;
                }
                if (!(hediff is Hediff_Senescence))
                {
                    return false;
                }
                senescenceNeedsCare = true;
            }
            if (!senescenceNeedsCare)
            {
                return false;
            }
            hospiceCare = GetHospiceCare(patient);
            return true;
        }

        public static float Efficacy(MedicalCareCategory tier)
        {
            switch (Clamp(tier))
            {
                case MedicalCareCategory.NoMeds:
                    return Settings.efficacyHands;
                case MedicalCareCategory.HerbalOrWorse:
                    return Settings.efficacyHerbal;
                case MedicalCareCategory.NormalOrWorse:
                    return Settings.efficacyIndustrial;
                default:
                    return 0f;
            }
        }

        /// <summary>
        /// Multiplier on the daily progression while cared for. Always positive, so
        /// senescence always ends in a natural death.
        /// </summary>
        public static float ProgressionFactor(MedicalCareCategory tier)
        {
            return 1f - CareStrength * Mathf.Clamp01(Efficacy(tier));
        }

        /// <summary>Multiplier on the base care duration (8 / 12 / 16 hours by default).</summary>
        public static float DurationFactor(MedicalCareCategory tier)
        {
            switch (Clamp(tier))
            {
                case MedicalCareCategory.NoMeds:
                    return 2f / 3f;
                case MedicalCareCategory.HerbalOrWorse:
                    return 1f;
                case MedicalCareCategory.NormalOrWorse:
                    return 4f / 3f;
                default:
                    return 0f;
            }
        }

        public static int CareDurationTicks(MedicalCareCategory tier)
        {
            return Mathf.RoundToInt(Settings.baseCareDurationHours * DurationFactor(tier) * GenDate.TicksPerHour);
        }

        /// <summary>The hospice tier a tend counts as, from the medicine actually used.</summary>
        public static MedicalCareCategory TierFromMedicine(ThingDef medicine)
        {
            if (medicine == null)
            {
                return MedicalCareCategory.NoMeds;
            }
            float potency = medicine.GetStatValueAbstract(StatDefOf.MedicalPotency);
            return potency < IndustrialPotencyThreshold
                ? MedicalCareCategory.HerbalOrWorse
                : MedicalCareCategory.NormalOrWorse;
        }
    }
}
