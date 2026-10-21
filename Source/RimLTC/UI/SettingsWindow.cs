using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimLTC
{
    /// <summary>The mod settings page, grouped like chapter 7 of the design document.</summary>
    public static class SettingsWindow
    {
        private static Vector2 scrollPosition;
        private static float viewHeight = 1400f;

        public static void Draw(Rect inRect, RimLTCSettings s)
        {
            Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, viewHeight);
            Widgets.BeginScrollView(inRect, ref scrollPosition, viewRect);
            Listing_Standard l = new Listing_Standard();
            l.Begin(viewRect);

            Section(l, "RimLTC_Settings_Section_Lifespan");
            if (l.RadioButton("RimLTC_Settings_TriggerMode_Absolute".Translate(), s.triggerMode == TriggerMode.AbsoluteAge))
            {
                s.triggerMode = TriggerMode.AbsoluteAge;
            }
            if (l.RadioButton("RimLTC_Settings_TriggerMode_LifeExpectancy".Translate(), s.triggerMode == TriggerMode.LifeExpectancyPercent))
            {
                s.triggerMode = TriggerMode.LifeExpectancyPercent;
            }
            if (s.triggerMode == TriggerMode.AbsoluteAge)
            {
                s.triggerAge = Mathf.Round(Slider(l, "RimLTC_Settings_TriggerAge", s.triggerAge, 60f, 200f, s.triggerAge.ToString("F0")));
            }
            else
            {
                s.triggerLifeExpectancyFactor = GenMath.RoundTo(Slider(l, "RimLTC_Settings_TriggerLifeExpectancy",
                    s.triggerLifeExpectancyFactor, 0.8f, 2f, s.triggerLifeExpectancyFactor.ToStringPercent()), 0.05f);
            }
            l.Label("RimLTC_Settings_AnimalsUseLifeExpectancy".Translate());
            s.ageStdDev = GenMath.RoundTo(Slider(l, "RimLTC_Settings_AgeStdDev", s.ageStdDev, 0f, 15f, s.ageStdDev.ToString("F1")), 0.5f);
            s.untendedSurvivalDays = Mathf.Round(Slider(l, "RimLTC_Settings_UntendedSurvivalDays", s.untendedSurvivalDays, 5f, 120f, s.untendedSurvivalDays.ToString("F0")));

            Section(l, "RimLTC_Settings_Section_Care");
            s.efficacyHands = Percent(l, "RimLTC_Settings_EfficacyHands", s.efficacyHands, 0f, 1f);
            s.efficacyHerbal = Percent(l, "RimLTC_Settings_EfficacyHerbal", s.efficacyHerbal, 0f, 1f);
            s.efficacyIndustrial = Percent(l, "RimLTC_Settings_EfficacyIndustrial", s.efficacyIndustrial, 0f, 1f);
            s.baseCareDurationHours = Mathf.Round(Slider(l, "RimLTC_Settings_BaseCareDuration", s.baseCareDurationHours, 4f, 48f, s.baseCareDurationHours.ToString("F0")));
            l.CheckboxLabeled("RimLTC_Settings_EnableFilth".Translate(), ref s.enableFilth);
            if (s.enableFilth)
            {
                s.filthIntervalHours = Mathf.Round(Slider(l, "RimLTC_Settings_FilthInterval", s.filthIntervalHours, 2f, 24f, s.filthIntervalHours.ToString("F0")));
            }
            if (l.ButtonText("RimLTC_Settings_DefaultHospiceCare".Translate(HospiceCareUI.Label(s.defaultHospiceCare))))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (MedicalCareCategory tier in HospiceCareUI.Tiers)
                {
                    MedicalCareCategory chosen = tier;
                    options.Add(new FloatMenuOption(HospiceCareUI.Label(chosen), () => s.defaultHospiceCare = chosen,
                        HospiceCareUI.Icon(chosen), Color.white));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }

            Section(l, "RimLTC_Settings_Section_Legacy");
            l.CheckboxLabeled("RimLTC_Settings_EnableLegacy".Translate(), ref s.enableLegacy);
            if (s.enableLegacy)
            {
                s.legacyMinSkillLevel = Mathf.RoundToInt(Slider(l, "RimLTC_Settings_LegacyMinSkill", s.legacyMinSkillLevel, 6f, 20f, s.legacyMinSkillLevel.ToString()));
                s.legacyChanceMultiplier = GenMath.RoundTo(Slider(l, "RimLTC_Settings_LegacyChanceMultiplier", s.legacyChanceMultiplier, 0.1f, 3f, s.legacyChanceMultiplier.ToString("0.0#") + "x"), 0.05f);
                s.legacyChanceCap = Percent(l, "RimLTC_Settings_LegacyChanceCap", s.legacyChanceCap, 0.05f, 0.95f);
                s.legacyLifetimeLimit = Mathf.RoundToInt(Slider(l, "RimLTC_Settings_LegacyLifetimeLimit", s.legacyLifetimeLimit, 1f, 5f, s.legacyLifetimeLimit.ToString()));
            }
            s.satisfactionMultiplier = GenMath.RoundTo(Slider(l, "RimLTC_Settings_SatisfactionMultiplier", s.satisfactionMultiplier, 0.5f, 2f, s.satisfactionMultiplier.ToString("0.0#") + "x"), 0.05f);
            s.moodNeglected = Mathf.Round(Slider(l, "RimLTC_Settings_MoodNeglected", s.moodNeglected, -30f, 0f, s.moodNeglected.ToStringWithSign()));
            s.moodRegretful = Mathf.Round(Slider(l, "RimLTC_Settings_MoodRegretful", s.moodRegretful, -20f, 0f, s.moodRegretful.ToStringWithSign()));
            s.moodPeaceful = Mathf.Round(Slider(l, "RimLTC_Settings_MoodPeaceful", s.moodPeaceful, 0f, 10f, "±" + s.moodPeaceful.ToString("F0")));
            s.moodTranscendent = Mathf.Round(Slider(l, "RimLTC_Settings_MoodTranscendent", s.moodTranscendent, 0f, 20f, s.moodTranscendent.ToStringWithSign()));

            Section(l, "RimLTC_Settings_Section_Tech");
            l.CheckboxLabeled("RimLTC_Settings_EnableHeartAttack".Translate(), ref s.enableHeartAttack);
            if (s.enableHeartAttack)
            {
                s.heartAttackChance = Percent(l, "RimLTC_Settings_HeartAttackChance", s.heartAttackChance, 0f, 0.25f, 0.01f);
            }
            s.bionicReductionPerOrgan = Percent(l, "RimLTC_Settings_BionicReduction", s.bionicReductionPerOrgan, 0f, 0.25f, 0.01f);
            l.CheckboxLabeled("RimLTC_Settings_RespectAgelessGenes".Translate(), ref s.respectAgelessGenes);

            Section(l, "RimLTC_Settings_Section_Targets");
            l.Label("RimLTC_Settings_TargetsAlways".Translate());
            l.CheckboxLabeled("RimLTC_Settings_ApplyToPrisoners".Translate(), ref s.applyToPrisoners);
            l.CheckboxLabeled("RimLTC_Settings_ApplyToAnimals".Translate(), ref s.applyToAnimals);

            l.GapLine();
            if (l.ButtonText("RimLTC_Settings_Reset".Translate()))
            {
                s.ResetToDefaults();
            }

            viewHeight = l.CurHeight + 24f;
            l.End();
            Widgets.EndScrollView();
        }

        private static void Section(Listing_Standard l, string key)
        {
            l.Gap();
            Text.Font = GameFont.Medium;
            l.Label(key.Translate());
            Text.Font = GameFont.Small;
            l.GapLine(6f);
        }

        private static float Slider(Listing_Standard l, string key, float value, float min, float max, string valueLabel)
        {
            string tipKey = key + "_Tip";
            return l.SliderLabeled(key.Translate(valueLabel), value, min, max, 0.6f, tipKey.CanTranslate() ? tipKey.Translate().ToString() : null);
        }

        private static float Percent(Listing_Standard l, string key, float value, float min, float max, float step = 0.05f)
        {
            return GenMath.RoundTo(Slider(l, key, value, min, max, value.ToStringPercent()), step);
        }
    }
}
