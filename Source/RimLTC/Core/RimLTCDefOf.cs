using RimWorld;
using Verse;

namespace RimLTC
{
    [DefOf]
    public static class RimLTCDefOf
    {
        public static HediffDef RimLTC_NaturalSenescence;

        static RimLTCDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(RimLTCDefOf));
        }
    }
}
