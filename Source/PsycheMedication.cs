using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Psyche
{
    public static class PsycheMedication
    {
        public static bool Qualifies(ThingDef drug)
        {
            PsycheMitigationExtension? ext = drug.GetModExtension<PsycheMitigationExtension>();
            if (ext != null)
            {
                return ext.mitigates;
            }

            return drug.ingestible?.drugCategory == DrugCategory.Social;
        }

        public static float Potency(ThingDef drug) =>
            drug.GetModExtension<PsycheMitigationExtension>()?.potency ?? 1f;

        public static void OnIngested(Pawn pawn, ThingDef drug)
        {
            if (pawn == null || drug == null || !PsycheUtility.IsTracked(pawn) || !Qualifies(drug))
            {
                return;
            }

            List<Thought_Memory>? memories = pawn.needs?.mood?.thoughts?.memories?.Memories;
            if (memories == null)
            {
                return;
            }

            float potency = Potency(drug);
            for (int i = 0; i < memories.Count; i++)
            {
                if (memories[i] is Thought_Psychlet pt && !pt.IsBoon)
                {
                    pt.Medicate(potency);
                }
            }
        }
    }
}
