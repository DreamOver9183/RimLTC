using RimWorld;
using Verse;

namespace RimLTC
{
    public class CompProperties_HospiceSettings : CompProperties
    {
        public CompProperties_HospiceSettings()
        {
            compClass = typeof(CompHospiceSettings);
        }
    }

    /// <summary>
    /// Per-pawn hospice care policy and lifetime legacy counter. Added to BasePawn
    /// by XML patch. Until the player picks a policy, the settings default applies.
    /// </summary>
    public class CompHospiceSettings : ThingComp
    {
        private bool hasCustomCare;
        private MedicalCareCategory hospiceCare = MedicalCareCategory.HerbalOrWorse;
        public int legacyTrainersProduced;

        public MedicalCareCategory HospiceCare
        {
            get => hasCustomCare ? hospiceCare : HospiceCareUtility.Clamp(RimLTCMod.Settings.defaultHospiceCare);
            set
            {
                hospiceCare = HospiceCareUtility.Clamp(value);
                hasCustomCare = true;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref hasCustomCare, "rimLTC_hasCustomCare");
            Scribe_Values.Look(ref hospiceCare, "rimLTC_hospiceCare", MedicalCareCategory.HerbalOrWorse);
            Scribe_Values.Look(ref legacyTrainersProduced, "rimLTC_legacyTrainersProduced");
        }
    }
}
