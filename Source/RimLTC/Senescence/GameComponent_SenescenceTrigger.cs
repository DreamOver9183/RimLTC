using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RimLTC
{
    /// <summary>
    /// Hourly scan that gives natural senescence to pawns who reached their
    /// personal trigger age. Instantiated automatically by the game.
    /// </summary>
    public class GameComponent_SenescenceTrigger : GameComponent
    {
        private const int CheckIntervalTicks = GenDate.TicksPerHour;

        private static readonly List<Pawn> tmpPawns = new List<Pawn>();

        public GameComponent_SenescenceTrigger(Game game)
        {
        }

        public override void GameComponentTick()
        {
            if (Find.TickManager.TicksGame % CheckIntervalTicks != 0)
            {
                return;
            }
            CheckAllPawns();
        }

        public static void CheckAllPawns()
        {
            tmpPawns.Clear();
            tmpPawns.AddRange(PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction);
            if (RimLTCMod.Settings.applyToPrisoners)
            {
                tmpPawns.AddRange(PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_PrisonersOfColony);
            }
            foreach (Pawn pawn in tmpPawns)
            {
                if (SenescenceUtility.ShouldDevelopSenescence(pawn))
                {
                    SenescenceUtility.AddSenescence(pawn);
                }
            }
            tmpPawns.Clear();
        }
    }
}
