using HarmonyLib;
using RimWorld;
using Verse;

namespace CM_Categorized_Bills
{
    public class CategorizedBillsMod : Mod
    {
        private static CategorizedBillsMod _instance;
        public static CategorizedBillsMod Instance => _instance;

        public CategorizedBillsMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("CM_Categorized_Bills");
            harmony.PatchAll();

            _instance = this;
        }
    }
}
