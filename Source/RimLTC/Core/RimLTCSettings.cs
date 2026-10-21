using RimWorld;
using Verse;

namespace RimLTC
{
    public enum TriggerMode
    {
        AbsoluteAge,
        LifeExpectancyPercent
    }

    /// <summary>
    /// Player-facing settings. Defaults follow chapter 7 of the design document.
    /// </summary>
    public class RimLTCSettings : ModSettings
    {
        // Lifespan & trigger
        public TriggerMode triggerMode = TriggerMode.AbsoluteAge;
        public float triggerAge = 100f;
        public float triggerLifeExpectancyFactor = 1.2f;
        public float ageStdDev = 5f;
        public float untendedSurvivalDays = 30f;

        // Care & resources
        public float efficacyHands = 0.5f;
        public float efficacyHerbal = 0.75f;
        public float efficacyIndustrial = 1f;
        public float baseCareDurationHours = 12f;
        public bool enableFilth = true;
        public float filthIntervalHours = 8f;
        public MedicalCareCategory defaultHospiceCare = MedicalCareCategory.HerbalOrWorse;

        // Legacy & passing
        public bool enableLegacy = true;
        public int legacyMinSkillLevel = 10;
        public float legacyChanceMultiplier = 1f;
        public float legacyChanceCap = 0.75f;
        public int legacyLifetimeLimit = 1;
        public float satisfactionMultiplier = 1f;
        public float moodNeglected = -12f;
        public float moodRegretful = -4f;
        public float moodPeaceful = 2f;
        public float moodTranscendent = 5f;

        // Disease & technology
        public bool enableHeartAttack = true;
        public float heartAttackChance = 0.03f;
        public float bionicReductionPerOrgan = 0.10f;
        public bool respectAgelessGenes = true;

        // Target filter
        public bool applyToPrisoners;
        public bool applyToAnimals;

        public void ResetToDefaults()
        {
            RimLTCSettings defaults = new RimLTCSettings();
            triggerMode = defaults.triggerMode;
            triggerAge = defaults.triggerAge;
            triggerLifeExpectancyFactor = defaults.triggerLifeExpectancyFactor;
            ageStdDev = defaults.ageStdDev;
            untendedSurvivalDays = defaults.untendedSurvivalDays;
            efficacyHands = defaults.efficacyHands;
            efficacyHerbal = defaults.efficacyHerbal;
            efficacyIndustrial = defaults.efficacyIndustrial;
            baseCareDurationHours = defaults.baseCareDurationHours;
            enableFilth = defaults.enableFilth;
            filthIntervalHours = defaults.filthIntervalHours;
            defaultHospiceCare = defaults.defaultHospiceCare;
            enableLegacy = defaults.enableLegacy;
            legacyMinSkillLevel = defaults.legacyMinSkillLevel;
            legacyChanceMultiplier = defaults.legacyChanceMultiplier;
            legacyChanceCap = defaults.legacyChanceCap;
            legacyLifetimeLimit = defaults.legacyLifetimeLimit;
            satisfactionMultiplier = defaults.satisfactionMultiplier;
            moodNeglected = defaults.moodNeglected;
            moodRegretful = defaults.moodRegretful;
            moodPeaceful = defaults.moodPeaceful;
            moodTranscendent = defaults.moodTranscendent;
            enableHeartAttack = defaults.enableHeartAttack;
            heartAttackChance = defaults.heartAttackChance;
            bionicReductionPerOrgan = defaults.bionicReductionPerOrgan;
            respectAgelessGenes = defaults.respectAgelessGenes;
            applyToPrisoners = defaults.applyToPrisoners;
            applyToAnimals = defaults.applyToAnimals;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref triggerMode, "triggerMode", TriggerMode.AbsoluteAge);
            Scribe_Values.Look(ref triggerAge, "triggerAge", 100f);
            Scribe_Values.Look(ref triggerLifeExpectancyFactor, "triggerLifeExpectancyFactor", 1.2f);
            Scribe_Values.Look(ref ageStdDev, "ageStdDev", 5f);
            Scribe_Values.Look(ref untendedSurvivalDays, "untendedSurvivalDays", 30f);

            Scribe_Values.Look(ref efficacyHands, "efficacyHands", 0.5f);
            Scribe_Values.Look(ref efficacyHerbal, "efficacyHerbal", 0.75f);
            Scribe_Values.Look(ref efficacyIndustrial, "efficacyIndustrial", 1f);
            Scribe_Values.Look(ref baseCareDurationHours, "baseCareDurationHours", 12f);
            Scribe_Values.Look(ref enableFilth, "enableFilth", true);
            Scribe_Values.Look(ref filthIntervalHours, "filthIntervalHours", 8f);
            Scribe_Values.Look(ref defaultHospiceCare, "defaultHospiceCare", MedicalCareCategory.HerbalOrWorse);

            Scribe_Values.Look(ref enableLegacy, "enableLegacy", true);
            Scribe_Values.Look(ref legacyMinSkillLevel, "legacyMinSkillLevel", 10);
            Scribe_Values.Look(ref legacyChanceMultiplier, "legacyChanceMultiplier", 1f);
            Scribe_Values.Look(ref legacyChanceCap, "legacyChanceCap", 0.75f);
            Scribe_Values.Look(ref legacyLifetimeLimit, "legacyLifetimeLimit", 1);
            Scribe_Values.Look(ref satisfactionMultiplier, "satisfactionMultiplier", 1f);
            Scribe_Values.Look(ref moodNeglected, "moodNeglected", -12f);
            Scribe_Values.Look(ref moodRegretful, "moodRegretful", -4f);
            Scribe_Values.Look(ref moodPeaceful, "moodPeaceful", 2f);
            Scribe_Values.Look(ref moodTranscendent, "moodTranscendent", 5f);

            Scribe_Values.Look(ref enableHeartAttack, "enableHeartAttack", true);
            Scribe_Values.Look(ref heartAttackChance, "heartAttackChance", 0.03f);
            Scribe_Values.Look(ref bionicReductionPerOrgan, "bionicReductionPerOrgan", 0.10f);
            Scribe_Values.Look(ref respectAgelessGenes, "respectAgelessGenes", true);

            Scribe_Values.Look(ref applyToPrisoners, "applyToPrisoners", false);
            Scribe_Values.Look(ref applyToAnimals, "applyToAnimals", false);
        }
    }
}
