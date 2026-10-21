using Verse;

namespace RimLTC
{
    public class Hediff_Senescence : HediffWithComps
    {
        private HediffComp_SenescenceCare careComp;

        public HediffComp_SenescenceCare CareComp => careComp ?? (careComp = this.TryGetComp<HediffComp_SenescenceCare>());
    }
}
