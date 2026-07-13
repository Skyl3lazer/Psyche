using HarmonyLib;
using Verse;

namespace Psyche
{
    [StaticConstructorOnStartup]
    public static class Bootstrap
    {
        static Bootstrap()
        {
            new Harmony("Skyl3lazer.Psyche").PatchAll();
        }
    }
}
