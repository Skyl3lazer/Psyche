---
version: "0.1.2"
level: copilot
processes:
  design: pair
  implementation: pair
  testing: copilot
  documentation: pair
  review: hint
  deployment: copilot
components:
  Source/: pair
  "1.6/Defs/": pair
  "1.6/Patches/Compat/": copilot
  .github/workflows/: copilot
---

This format is based on [AI-DECLARATION.md](https://ai-declaration.md/en/0.1.2).

## Notes

- Claude Code wrote the release workflow under `.github/workflows` from a scaffold.
- Claude Code wrote the third-party compatibility patches under `1.6/Patches/Compat`.
- The C# under `Source/` and the defs under `1.6/Defs` were written by the author and Claude Code together, feature by feature.
