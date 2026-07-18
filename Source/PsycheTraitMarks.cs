using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Psyche
{
    public static class PsycheTraitMarks
    {
        private static List<HediffDef>? markDefs;

        private static List<HediffDef> MarkDefs
        {
            get
            {
                if (markDefs == null)
                {
                    markDefs = new List<HediffDef>();
                    foreach (HediffDef def in DefDatabase<HediffDef>.AllDefs)
                    {
                        if (def.GetModExtension<PsycheTraitMarkExtension>() != null)
                        {
                            markDefs.Add(def);
                        }
                    }
                }

                return markDefs;
            }
        }

        public static void Reconcile(Pawn pawn)
        {
            if (!PsycheUtility.IsTracked(pawn) || pawn.story?.traits == null)
            {
                return;
            }

            BodyPartRecord? brain = pawn.health.hediffSet.GetBrain();
            if (brain == null)
            {
                return;
            }

            bool seeded = false;
            List<HediffDef> defs = MarkDefs;
            for (int i = 0; i < defs.Count; i++)
            {
                HediffDef def = defs[i];
                PsycheTraitMarkExtension ext = def.GetModExtension<PsycheTraitMarkExtension>();
                if (!HasTrait(pawn, ext) || pawn.health.hediffSet.HasHediff(def))
                {
                    continue;
                }

                Hediff mark = HediffMaker.MakeHediff(def, pawn, brain);
                mark.Severity = ext.seedSeverity;
                if (mark is Hediff_PsycheScar scar)
                {
                    scar.NotePeak();
                    bool alwaysUntreatable = def.GetModExtension<PsycheScarExtension>()?.alwaysUntreatable ?? false;
                    scar.untreatable = alwaysUntreatable || IsTraitFromGene(pawn, ext.trait, ext.degree);
                }

                pawn.health.AddHediff(mark, brain);
                seeded = true;
            }

            if (seeded)
            {
                pawn.needs?.TryGetNeed<Need_Psyche>()?.Recompute();
            }
        }

        public static void RemoveTraitFor(Pawn pawn, PsycheTraitMarkExtension ext)
        {
            Trait? trait = pawn.story?.traits?.GetTrait(ext.trait);
            if (trait != null && trait.Degree == ext.degree)
            {
                pawn.story.traits.RemoveTrait(trait);
            }
        }

        private static bool HasTrait(Pawn pawn, PsycheTraitMarkExtension ext)
        {
            Trait? trait = pawn.story.traits.GetTrait(ext.trait);
            return trait != null && trait.Degree == ext.degree;
        }

        private static bool IsTraitFromGene(Pawn pawn, TraitDef trait, int degree)
        {
            List<Gene>? genes = pawn.genes?.GenesListForReading;
            if (genes == null)
            {
                return false;
            }

            for (int i = 0; i < genes.Count; i++)
            {
                Gene gene = genes[i];
                List<GeneticTraitData>? forced = gene.def.forcedTraits;
                if (!gene.Active || forced == null)
                {
                    continue;
                }

                for (int j = 0; j < forced.Count; j++)
                {
                    if (forced[j].def == trait && forced[j].degree == degree)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
