using RimWorld;
using UnityEngine;
using Verse;

namespace RimLTC
{
    /// <summary>
    /// Rules for the hidden care satisfaction score (0-100). It is accumulated
    /// over the whole senescence, so last-minute care cannot buy a good passing.
    /// </summary>
    public static class CareSatisfactionUtility
    {
        public const float Initial = 45f;
        public const float Max = 100f;

        public const float RegretfulThreshold = 30f;
        public const float PeacefulThreshold = 60f;
        public const float PeacefulPositiveThreshold = 75f;
        public const float TranscendentThreshold = 90f;

        private const float HandsCap = 55f;
        private const float HerbalCap = 85f;

        /// <summary>Share of the distance to the tier cap closed by one perfect tend.</summary>
        private const float ConvergenceRate = 0.15f;

        private const float UntendedPenalty = 4f;
        private const float OnGroundPenalty = 4f;
        private const float FilthPenalty = 2f;
        private const float MaxPenaltyPerCheck = 8f;

        private const float ComfortableBedThreshold = 0.75f;
        private const float FilthCheckRadius = 2.9f;

        /// <summary>
        /// The score a tier converges to. Only industrial care with a comfortable
        /// bed and a clean room can go past the herbal cap.
        /// </summary>
        public static float Cap(MedicalCareCategory tier, Pawn pawn)
        {
            switch (HospiceCareUtility.Clamp(tier))
            {
                case MedicalCareCategory.NoMeds:
                    return HandsCap;
                case MedicalCareCategory.HerbalOrWorse:
                    return HerbalCap;
                case MedicalCareCategory.NormalOrWorse:
                    return HasGoodEnvironment(pawn) ? Max : HerbalCap;
                default:
                    return 0f;
            }
        }

        /// <summary>New score after one tend of the given tier and quality.</summary>
        public static float AfterTend(float satisfaction, MedicalCareCategory tier, float quality, Pawn pawn)
        {
            float cap = Cap(tier, pawn);
            if (satisfaction < cap)
            {
                float gain = (cap - satisfaction) * ConvergenceRate * Mathf.Clamp01(quality) * RimLTCMod.Settings.satisfactionMultiplier;
                return Mathf.Min(cap, satisfaction + gain);
            }
            // A lower tier than before slowly pulls the score back down to its cap.
            return satisfaction - (satisfaction - cap) * ConvergenceRate * 0.5f;
        }

        /// <summary>Half-daily penalty for neglect: untended, lying on the ground, filth around.</summary>
        public static float NeglectPenalty(Pawn pawn, bool caredFor)
        {
            float penalty = 0f;
            if (!caredFor)
            {
                penalty += UntendedPenalty;
            }
            if (pawn.Spawned && pawn.Downed && !pawn.InBed())
            {
                penalty += OnGroundPenalty;
            }
            if (HasFilthNearby(pawn))
            {
                penalty += FilthPenalty;
            }
            return Mathf.Min(penalty, MaxPenaltyPerCheck);
        }

        public static bool HasGoodEnvironment(Pawn pawn)
        {
            Building_Bed bed = pawn.CurrentBed();
            return bed != null
                && bed.GetStatValue(StatDefOf.Comfort) >= ComfortableBedThreshold
                && !HasFilthNearby(pawn);
        }

        public static bool HasFilthNearby(Pawn pawn)
        {
            if (!pawn.Spawned)
            {
                return false;
            }
            Map map = pawn.Map;
            Room room = pawn.GetRoom();
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(pawn.Position, FilthCheckRadius, true))
            {
                if (!cell.InBounds(map) || (room != null && cell.GetRoom(map) != room))
                {
                    continue;
                }
                foreach (Thing thing in cell.GetThingList(map))
                {
                    if (thing is Filth)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
