using LudeonTK;
using RimWorld;
using Verse;

namespace RimLTC
{
    /// <summary>Dev mode tools for testing senescence without waiting for decades.</summary>
    public static class DebugActions_RimLTC
    {
        private const string Category = "RimLTC";

        [DebugAction(Category, "Add senescence", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void AddSenescence(Pawn p)
        {
            if (SenescenceUtility.GetSenescence(p) == null)
            {
                SenescenceUtility.AddSenescence(p);
            }
        }

        [DebugAction(Category, "Remove senescence", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void RemoveSenescence(Pawn p)
        {
            Hediff_Senescence hediff = SenescenceUtility.GetSenescence(p);
            if (hediff != null)
            {
                p.health.RemoveHediff(hediff);
            }
        }

        [DebugAction(Category, "Senescence: early frailty (0.20)", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SetStage1(Pawn p) => SetSeverity(p, 0.20f);

        [DebugAction(Category, "Senescence: bedridden (0.50)", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SetStage2(Pawn p) => SetSeverity(p, 0.50f);

        [DebugAction(Category, "Senescence: end of life (0.95)", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SetStage3(Pawn p) => SetSeverity(p, 0.95f);

        [DebugAction(Category, "Care satisfaction: 20", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SetSatisfaction20(Pawn p) => SetSatisfaction(p, 20f);

        [DebugAction(Category, "Care satisfaction: 50", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SetSatisfaction50(Pawn p) => SetSatisfaction(p, 50f);

        [DebugAction(Category, "Care satisfaction: 80", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SetSatisfaction80(Pawn p) => SetSatisfaction(p, 80f);

        [DebugAction(Category, "Care satisfaction: 95", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SetSatisfaction95(Pawn p) => SetSatisfaction(p, 95f);

        [DebugAction(Category, "Log care state", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void LogCareState(Pawn p)
        {
            HediffComp_SenescenceCare care = SenescenceUtility.GetSenescence(p)?.CareComp;
            string state = care == null ? "no senescence" : care.CompDebugString();
            Log.Message("[RimLTC] " + p.LabelShort
                + "\nage: " + p.ageTracker.AgeBiologicalYearsFloat.ToString("F1")
                + " / trigger age: " + SenescenceUtility.TriggerAge(p).ToString("F1")
                + "\nhospice care: " + HospiceCareUtility.GetHospiceCare(p)
                + "\ngene immune: " + SenescenceUtility.IsGeneImmune(p)
                + "\n" + state);
        }

        [DebugAction(Category, "Force legacy skilltrainer", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ForceLegacy(Pawn p)
        {
            Hediff_Senescence hediff = SenescenceUtility.GetSenescence(p);
            int stage = hediff?.CurStageIndex ?? 0;
            LegacyUtility.TryProduceLegacy(p, stage, MedicalCareCategory.NormalOrWorse, 1f, forced: true);
        }

        [DebugAction(Category, "Run age trigger scan now", allowedGameStates = AllowedGameStates.Playing)]
        private static void RunTriggerScan()
        {
            GameComponent_SenescenceTrigger.CheckAllPawns();
        }

        private static void SetSeverity(Pawn p, float severity)
        {
            Hediff_Senescence hediff = SenescenceUtility.GetSenescence(p);
            if (hediff == null)
            {
                SenescenceUtility.AddSenescence(p);
                hediff = SenescenceUtility.GetSenescence(p);
            }
            if (hediff != null)
            {
                hediff.Severity = severity;
            }
        }

        private static void SetSatisfaction(Pawn p, float value)
        {
            HediffComp_SenescenceCare care = SenescenceUtility.GetSenescence(p)?.CareComp;
            if (care != null)
            {
                care.careSatisfaction = value;
            }
        }
    }
}
