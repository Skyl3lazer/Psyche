using RimWorld;
using UnityEngine;
using Verse;

namespace Psyche
{
    public class StatPart_PsycheThreshold : StatPart
    {
        public static bool SuppressForInnateRead;

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (SuppressForInnateRead || !req.HasThing)
            {
                return;
            }

            if (req.Thing is Pawn pawn)
            {
                Need_Psyche? need = pawn.needs?.TryGetNeed<Need_Psyche>();
                if (need != null)
                {
                    val += need.ThresholdOffset;
                }
            }
        }

        public override string? ExplanationPart(StatRequest req)
        {
            if (req.Thing is Pawn pawn)
            {
                Need_Psyche? need = pawn.needs?.TryGetNeed<Need_Psyche>();
                if (need != null && need.InnateCached)
                {
                    float offset = need.ThresholdOffset;
                    if (Mathf.Abs(offset) > 0.0001f)
                    {
                        return "Psyche (" + need.CurLevel.ToStringPercent() + "): " + offset.ToStringWithSign("0.###");
                    }
                }
            }

            return null;
        }
    }
}
