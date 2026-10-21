using System.Linq;
using RimWorld;
using Verse;

namespace RimLTC
{
    /// <summary>Four-tier passing thoughts given to the colony when an elder dies.</summary>
    public static class PassingUtility
    {
        private const int PeacefulNegativeStage = 0;
        private const int PeacefulPositiveStage = 1;

        public static bool IsColonyMember(Pawn pawn)
        {
            return pawn.RaceProps.Humanlike && pawn.Faction != null && pawn.Faction.IsPlayer;
        }

        public static ThoughtDef ThoughtFor(float satisfaction, out int stage)
        {
            stage = 0;
            if (satisfaction < CareSatisfactionUtility.RegretfulThreshold)
            {
                return RimLTCDefOf.RimLTC_NeglectedPassing;
            }
            if (satisfaction < CareSatisfactionUtility.PeacefulThreshold)
            {
                return RimLTCDefOf.RimLTC_RegretfulPassing;
            }
            if (satisfaction < CareSatisfactionUtility.TranscendentThreshold)
            {
                stage = satisfaction < CareSatisfactionUtility.PeacefulPositiveThreshold
                    ? PeacefulNegativeStage
                    : PeacefulPositiveStage;
                return RimLTCDefOf.RimLTC_PeacefulPassing;
            }
            return RimLTCDefOf.RimLTC_TranscendentPassing;
        }

        public static void GivePassingThoughts(Pawn deceased, float satisfaction)
        {
            ThoughtDef def = ThoughtFor(satisfaction, out int stage);
            // Copy: PawnsFinder returns a shared cached list.
            foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_Colonists.ToList())
            {
                if (pawn == deceased || pawn.needs?.mood?.thoughts?.memories == null)
                {
                    continue;
                }
                pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(def, stage));
            }
            MessageTypeDef messageType = def == RimLTCDefOf.RimLTC_NeglectedPassing ? MessageTypeDefOf.NegativeEvent
                : def == RimLTCDefOf.RimLTC_TranscendentPassing ? MessageTypeDefOf.PositiveEvent
                : MessageTypeDefOf.NeutralEvent;
            Messages.Message(("RimLTC_PassingMessage_" + def.defName).Translate(deceased.Named("PAWN")),
                new LookTargets(deceased.Corpse ?? (Thing)deceased), messageType);
        }

        /// <summary>Writes the mood values from the settings into the thought defs.</summary>
        public static void ApplyMoodSettings()
        {
            RimLTCSettings settings = RimLTCMod.Settings;
            RimLTCDefOf.RimLTC_NeglectedPassing.stages[0].baseMoodEffect = settings.moodNeglected;
            RimLTCDefOf.RimLTC_RegretfulPassing.stages[0].baseMoodEffect = settings.moodRegretful;
            RimLTCDefOf.RimLTC_PeacefulPassing.stages[PeacefulNegativeStage].baseMoodEffect = -settings.moodPeaceful;
            RimLTCDefOf.RimLTC_PeacefulPassing.stages[PeacefulPositiveStage].baseMoodEffect = settings.moodPeaceful;
            RimLTCDefOf.RimLTC_TranscendentPassing.stages[0].baseMoodEffect = settings.moodTranscendent;
        }
    }
}
