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
            }
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
            if (!(group is Thought_Psychlet) || !PsycheUtility.IsTracked(pawn))
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

            bool isBoon = leading is Thought_Psychlet lead && lead.IsBoon;

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
                label = label + " (contemplating)";
            }

            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
                string body;
                if (isBoon)
                {
                    body = "Psyche restored: " + amount.ToString("##0");
                }
                else if (contemplating)
                {
                    body = "The injury has faded, but has not fully settled - it may still leave a scar before it passes.";
                }
                else
                {
                    body = "Psyche damage: -" + amount.ToString("##0");
                }

                Need_Psyche? need = pawn.needs?.TryGetNeed<Need_Psyche>();
                string status = need != null ? "\n\n" + need.StatusString() : "";
                string tip = leading.LabelCap.AsTipTitle() + "\n\n" + leading.Description + "\n\n" + body + status;
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
            string number = isBoon ? amount.ToString("##0") : (contemplating ? "0" : "-" + amount.ToString("##0"));
            Widgets.Label(new Rect(rect.x + 235f, rect.y, 32f, rect.height), number);
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            Text.WordWrap = true;

            Group.Clear();
            __result = true;
            return false;
        }
    }
}
