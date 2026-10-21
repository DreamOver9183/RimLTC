using UnityEngine;
using Verse;

namespace RimLTC
{
    public class RimLTCMod : Mod
    {
        public static RimLTCSettings Settings { get; private set; }

        public RimLTCMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<RimLTCSettings>();
        }

        public override string SettingsCategory()
        {
            return "RimLTC_SettingsCategory".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            SettingsWindow.Draw(inRect, Settings);
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            if (RimLTCDefOf.RimLTC_NeglectedPassing != null)
            {
                PassingUtility.ApplyMoodSettings();
            }
        }
    }
}
