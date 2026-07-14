using RimWorld;
using Verse;

namespace Psyche
{
    public static class PsycheScarEffects
    {
        public static float MentalStateChanceFactor(Pawn pawn, Trait trait, MentalStateDef state)
        {
            if (pawn == null || state == null)
            {
                return 1f;
            }

            if (state.defName == "FireStartingSpree")
            {
                return PsychePyromania.FireFactor(pawn);
            }

            return 1f;
        }
    }
}
