using HarmonyLib;
using Verse;

namespace Psyche
{
    [StaticConstructorOnStartup]
    public static class Bootstrap
    {
        static Bootstrap()
        {
            Harmony harmony = new Harmony("Skyl3lazer.Psyche");
            harmony.PatchAll();
            PsycheDubsBreakCompat.Apply(harmony);
        }
    }
}
