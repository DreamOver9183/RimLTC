using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimLTC
{
    public class HediffCompProperties_SenescenceCare : HediffCompProperties
    {
        public HediffCompProperties_SenescenceCare()
        {
            compClass = typeof(HediffComp_SenescenceCare);
        }
    }

    /// <summary>
    /// Drives senescence progression and keeps the hidden care state of one elder.
    /// All periodic work runs once per in-game hour.
    /// </summary>
    public class HediffComp_SenescenceCare : HediffComp
    {
        public const int UpdateIntervalTicks = GenDate.TicksPerHour;
        private const int HalfDayHours = 12;
        private const int BedriddenStageIndex = 1;

        /// <summary>Hidden 0-100 score that decides the passing thought.</summary>
        public float careSatisfaction = CareSatisfactionUtility.Initial;
        public MedicalCareCategory lastCareTier = MedicalCareCategory.NoCare;
        public float lastTendQuality;
        private int hoursSinceHalfDayCheck;
        private int hoursSinceFilth;

        private HediffComp_TendDuration tendComp;

        private static RimLTCSettings Settings => RimLTCMod.Settings;

        public HediffComp_TendDuration TendComp => tendComp ?? (tendComp = parent.TryGetComp<HediffComp_TendDuration>());

        /// <summary>Whether the last hospice tend is still in effect.</summary>
        public bool IsCaredFor => lastCareTier > MedicalCareCategory.NoCare && TendComp != null && TendComp.IsTended;

        public bool IsBedridden => parent.CurStageIndex >= BedriddenStageIndex;

        public float HourlyProgression
        {
            get
            {
                float factor = IsCaredFor ? HospiceCareUtility.ProgressionFactor(lastCareTier) : 1f;
                return SenescenceUtility.BaseDailyProgression / GenDate.HoursPerDay * factor;
            }
        }

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            if (!Pawn.IsHashIntervalTick(UpdateIntervalTicks, delta))
            {
                return;
            }
            severityAdjustment += HourlyProgression;

            hoursSinceFilth++;
            if (hoursSinceFilth >= Settings.filthIntervalHours)
            {
                hoursSinceFilth = 0;
                TryMakeFilth();
            }

            hoursSinceHalfDayCheck++;
            if (hoursSinceHalfDayCheck >= HalfDayHours)
            {
                hoursSinceHalfDayCheck = 0;
                HalfDayCheck();
            }
        }

        public override void CompTended(float quality, float maxQuality, int batchPosition = 0)
        {
            base.CompTended(quality, maxQuality, batchPosition);
            MedicalCareCategory tier = HospiceCareUtility.TierFromMedicine(Patch_DoTend.CurrentMedicine);
            lastCareTier = tier;
            lastTendQuality = Mathf.Clamp01(quality);
            if (TendComp != null)
            {
                TendComp.tendTicksLeft = HospiceCareUtility.CareDurationTicks(tier);
            }
            careSatisfaction = CareSatisfactionUtility.AfterTend(careSatisfaction, tier, lastTendQuality, Pawn);
            LegacyUtility.TryProduceLegacy(Pawn, parent.CurStageIndex, tier, lastTendQuality);
        }

        private void HalfDayCheck()
        {
            careSatisfaction = Mathf.Max(0f,
                careSatisfaction - CareSatisfactionUtility.NeglectPenalty(Pawn, IsCaredFor));
            TryCascadeComplication();
        }

        /// <summary>Soiled bedding: vanilla filth that vanilla cleaners take care of.</summary>
        private void TryMakeFilth()
        {
            if (!Settings.enableFilth || !IsBedridden || !Pawn.Spawned)
            {
                return;
            }
            ThingDef filth = SenescenceExtension.Get?.filthDef;
            if (filth != null)
            {
                FilthMaker.TryMakeFilth(Pawn.Position, Pawn.Map, filth, Pawn.LabelIndefinite());
            }
        }

        /// <summary>Severe senescence can turn an existing heart condition into a heart attack.</summary>
        private void TryCascadeComplication()
        {
            SenescenceExtension ext = SenescenceExtension.Get;
            if (!Settings.enableHeartAttack || ext?.cascadeHediff == null || parent.Severity < ext.cascadeMinSeverity)
            {
                return;
            }
            HediffSet hediffSet = Pawn.health.hediffSet;
            if (hediffSet.HasHediff(ext.cascadeHediff)
                || !ext.cascadeTriggerHediffs.Any(h => h != null && hediffSet.HasHediff(h))
                || !Rand.Chance(Settings.heartAttackChance))
            {
                return;
            }
            BodyPartRecord heart = Pawn.RaceProps.body.GetPartsWithDef(BodyPartDefOf.Heart)
                .FirstOrDefault(p => !hediffSet.PartIsMissing(p) && !hediffSet.HasDirectlyAddedPartFor(p));
            if (heart == null)
            {
                return;
            }
            Pawn.health.AddHediff(ext.cascadeHediff, heart);
            if (PawnUtility.ShouldSendNotificationAbout(Pawn))
            {
                Messages.Message("RimLTC_CascadeComplication".Translate(Pawn.Named("PAWN"), ext.cascadeHediff.label),
                    Pawn, MessageTypeDefOf.NegativeHealthEvent);
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref careSatisfaction, "careSatisfaction", CareSatisfactionUtility.Initial);
            Scribe_Values.Look(ref lastCareTier, "lastCareTier", MedicalCareCategory.NoCare);
            Scribe_Values.Look(ref lastTendQuality, "lastTendQuality");
            Scribe_Values.Look(ref hoursSinceHalfDayCheck, "hoursSinceHalfDayCheck");
            Scribe_Values.Look(ref hoursSinceFilth, "hoursSinceFilth");
        }

        public override string CompDebugString()
        {
            return "care satisfaction: " + careSatisfaction.ToString("F1")
                + "\ncare tier: " + lastCareTier
                + "\ncared for: " + IsCaredFor
                + "\nprogression/day: " + (HourlyProgression * GenDate.HoursPerDay).ToString("F4");
        }
    }
}
