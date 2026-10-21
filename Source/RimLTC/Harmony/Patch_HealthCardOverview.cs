using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimLTC
{
    /// <summary>
    /// Adds the hospice policy row to Health > Overview, directly under the vanilla
    /// medical care button. The transpiler wraps the medical care button call and
    /// pushes the rest of the tab down by one row. If that call cannot be found
    /// (a future game version or another mod), the row is appended at the end of
    /// the overview instead.
    /// </summary>
    [HarmonyPatch(typeof(HealthCardUtility), "DrawOverviewTab")]
    public static class Patch_HealthCardOverview
    {
        private const float RowGap = 4f;
        private const float RowHeight = HospiceCareUI.ButtonHeight;

        private static bool injectedInline;
        private static Pawn currentPawn;
        private static Rect currentRect;
        private static float pendingOffset;

        public static void Prefix(Rect rect, Pawn pawn)
        {
            currentPawn = pawn;
            currentRect = rect;
            pendingOffset = 0f;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            MethodInfo selectButton = AccessTools.Method(typeof(MedicalCareUtility), nameof(MedicalCareUtility.MedicalCareSelectButton));
            MethodInfo setter = AccessTools.Method(typeof(MedicalCareUtility), nameof(MedicalCareUtility.MedicalCareSetter));
            MethodInfo selectButtonWrapper = AccessTools.Method(typeof(Patch_HealthCardOverview), nameof(MedicalCareSelectButtonWithHospice));
            MethodInfo setterWrapper = AccessTools.Method(typeof(Patch_HealthCardOverview), nameof(MedicalCareSetterWithHospice));
            MethodInfo applyOffset = AccessTools.Method(typeof(Patch_HealthCardOverview), nameof(ApplyPendingOffset));

            ParameterInfo curYParam = original.GetParameters().LastOrDefault(p => p.ParameterType == typeof(float));
            int curYArg = curYParam == null ? -1 : curYParam.Position + (original.IsStatic ? 0 : 1);

            injectedInline = false;
            foreach (CodeInstruction instruction in instructions)
            {
                if (!injectedInline && curYArg >= 0 && (instruction.Calls(selectButton) || instruction.Calls(setter)))
                {
                    MethodInfo wrapper = instruction.Calls(selectButton) ? selectButtonWrapper : setterWrapper;
                    CodeInstruction call = new CodeInstruction(OpCodes.Call, wrapper);
                    instruction.MoveLabelsTo(call);
                    instruction.MoveBlocksTo(call);
                    yield return call;
                    yield return CodeInstruction.LoadArgument(curYArg, true);
                    yield return new CodeInstruction(OpCodes.Call, applyOffset);
                    injectedInline = true;
                    continue;
                }
                yield return instruction;
            }
            if (!injectedInline)
            {
                Log.Message("[RimLTC] Medical care button not found in the health overview; the hospice care row is drawn at the end of the overview instead.");
            }
        }

        public static void Postfix(Rect rect, Pawn pawn, ref float __result)
        {
            if (!injectedInline && HospiceCareUI.ShouldShowInHealthTab(pawn))
            {
                Rect row = new Rect(rect.x, __result + RowGap, rect.width, RowHeight);
                HospiceCareUI.DrawLabelledRow(row, pawn, MedicalCareUtility.CareSetterWidth);
                __result += RowHeight + RowGap;
            }
            currentPawn = null;
        }

        public static void MedicalCareSelectButtonWithHospice(Rect rect, Pawn pawn)
        {
            MedicalCareUtility.MedicalCareSelectButton(rect, pawn);
            DrawInlineRow(rect);
        }

        public static void MedicalCareSetterWithHospice(Rect rect, ref MedicalCareCategory medCare)
        {
            MedicalCareUtility.MedicalCareSetter(rect, ref medCare);
            DrawInlineRow(rect);
        }

        /// <summary>Draws the row under the medical care button, aligned with it.</summary>
        private static void DrawInlineRow(Rect medicalRect)
        {
            if (!HospiceCareUI.ShouldShowInHealthTab(currentPawn))
            {
                return;
            }
            float buttonWidth = Mathf.Max(medicalRect.width, MedicalCareUtility.CareSetterWidth);
            float right = Mathf.Max(medicalRect.xMax, currentRect.x + buttonWidth);
            Rect row = new Rect(currentRect.x, medicalRect.yMax + RowGap, right - currentRect.x, RowHeight);
            HospiceCareUI.DrawLabelledRow(row, currentPawn, buttonWidth);
            pendingOffset += RowHeight + RowGap;
        }

        public static void ApplyPendingOffset(ref float curY)
        {
            curY += pendingOffset;
            pendingOffset = 0f;
        }
    }
}
