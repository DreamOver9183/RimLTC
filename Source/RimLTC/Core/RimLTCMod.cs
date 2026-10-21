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
    }
}
