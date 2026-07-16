using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Psyche
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.GetGizmos))]
    public static class Patch_PawnGetGizmos
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            foreach (Gizmo gizmo in __result)
            {
                yield return gizmo;
            }

            Command_Action? seek = PsycheClaritySeeking.GizmoFor(__instance);
            if (seek != null)
            {
                yield return seek;
            }

            Command_Action? devSeek = PsycheClaritySeeking.DevGizmoFor(__instance);
            if (devSeek != null)
            {
                yield return devSeek;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_NeedsTracker), "ShouldHaveNeed")]
    public static class Patch_ShouldHaveNeed
    {
        private static readonly FieldInfo PawnField = AccessTools.Field(typeof(Pawn_NeedsTracker), "pawn");

        public static void Postfix(Pawn_NeedsTracker __instance, NeedDef __0, ref bool __result)
        {
            if (!__result || __0 != PsycheDefOf.Psyche_Core || PawnField == null)
            {
                return;
            }

            Pawn? pawn = PawnField.GetValue(__instance) as Pawn;
            if (pawn == null || !PsycheUtility.IsTracked(pawn) || pawn.health?.hediffSet?.GetBrain() == null)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.SetFaction))]
    public static class Patch_SetFaction
    {
        public static void Postfix(Pawn __instance)
        {
            if (__instance.needs != null && __instance.RaceProps.Humanlike)
            {
                __instance.needs.AddOrRemoveNeedsAsAppropriate();
            }

            PsycheTraitMarks.Reconcile(__instance);
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.SpawnSetup))]
    public static class Patch_SpawnSetup
    {
        public static void Postfix(Pawn __instance)
        {
            PsycheTraitMarks.Reconcile(__instance);
        }
    }

    [HarmonyPatch(typeof(TraitMentalStateGiver), nameof(TraitMentalStateGiver.CheckGive))]
    public static class Patch_TraitMentalStateThrottle
    {
        public static bool Prefix(TraitMentalStateGiver __instance, Pawn pawn, ref bool __result)
        {
            TraitDegreeData? data = __instance.traitDegreeData;
            MentalStateDef? state = data?.randomMentalState ?? data?.forcedMentalState;
            if (state == null)
            {
                return true;
            }

            float factor = PsycheScarEffects.MentalStateChanceFactor(pawn, TraitFor(pawn, data), state);
            if (factor < 1f && Rand.Value >= factor)
            {
                __result = false;
                return false;
            }

            return true;
        }

        private static Trait? TraitFor(Pawn pawn, TraitDegreeData data)
        {
            List<Trait>? traits = pawn.story?.traits?.allTraits;
            if (traits == null)
            {
                return null;
            }

            for (int i = 0; i < traits.Count; i++)
            {
                if (traits[i].CurrentData == data)
                {
                    return traits[i];
                }
            }

            return null;
        }
    }

    [HarmonyPatch(typeof(TraitSet), nameof(TraitSet.GainTrait))]
    public static class Patch_GainTrait
    {
        private static readonly FieldInfo PawnField = AccessTools.Field(typeof(TraitSet), "pawn");

        public static void Postfix(TraitSet __instance)
        {
            if (PawnField?.GetValue(__instance) is Pawn pawn)
            {
                PsycheTraitMarks.Reconcile(pawn);
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.AddHediff), new[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageWorker.DamageResult) })]
    public static class Patch_LimbLost
    {
        public static void Postfix(Hediff hediff, DamageInfo? dinfo)
        {
            if (hediff is Hediff_MissingPart && hediff.pawn != null && dinfo.HasValue
                && dinfo.Value.Def != null && dinfo.Value.Def.ExternalViolenceFor(hediff.pawn))
            {
                PsycheOwnTriggers.Fire(hediff.pawn, PsycheDefOf.Psyche_OT_Amputation);
                PsycheOwnTriggers.FireOnRelations(hediff.pawn, PsycheDefOf.Psyche_OT_AllyWounded);
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker), "MakeDowned")]
    public static class Patch_Downed
    {
        private static readonly FieldInfo PawnField = AccessTools.Field(typeof(Pawn_HealthTracker), "pawn");

        public static void Postfix(Pawn_HealthTracker __instance, DamageInfo? dinfo)
        {
            if (PawnField?.GetValue(__instance) is not Pawn pawn)
            {
                return;
            }

            if (dinfo.HasValue && dinfo.Value.Def != null && dinfo.Value.Def.ExternalViolenceFor(pawn))
            {
                PsycheOwnTriggers.Fire(pawn, PsycheDefOf.Psyche_OT_CombatTrauma);
                PsycheOwnTriggers.FireOnRelations(pawn, PsycheDefOf.Psyche_OT_AllyWounded);
            }
        }
    }

    [HarmonyPatch(typeof(GenRecipe), "PostProcessProduct")]
    public static class Patch_MasterworkCrafted
    {
        public static void Postfix(Thing __result, Pawn worker)
        {
            if (__result == null || worker == null || !PsycheUtility.IsTracked(worker))
            {
                return;
            }

            CompQuality? cq = __result.TryGetComp<CompQuality>();
            if (cq == null)
            {
                return;
            }

            if (cq.Quality == QualityCategory.Legendary)
            {
                PsycheOwnTriggers.Fire(worker, PsycheDefOf.Psyche_OT_Legendary);
            }
            else if (cq.Quality == QualityCategory.Masterwork)
            {
                PsycheOwnTriggers.Fire(worker, PsycheDefOf.Psyche_OT_Masterwork);
            }
        }
    }

    [HarmonyPatch(typeof(InspirationHandler), nameof(InspirationHandler.TryStartInspiration))]
    public static class Patch_InspirationGained
    {
        public static void Postfix(InspirationHandler __instance, bool __result)
        {
            if (__result)
            {
                PsycheOwnTriggers.Fire(__instance.pawn, PsycheDefOf.Psyche_OT_Inspiration);
            }
        }
    }

    [HarmonyPatch(typeof(TendUtility), nameof(TendUtility.DoTend))]
    public static class Patch_AllySaved
    {
        public static void Prefix(Pawn doctor, Pawn patient, out float __state)
        {
            __state = -1f;
            if (doctor != null && patient != null && patient != doctor
                && PsycheUtility.IsTracked(doctor) && patient.Downed && !patient.HostileTo(doctor)
                && patient.health.hediffSet.BleedRateTotal >= PsycheTuning.LifeThreateningBleedRate)
            {
                __state = patient.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss)?.Severity ?? 0f;
            }
        }

        public static void Postfix(Pawn doctor, float __state)
        {
            if (__state < 0f)
            {
                return;
            }

            float bloodLoss = __state > 1f ? 1f : __state;
            float factor = PsycheTuning.SavedAllyBloodScaleMin
                + ((PsycheTuning.SavedAllyBloodScaleMax - PsycheTuning.SavedAllyBloodScaleMin) * bloodLoss);
            float baseSize = PsycheDefOf.Psyche_OT_SavedAlly.stages[0].baseMoodEffect;
            PsycheOwnTriggers.Fire(doctor, PsycheDefOf.Psyche_OT_SavedAlly, baseSize * factor * PsycheTuning.WoundScale);
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
    public static class Patch_PawnKilled
    {
        public static void Postfix(Pawn __instance, DamageInfo? dinfo)
        {
            PsycheClosure.OnPawnDied(__instance, dinfo?.Instigator as Pawn);
        }
    }

    [HarmonyPatch(typeof(Building_Grave), nameof(Building_Grave.Notify_HauledTo))]
    public static class Patch_CorpseBuried
    {
        public static void Postfix(Building_Grave __instance, Thing thing)
        {
            if (thing is Corpse corpse && corpse.InnerPawn != null && __instance.Map != null)
            {
                PsycheClosure.OnBuried(corpse.InnerPawn, __instance.Map);
            }
        }
    }

    [HarmonyPatch(typeof(CompDrug), nameof(CompDrug.PostIngested))]
    public static class Patch_DrugIngested
    {
        public static void Postfix(CompDrug __instance, Pawn ingester)
        {
            PsycheMedication.OnIngested(ingester, __instance.parent.def);
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.AddHediff), new[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageWorker.DamageResult) })]
    public static class Patch_AddictionGained
    {
        public static void Postfix(Hediff hediff)
        {
            if (hediff is Hediff_Addiction addiction && addiction.pawn != null)
            {
                PsycheAddictionShortcut.OnAddictionGained(addiction.pawn, addiction);
            }
        }
    }

    [HarmonyPatch(typeof(Thought_Memory), nameof(Thought_Memory.MoodOffset))]
    public static class Patch_SourceMoodZero
    {
        [System.ThreadStatic] public static bool BypassZero;

        public static void Postfix(Thought_Memory __instance, ref float __result)
        {
            if (BypassZero || __instance is Thought_Psychlet)
            {
                return;
            }

            if (PsycheThoughtSetup.IsRegisteredSource(__instance.def) && PsycheUtility.HasPsyche(__instance.pawn))
            {
                __result = 0f;
            }
        }
    }

    [HarmonyPatch(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.TryGainMemory), new[] { typeof(Thought_Memory), typeof(Pawn) })]
    public static class Patch_TryGainMemory
    {
        public static void Postfix(MemoryThoughtHandler __instance, Thought_Memory newThought)
        {
            if (newThought == null || !__instance.Memories.Contains(newThought))
            {
                return;
            }

            Pawn pawn = __instance.pawn;

            // Companions and own-trigger psychlets: capture + recompute; never spawn from these.
            if (newThought is Thought_Psychlet pt)
            {
                pt.EnsureCaptured();
                pawn.needs?.TryGetNeed<Need_Psyche>()?.Recompute();
                return;
            }

            // Registered source: spawn a companion that carries the wound. The source stays, mood-zeroed (invisible).
            if (!PsycheThoughtSetup.IsRegisteredSource(newThought.def) || !PsycheUtility.HasPsyche(pawn))
            {
                return;
            }

            float signed;
            Patch_SourceMoodZero.BypassZero = true;
            try
            {
                signed = newThought.MoodOffset() * PsycheTuning.WoundScale;
            }
            finally
            {
                Patch_SourceMoodZero.BypassZero = false;
            }

            if (signed == 0f)
            {
                return;
            }

            ThoughtDef carrier = signed > 0f ? PsycheDefOf.Psyche_Boon : PsycheDefOf.Psyche_Injury;
            Thought_Psychlet companion = (Thought_Psychlet)ThoughtMaker.MakeThought(carrier);
            companion.SetSource(newThought.def, newThought.CurStageIndex);
            companion.otherPawn = newThought.otherPawn;
            companion.InitMagnitude(signed);
            __instance.TryGainMemory(companion);
            pawn.needs?.TryGetNeed<Need_Psyche>()?.Recompute();
        }
    }

    [HarmonyPatch(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.RemoveMemory))]
    public static class Patch_RemoveMemory
    {
        public static void Postfix(MemoryThoughtHandler __instance, Thought_Memory th)
        {
            if (th is Thought_Psychlet pt && pt.ShouldDiscard)
            {
                pt.RollOnExpiry();
                __instance.pawn.needs?.TryGetNeed<Need_Psyche>()?.Recompute();
            }
        }
    }
}
