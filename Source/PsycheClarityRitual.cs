using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Psyche
{
    public static class PsycheClarityRitual
    {
        private static bool resolved;
        private static PreceptDef? preceptDef;
        private static RitualPatternDef? patternDef;

        private static void Resolve()
        {
            if (resolved)
            {
                return;
            }

            resolved = true;
            preceptDef = DefDatabase<PreceptDef>.GetNamedSilentFail("Psyche_ClarityContemplation");
            patternDef = DefDatabase<RitualPatternDef>.GetNamedSilentFail("Psyche_ClarityContemplation");
        }

        public static bool TryBegin(Pawn pawn)
        {
            if (!ModsConfig.IdeologyActive || pawn.Map == null)
            {
                return false;
            }

            Resolve();
            if (preceptDef == null || patternDef == null)
            {
                return false;
            }

            try
            {
                Ideo ideo = pawn.Ideo;
                if (ideo == null)
                {
                    return false;
                }

                Precept_Ritual? ritual = ideo.GetPrecept(preceptDef) as Precept_Ritual;
                if (ritual == null)
                {
                    Precept_Ritual made = (Precept_Ritual)PreceptMaker.MakePrecept(preceptDef);
                    ideo.AddPrecept(made, true, null, patternDef);
                    ritual = made;
                }

                if (ritual?.behavior == null)
                {
                    return false;
                }

                IntVec3 spot = PsycheTherapy.PickRendezvous(pawn);
                if (!spot.IsValid)
                {
                    spot = pawn.Position;
                }

                TargetInfo target = new TargetInfo(spot, pawn.Map);
                Dictionary<string, Pawn> forced = new Dictionary<string, Pawn> { { "seeker", pawn } };
                ritual.ShowRitualBeginWindow(target, null, pawn, forced);
                return true;
            }
            catch (Exception e)
            {
                Log.Warning("[Psyche] Clarity ritual failed to open; falling back to solo. " + e);
                return false;
            }
        }
    }
}
