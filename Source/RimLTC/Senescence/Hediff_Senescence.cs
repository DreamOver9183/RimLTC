using RimWorld;
using Verse;

namespace RimLTC
{
    public class Hediff_Senescence : HediffWithComps
    {
        private HediffComp_SenescenceCare careComp;

        public HediffComp_SenescenceCare CareComp => careComp ?? (careComp = this.TryGetComp<HediffComp_SenescenceCare>());

        /// <summary>
        /// "No care" hospice policy: doctors never tend senescence itself, while
        /// wounds and diseases still follow the vanilla medical policy.
        /// </summary>
        public override bool TendableNow(bool ignoreTimer = false)
        {
            if (HospiceCareUtility.GetHospiceCare(pawn) == MedicalCareCategory.NoCare)
            {
                return false;
            }
            return base.TendableNow(ignoreTimer);
        }
    }
}
