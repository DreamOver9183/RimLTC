using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimLTC
{
    /// <summary>
    /// Elders being cared for may leave a vanilla skilltrainer of their best skill
    /// by their bedside.
    /// </summary>
    public static class LegacyUtility
    {
        /// <summary>Base chance per tend for stage 1 / 2 / 3.</summary>
        private static readonly float[] StageBaseChance = { 0.02f, 0.06f, 0.15f };

        private static Dictionary<SkillDef, ThingDef> trainerBySkill;

        private static RimLTCSettings Settings => RimLTCMod.Settings;

        /// <summary>Skilltrainer for a skill, found by its learn-skill comp rather than by defName.</summary>
        public static ThingDef TrainerFor(SkillDef skill)
        {
            if (trainerBySkill == null)
            {
                trainerBySkill = new Dictionary<SkillDef, ThingDef>();
                foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
                {
                    SkillDef learned = def.GetCompProperties<CompProperties_UseEffect_LearnSkill>()?.skill;
                    if (learned == null)
                    {
                        continue;
                    }
                    // Prefer the generated vanilla neurotrainer if several items teach the same skill.
                    bool isVanillaTrainer = def.defName.StartsWith(ThingDefGenerator_Neurotrainer.NeurotrainerDefPrefix);
                    if (!trainerBySkill.ContainsKey(learned) || isVanillaTrainer)
                    {
                        trainerBySkill[learned] = def;
                    }
                }
            }
            return skill != null && trainerBySkill.TryGetValue(skill, out ThingDef trainer) ? trainer : null;
        }

        public static float ChanceFor(int stageIndex, MedicalCareCategory tier, float quality)
        {
            float stageBase = StageBaseChance[Mathf.Clamp(stageIndex, 0, StageBaseChance.Length - 1)];
            float chance = stageBase * HospiceCareUtility.Efficacy(tier) * Mathf.Clamp01(quality) * Settings.legacyChanceMultiplier;
            return Mathf.Min(chance, Settings.legacyChanceCap);
        }

        /// <summary>Rolled once per hospice tend.</summary>
        public static void TryProduceLegacy(Pawn pawn, int stageIndex, MedicalCareCategory tier, float quality, bool forced = false)
        {
            if (!Settings.enableLegacy || !pawn.Spawned || pawn.skills == null)
            {
                return;
            }
            CompHospiceSettings hospice = pawn.TryGetComp<CompHospiceSettings>();
            if (hospice == null || hospice.legacyTrainersProduced >= Settings.legacyLifetimeLimit)
            {
                return;
            }
            SkillRecord best = pawn.skills.skills.Where(s => !s.TotallyDisabled).MaxByWithFallback(s => s.Level);
            if (best == null || best.Level < Settings.legacyMinSkillLevel)
            {
                return;
            }
            ThingDef trainerDef = TrainerFor(best.def);
            if (trainerDef == null || (!forced && !Rand.Chance(ChanceFor(stageIndex, tier, quality))))
            {
                return;
            }
            Thing trainer = ThingMaker.MakeThing(trainerDef);
            if (!GenPlace.TryPlaceThing(trainer, pawn.Position, pawn.Map, ThingPlaceMode.Near))
            {
                return;
            }
            hospice.legacyTrainersProduced++;
            Messages.Message("RimLTC_LegacyProduced".Translate(pawn.Named("PAWN"), best.def.label, trainer.Label),
                trainer, MessageTypeDefOf.PositiveEvent);
        }
    }
}
