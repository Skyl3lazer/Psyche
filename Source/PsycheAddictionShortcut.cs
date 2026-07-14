using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Psyche
{
    public static class PsycheAddictionShortcut
    {
        private static HashSet<ChemicalDef>? qualifyingChemicals;

        private static HashSet<ChemicalDef> QualifyingChemicals
        {
            get
            {
                if (qualifyingChemicals == null)
                {
                    qualifyingChemicals = new HashSet<ChemicalDef>();
                    foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
                    {
                        CompProperties_Drug? props = def.GetCompProperties<CompProperties_Drug>();
                        if (props?.chemical != null && PsycheMedication.Qualifies(def))
                        {
                            qualifyingChemicals.Add(props.chemical);
                        }
                    }
                }

                return qualifyingChemicals;
            }
        }

        public static float FaintCeiling
        {
            get
            {
                List<HediffStage>? stages = PsycheDefOf.Psyche_Scar.stages;
                if (stages != null)
                {
                    for (int i = 0; i < stages.Count; i++)
                    {
                        if (stages[i].minSeverity > 0f)
                        {
                            return stages[i].minSeverity;
                        }
                    }
                }

                return 3f;
            }
        }

        public static void OnAddictionGained(Pawn pawn, Hediff_Addiction addiction)
        {
            if (pawn == null || !PsycheUtility.IsTracked(pawn) || addiction.Chemical == null
                || !QualifyingChemicals.Contains(addiction.Chemical))
            {
                return;
            }

            ReduceWorstScar(pawn);
        }

        public static void ReduceWorstScar(Pawn pawn)
        {
            float ceiling = FaintCeiling;
            Hediff_PsycheScar? worst = null;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                if (hediffs[i] is Hediff_PsycheScar scar && scar.Severity > ceiling
                    && (worst == null || scar.Severity > worst.Severity))
                {
                    worst = scar;
                }
            }

            if (worst == null)
            {
                return;
            }

            worst.Severity = Mathf.Max(0f, ceiling - 0.01f);
            pawn.needs?.TryGetNeed<Need_Psyche>()?.Recompute();
            Messages.Message(
                "Psyche_ScarTradedForAddiction".Translate(pawn.LabelShort, worst.LabelBase).CapitalizeFirst(),
                pawn,
                MessageTypeDefOf.NeutralEvent,
                false);
        }
    }
}
