using UnityEngine;
using Verse;

namespace Psyche
{
    public static class PsychePyromania
    {
        public static float FireFactor(Pawn pawn)
        {
            Hediff? scar = pawn.health?.hediffSet?.GetFirstHediffOfDef(PsycheDefOf.Psyche_Scar_Pyromaniac);
            if (scar == null)
            {
                return 1f;
            }

            float[] factors = PsycheTuning.PyromaniaFireFactorByStage;
            int index = Mathf.Clamp(scar.CurStageIndex, 0, factors.Length - 1);
            return factors[index];
        }
    }
}
