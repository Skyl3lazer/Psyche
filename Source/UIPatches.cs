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
                if (memories[i] is Thought_Psyche pt && pt.RawMagnitude > 0f && pt.VisibleInNeedsTab && !outThoughts.Contains(pt))
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

        public static bool Prefix(Rect rect, Thought group, Pawn pawn, ref bool __result)
        {
            if (!(group is Thought_Psyche) || !PsycheUtility.IsTracked(pawn))
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

            float damage = 0f;
            for (int i = 0; i < Group.Count; i++)
            {
                if (Group[i] is Thought_Psyche pt)
                {
                    damage += pt.CurrentPsycheDamage;
                }
            }

            string label = leading.LabelCap;
            if (Group.Count > 1)
            {
                label = label + " x" + Group.Count;
            }

            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
                string tip = leading.LabelCap.AsTipTitle() + "\n\n" + leading.Description + "\n\n" + "Psyche damage: -" + damage.ToString("##0");
                TooltipHandler.TipRegion(rect, new TipSignal(tip, 83821));
            }

            Text.WordWrap = false;
            Text.Anchor = TextAnchor.MiddleLeft;
            Rect labelRect = new Rect(rect.x + 10f, rect.y, 225f, rect.height);
            labelRect.yMin -= 3f;
            labelRect.yMax += 3f;
            Widgets.Label(labelRect, label);

            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = PsycheDamageColor;
            Widgets.Label(new Rect(rect.x + 235f, rect.y, 32f, rect.height), (-damage).ToString("##0"));
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            Text.WordWrap = true;

            Group.Clear();
            __result = true;
            return false;
        }
    }
}
