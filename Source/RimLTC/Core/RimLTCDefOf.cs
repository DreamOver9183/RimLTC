using RimWorld;
using Verse;

namespace RimLTC
{
    [DefOf]
    public static class RimLTCDefOf
    {
        public static HediffDef RimLTC_NaturalSenescence;

        public static ThoughtDef RimLTC_NeglectedPassing;
        public static ThoughtDef RimLTC_RegretfulPassing;
        public static ThoughtDef RimLTC_PeacefulPassing;
        public static ThoughtDef RimLTC_TranscendentPassing;

        static RimLTCDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(RimLTCDefOf));
        }
    }
}
