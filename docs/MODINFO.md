# Psyche

The starting-point brief for the mod. Fill this out before writing code - it is the
single source of truth the rest of the work builds from. Delete the prompts as you go.

## One-liner

> One-line summary of what Psyche does.

## Concept

What does the mod do, and why would a player want it? Describe the player-facing
behavior in a paragraph or two, before any implementation detail.

## How it works

Walk through the mod from the player's side: where they encounter it, what they click,
what changes in their game. Keep it concrete.

## Scope

- In scope:
- Out of scope (at least for v1.0):

## Dependencies

- Harmony (remove if this is an XML-only mod)
- (List any framework or content mods this builds on.)

## Technical approach

- Content: which Defs / Patches / Textures are involved?
- Code: what does the assembly do? Any Harmony patches (target method + why)?
- Performance: anything tick-driven? What is cached at load instead?
- Compatibility: what other mods touch the same systems, and how do we stay clear?

## Future ideas

-

## Todo

- [ ] Fill out this brief
- [ ] Fill in About/About.xml (packageId, description, dependencies)
- [ ] Replace About/Preview.png (640x360) and About/ModIcon.png (64x64)
- [ ] First build: `dotnet build Source/Psyche.csproj -c Release`
- [ ] Create the GitHub repo and push
- [ ] Publish to the Workshop (RimWorld in-game uploader writes About/PublishedFileId.txt)
