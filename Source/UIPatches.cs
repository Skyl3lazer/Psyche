using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Psyche
{
    [HarmonyPatch(typeof(ThoughtHandler), nameof(ThoughtHandler.GetAllMoodThoughts))]
    public static class Patch_GetAllMoodThoughts
    {
        public static void Postfix(ThoughtHandler __instance, List<Thought> outThoughts)
        {
            if (!PsycheUtility.IsTracked(__instance.pawn))
            {
                return;
            }

            List<Thought_Memory> memories = __instance.memories.Memories;
            for (int i = 0; i < memories.Count; i++)
            {
                if (memories[i] is Thought_Psychlet pt && pt.RawMagnitude > 0f && pt.VisibleInNeedsTab && !outThoughts.Contains(pt))
                {
                    outThoughts.Add(pt);
                }
                else if (memories[i] is Thought_ClarityWindow window && !outThoughts.Contains(window))
                {
                    outThoughts.Add(window);
                }
            }
        }
    }

    [HarmonyPatch(typeof(PawnNeedsUIUtility), nameof(PawnNeedsUIUtility.GetThoughtGroupsInDisplayOrder))]
    public static class Patch_ThoughtDisplayOrder
    {
        private static readonly List<Thought> Group = new List<Thought>();

        public static void Postfix(Need_Mood mood, List<Thought> outThoughtGroupsPresent)
        {
            Pawn? owner = null;
            for (int i = 0; i < outThoughtGroupsPresent.Count; i++)
            {
                if (outThoughtGroupsPresent[i] is Thought_Psychlet pt)
                {
                    owner = pt.pawn;
                    break;
                }
            }

            if (owner == null || !PsycheUtility.HasPsyche(owner))
            {
                return;
            }

            for (int i = 0; i < outThoughtGroupsPresent.Count; i++)
            {
                if (outThoughtGroupsPresent[i] is Thought_Psychlet)
                {
                    outThoughtGroupsPresent[i].cachedMoodOffsetOfGroup = GroupSortValue(mood, outThoughtGroupsPresent[i]);
                }
            }

            outThoughtGroupsPresent.SortByDescending((Thought t) => t.cachedMoodOffsetOfGroup, (Thought t) => t.GetHashCode());
        }

        private static float GroupSortValue(Need_Mood mood, Thought group)
        {
            mood.thoughts.GetMoodThoughts(group, Group);
            float sum = 0f;
            for (int i = 0; i < Group.Count; i++)
            {
                if (Group[i] is Thought_Psychlet pt)
                {
                    sum += pt.SortValue;
                }
            }

            Group.Clear();
            return sum;
        }
    }

    [HarmonyPatch(typeof(NeedsCardUtility), "DrawThoughtGroup")]
    public static class Patch_DrawThoughtGroup
    {
        private static readonly List<Thought> Group = new List<Thought>();
        private static readonly Color PsycheDamageColor = new Color(0.9f, 0.55f, 0.2f);
        private static readonly Color PsycheContemplatingColor = new Color(0.6f, 0.6f, 0.55f);
        private static readonly Color PsycheBoonColor = new Color(0.4f, 0.65f, 0.95f);

        public static bool Prefix(Rect rect, Thought group, Pawn pawn, ref bool __result)
        {
            if (!PsycheUtility.IsTracked(pawn))
            {
                return true;
            }

            if (group is Thought_ClarityWindow window)
            {
                DrawClarityWindow(rect, window, pawn, ref __result);
                return false;
            }

            if (!(group is Thought_Psychlet))
            {
                return true;
            }

            pawn.needs.mood.thoughts.GetMoodThoughts(group, Group);
            if (Group.Count == 0)
            {
                __result = false;
                return false;
            }

            Thought leading = PawnNeedsUIUtility.GetLeadingThoughtInGroup(Group);
            if (!leading.VisibleInNeedsTab)
            {
                Group.Clear();
                __result = false;
                return false;
            }

            Thought_Psychlet? lead = leading as Thought_Psychlet;
            bool isBoon = lead != null && lead.IsBoon;

            float amount = 0f;
            for (int i = 0; i < Group.Count; i++)
            {
                if (Group[i] is Thought_Psychlet pt)
                {
                    amount += isBoon ? pt.CurrentBoonBonus : pt.CurrentPsycheDamage;
                }
            }

            bool contemplating = !isBoon && amount <= PsycheTuning.InjuryHealedEpsilon;

            string label = leading.LabelCap;
            if (Group.Count > 1)
            {
                label = label + " x" + Group.Count;
            }

            if (contemplating)
            {
                label = label + " (" + "Psyche_Label_Contemplating".Translate() + ")";
            }

            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
                string body;
                if (isBoon)
                {
                    body = "Psyche_Tip_PsycheRestored".Translate(amount.ToString("##0"));
                }
                else if (contemplating)
                {
                    body = "Psyche_Tip_Contemplating".Translate();
                }
                else
                {
                    body = "Psyche_Tip_PsycheDamage".Translate(amount.ToString("##0"));
                }

                string details = body;
                if (lead != null)
                {
                    string expiry = ExpiryLine(lead, isBoon);
                    if (!expiry.NullOrEmpty())
                    {
                        details += "\n" + expiry;
                    }

                    if (!isBoon)
                    {
                        string treatment = TreatmentLines(lead);
                        if (!treatment.NullOrEmpty())
                        {
                            details += "\n\n" + treatment;
                        }
                    }
                }

                Need_Psyche? need = pawn.needs?.TryGetNeed<Need_Psyche>();
                string status = need != null ? "\n\n" + need.StatusString() : "";
                string tip = leading.LabelCap.AsTipTitle() + "\n\n" + leading.Description + "\n\n" + details + status;
                TooltipHandler.TipRegion(rect, new TipSignal(tip, 83821));
            }

            Text.WordWrap = false;
            Text.Anchor = TextAnchor.MiddleLeft;
            Rect labelRect = new Rect(rect.x + 10f, rect.y, 225f, rect.height);
            labelRect.yMin -= 3f;
            labelRect.yMax += 3f;
            Widgets.Label(labelRect, label);

            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = isBoon ? PsycheBoonColor : (contemplating ? PsycheContemplatingColor : PsycheDamageColor);
            string number = (isBoon ? amount.ToString("##0") : (contemplating ? "0" : "-" + amount.ToString("##0"))) + "p";
            Widgets.Label(new Rect(rect.x + 235f, rect.y, 42f, rect.height), number);
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            Text.WordWrap = true;

            Group.Clear();
            __result = true;
            return false;
        }

        private static string ExpiryLine(Thought_Psychlet lead, bool isBoon)
        {
            string period = lead.TicksUntilExpiry.ToStringTicksToPeriod();
            if (isBoon)
            {
                return lead.MarkPossible ? "Psyche_Tip_FadesInClarity".Translate(period) : "Psyche_Tip_FadesIn".Translate(period);
            }

            return lead.MarkPossible ? "Psyche_Tip_PassesInScar".Translate(period) : "Psyche_Tip_PassesIn".Translate(period);
        }

        private static string TreatmentLines(Thought_Psychlet lead)
        {
            InjuryTreatState state = lead.TreatState;
            string eligibility;
            switch (state)
            {
                case InjuryTreatState.EligibleNow:
                    eligibility = "Psyche_Tip_ReadyForTherapy".Translate();
                    break;
                case InjuryTreatState.OnCooldown:
                    eligibility = "Psyche_Tip_TreatableAgain".Translate(lead.TicksUntilTreatable.ToStringTicksToPeriod());
                    break;
                case InjuryTreatState.TooMinor:
                    return "Psyche_Tip_TooMinor".Translate();
                case InjuryTreatState.Faded:
                    eligibility = "Psyche_Tip_Faded".Translate();
                    break;
                default:
                    return string.Empty;
            }

            return "Psyche_Tip_TherapyProgress".Translate(lead.TreatmentLevel.ToStringPercent()) + "\n" + eligibility;
        }

        private static void DrawClarityWindow(Rect rect, Thought_ClarityWindow window, Pawn pawn, ref bool __result)
        {
            pawn.needs.mood.thoughts.GetMoodThoughts(window, Group);
            if (Group.Count == 0)
            {
                __result = false;
                return;
            }

            string label = "Psyche_Reflecting_Label".Translate(window.ScarBandLabel);
            if (Group.Count > 1)
            {
                label = label + " x" + Group.Count;
            }

            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
                string tip = label.AsTipTitle() + "\n\n" + "Psyche_Reflecting_Desc".Translate(PsycheTuning.ClarityGlitterCost);
                TooltipHandler.TipRegion(rect, new TipSignal(tip, 83822));
            }

            Text.WordWrap = false;
            Text.Anchor = TextAnchor.MiddleLeft;
            Rect labelRect = new Rect(rect.x + 10f, rect.y, 225f, rect.height);
            labelRect.yMin -= 3f;
            labelRect.yMax += 3f;
            Widgets.Label(labelRect, label);

            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = PsycheBoonColor;
            Widgets.Label(new Rect(rect.x + 235f, rect.y, 42f, rect.height), "0p");
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            Text.WordWrap = true;

            Group.Clear();
            __result = true;
        }
    }
}
