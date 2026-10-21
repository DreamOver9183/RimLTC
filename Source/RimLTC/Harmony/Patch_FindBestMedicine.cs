using HarmonyLib;
using RimWorld;
using Verse;

namespace RimLTC
{
    /// <summary>
    /// When senescence is the only thing that needs tending, medicine is chosen by
    /// the hospice policy instead of the medical policy. The vanilla search runs
    /// unchanged with the hospice tier swapped in, then the policy is restored.
    /// </summary>
    [HarmonyPatch(typeof(HealthAIUtility), nameof(HealthAIUtility.FindBestMedicine))]
    public static class Patch_FindBestMedicine
    {
        public static void Prefix(Pawn patient, out MedicalCareCategory? __state)
        {
            __state = null;
            if (patient?.playerSettings == null
                || !HospiceCareUtility.OnlyNeedsHospiceCare(patient, out MedicalCareCategory hospiceCare))
            {
                return;
            }
            __state = patient.playerSettings.medCare;
            patient.playerSettings.medCare = hospiceCare;
        }

        public static void Finalizer(Pawn patient, MedicalCareCategory? __state)
        {
            if (__state.HasValue && patient?.playerSettings != null)
            {
                patient.playerSettings.medCare = __state.Value;
            }
        }
    }
}
