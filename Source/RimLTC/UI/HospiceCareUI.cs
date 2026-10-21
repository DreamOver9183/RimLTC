using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimLTC
{
    /// <summary>
    /// Hospice policy widgets. They reuse the vanilla medical care icons and the
    /// vanilla dropdown, so they look and behave like the medical care button.
    /// </summary>
    public static class HospiceCareUI
    {
        public const float ButtonHeight = 28f;

        public static readonly MedicalCareCategory[] Tiers =
        {
            MedicalCareCategory.NoCare,
            MedicalCareCategory.NoMeds,
            MedicalCareCategory.HerbalOrWorse,
            MedicalCareCategory.NormalOrWorse
        };

        private static Texture2D[] icons;

        public static Texture2D Icon(MedicalCareCategory tier)
        {
            if (icons == null)
            {
                icons = new[]
                {
                    ContentFinder<Texture2D>.Get("UI/Icons/Medical/NoCare"),
                    ContentFinder<Texture2D>.Get("UI/Icons/Medical/NoMeds"),
                    ThingDefOf.MedicineHerbal.uiIcon,
                    ThingDefOf.MedicineIndustrial.uiIcon
                };
            }
            return icons[(int)HospiceCareUtility.Clamp(tier)];
        }

        public static string Label(MedicalCareCategory tier)
        {
            return ("RimLTC_HospiceCare_" + HospiceCareUtility.Clamp(tier)).Translate();
        }

        public static string Description(MedicalCareCategory tier)
        {
            return ("RimLTC_HospiceCare_" + HospiceCareUtility.Clamp(tier) + "_Desc").Translate();
        }

        /// <summary>Hospice policy matters for pawns that are, or will soon be, elders.</summary>
        public static bool ShouldShowInHealthTab(Pawn pawn)
        {
            if (Current.ProgramState != ProgramState.Playing || pawn == null || pawn.Dead
                || pawn.TryGetComp<CompHospiceSettings>() == null || !SenescenceUtility.IsCandidate(pawn))
            {
                return false;
            }
            return SenescenceUtility.GetSenescence(pawn) != null
                || pawn.ageTracker.AgeBiologicalYearsFloat >= SenescenceUtility.TriggerAge(pawn) * 0.75f;
        }

        /// <summary>
        /// Icon plus label button. The vanilla dropdown is laid over it with a clear
        /// texture, so clicking and drag-painting work exactly like vanilla.
        /// </summary>
        public static void DrawSelectButton(Rect rect, Pawn pawn)
        {
            MedicalCareCategory current = HospiceCareUtility.GetHospiceCare(pawn);
            Widgets.DrawAtlas(rect, Widgets.ButtonSubtleAtlas);
            float iconSize = Mathf.Min(24f, rect.height - 4f);
            Rect iconRect = new Rect(rect.x + 3f, rect.y + (rect.height - iconSize) / 2f, iconSize, iconSize);
            GUI.DrawTexture(iconRect, Icon(current));
            Rect labelRect = new Rect(iconRect.xMax + 4f, rect.y, rect.xMax - iconRect.xMax - 6f, rect.height);
            if (labelRect.width > 20f)
            {
                TextAnchor anchor = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(labelRect, Label(current).Truncate(labelRect.width));
                Text.Anchor = anchor;
            }
            Widgets.Dropdown(rect, pawn,
                p => HospiceCareUtility.GetHospiceCare(p),
                GenerateMenu,
                null,
                BaseContent.ClearTex,
                Label(current),
                Icon(current),
                paintable: true);
            TooltipHandler.TipRegion(rect, () =>
                "RimLTC_HospiceCare".Translate() + ": " + Label(HospiceCareUtility.GetHospiceCare(pawn))
                + "\n\n" + Description(HospiceCareUtility.GetHospiceCare(pawn)), pawn.thingIDNumber ^ 0x4C5443);
        }

        /// <summary>A labelled row: "Hospice care" on the left, the select button on the right.</summary>
        public static void DrawLabelledRow(Rect row, Pawn pawn, float buttonWidth)
        {
            buttonWidth = Mathf.Min(buttonWidth, row.width);
            Rect buttonRect = new Rect(row.xMax - buttonWidth, row.y, buttonWidth, ButtonHeight);
            Rect labelRect = new Rect(row.x, row.y, row.width - buttonWidth - 4f, ButtonHeight);
            if (labelRect.width > 30f)
            {
                TextAnchor anchor = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(labelRect, "RimLTC_HospiceCare".Translate().Truncate(labelRect.width));
                Text.Anchor = anchor;
            }
            DrawSelectButton(buttonRect, pawn);
        }

        private static IEnumerable<Widgets.DropdownMenuElement<MedicalCareCategory>> GenerateMenu(Pawn pawn)
        {
            foreach (MedicalCareCategory tier in Tiers)
            {
                MedicalCareCategory chosen = tier;
                yield return new Widgets.DropdownMenuElement<MedicalCareCategory>
                {
                    option = new FloatMenuOption(Label(chosen),
                        () => HospiceCareUtility.SetHospiceCare(pawn, chosen),
                        Icon(chosen), Color.white),
                    payload = chosen
                };
            }
        }

        /// <summary>Float menu that applies one tier to many pawns at once.</summary>
        public static void OpenBatchMenu(IEnumerable<Pawn> pawns)
        {
            List<Pawn> targets = new List<Pawn>(pawns);
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (MedicalCareCategory tier in Tiers)
            {
                MedicalCareCategory chosen = tier;
                options.Add(new FloatMenuOption("RimLTC_HospiceCare_SetAll".Translate(Label(chosen)), () =>
                {
                    foreach (Pawn pawn in targets)
                    {
                        HospiceCareUtility.SetHospiceCare(pawn, chosen);
                    }
                }, Icon(chosen), Color.white));
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }
    }
}
