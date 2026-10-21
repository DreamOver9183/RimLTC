using RimWorld;
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
    /// All work runs once per in-game hour.
    /// </summary>
    public class HediffComp_SenescenceCare : HediffComp
    {
        public const int UpdateIntervalTicks = GenDate.TicksPerHour;
        private const int HalfDayHours = 12;

        public MedicalCareCategory lastCareTier = MedicalCareCategory.NoCare;
        public float lastTendQuality;
        private int hoursSinceHalfDayCheck;

        private HediffComp_TendDuration tendComp;

        private static RimLTCSettings Settings => RimLTCMod.Settings;

        public HediffComp_TendDuration TendComp => tendComp ?? (tendComp = parent.TryGetComp<HediffComp_TendDuration>());

        /// <summary>Whether the last hospice tend is still in effect.</summary>
        public bool IsCaredFor => lastCareTier > MedicalCareCategory.NoCare && TendComp != null && TendComp.IsTended;

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

            hoursSinceHalfDayCheck++;
            if (hoursSinceHalfDayCheck >= HalfDayHours)
            {
                hoursSinceHalfDayCheck = 0;
                HalfDayCheck();
            }
        }

        private void HalfDayCheck()
        {
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref lastCareTier, "lastCareTier", MedicalCareCategory.NoCare);
            Scribe_Values.Look(ref lastTendQuality, "lastTendQuality");
            Scribe_Values.Look(ref hoursSinceHalfDayCheck, "hoursSinceHalfDayCheck");
        }

        public override string CompDebugString()
        {
            return "care tier: " + lastCareTier
                + "\ncared for: " + IsCaredFor
                + "\nprogression/day: " + (HourlyProgression * GenDate.HoursPerDay).ToString("F4");
        }
    }
}
