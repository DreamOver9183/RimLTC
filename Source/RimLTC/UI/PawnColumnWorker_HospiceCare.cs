using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimLTC
{
    /// <summary>
    /// Hospice care column for the Assign tab. Left-clicking the header sets the
    /// policy for every listed pawn; right-clicking sorts.
    /// </summary>
    public class PawnColumnWorker_HospiceCare : PawnColumnWorker
    {
        private const int Width = 160;

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (!AppliesTo(pawn))
            {
                return;
            }
            float height = Mathf.Min(HospiceCareUI.ButtonHeight, rect.height - 2f);
            Rect buttonRect = new Rect(rect.x, rect.y + (rect.height - height) / 2f, rect.width - 4f, height);
            HospiceCareUI.DrawSelectButton(buttonRect, pawn);
        }

        public override int GetMinWidth(PawnTable table) => Mathf.Max(base.GetMinWidth(table), Width);

        public override int GetOptimalWidth(PawnTable table) => Mathf.Clamp(Width, GetMinWidth(table), GetMaxWidth(table));

        public override int GetMaxWidth(PawnTable table) => Mathf.Min(base.GetMaxWidth(table), Width + 40);

        public override int Compare(Pawn a, Pawn b) => SortValue(a).CompareTo(SortValue(b));

        protected override void HeaderClicked(Rect headerRect, PawnTable table)
        {
            if (Event.current.button == 0)
            {
                HospiceCareUI.OpenBatchMenu(table.PawnsListForReading.Where(AppliesTo));
                return;
            }
            base.HeaderClicked(headerRect, table);
        }

        protected override string GetHeaderTip(PawnTable table)
        {
            return base.GetHeaderTip(table) + "\n\n" + "RimLTC_HospiceCare_HeaderTip".Translate();
        }

        private static bool AppliesTo(Pawn pawn)
        {
            return pawn.TryGetComp<CompHospiceSettings>() != null && SenescenceUtility.IsCandidate(pawn);
        }

        private static int SortValue(Pawn pawn)
        {
            return AppliesTo(pawn) ? (int)HospiceCareUtility.GetHospiceCare(pawn) : -1;
        }
    }
}
