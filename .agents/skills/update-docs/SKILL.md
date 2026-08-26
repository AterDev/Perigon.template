---
name: update-docs
description: "Assess and update Perigon documentation for repository changes. Use when code or template changes may affect user-visible behavior, setup, configuration, compatibility, deployment, or versioned documentation in the sibling Perigon.docs repository."
---

# Update Perigon Documentation

Keep the documentation consistent with the behavior of the current repository change. Update both supported languages when documentation is required; if it is not required, record the concrete reason instead of making speculative edits.

## Establish scope

- Inspect the requested change, relevant Git diff or commit range, and affected runtime/template behavior. Include committed changes in the intended range, not only the working tree.
- Documentation is normally required for changes to user-visible behavior, commands, configuration, defaults, templates or directory structure, public APIs, dependencies and compatibility, deployment/operations, security guidance, or upgrade steps.
- Pure refactors, tests, formatting, and internal implementation changes normally do not require documentation when they leave user behavior and supported workflows unchanged.
- Preserve unrelated or pre-existing changes in both repositories.

## Locate the correct documentation

1. Locate the `Perigon.docs` repository, normally at `../Perigon.docs` relative to the current repository. If it is absent, do not create a replacement; report the missing repository and request its location.
2. Read `webinfo.json` before editing. Use the `DocInfos` entry whose `Name` is `Perigon` to obtain the configured languages, documentation versions, template package version, Aspire version, and content root.
3. Limit project documentation edits to `Content/docs/Perigon` unless a metadata change requires `webinfo.json`. Select language and version directories from `webinfo.json` rather than hard-coding the current values.
4. Search by feature names, commands, configuration keys, and old values to find every relevant page. Use existing navigation and corresponding-page structure instead of inventing a parallel section.

## Update consistently

- Update the matching `zh-CN` and `en-US` pages with equivalent technical meaning. Preserve the natural terminology, filenames, links, headings, examples, and style of each language rather than translating paths literally.
- Document the observable behavior and necessary user actions. Include prerequisites, defaults, limitations, compatibility, migration, or rollback information only when supported by the implementation.
- Keep command examples, configuration keys, package names, versions, and paths exact and verified against source.
- When the template package version changes, update the Perigon `TemplatePackageVersion` in `webinfo.json` and all version references required by the docs validator. Check at least the bilingual quick starts, template-specific quick starts, and version-features/compatibility pages.
- When the Aspire or documentation version changes, use `webinfo.json` as the metadata source and synchronize every corresponding versioned page and example.
- Do not edit documentation for other products under the docs repository.

## Verify

- Review the docs repository diff for bilingual parity, accidental unrelated edits, stale values, broken relative links, and invalid Markdown fences.
- Run `build.ps1` from the `Perigon.docs` root when available. It normally runs `scripts/validate-docs.ps1` and builds from `webinfo.json`.
- If the full build cannot run, execute `pwsh -NoProfile -File ./scripts/validate-docs.ps1` and report why the build was skipped. Do not claim validation succeeded when a required tool was unavailable.
- Recheck the source change against the finished documentation so examples and claims reflect implemented behavior.

Finish with the documentation decision, files changed in each language, metadata changes, validation result, and any known gap requiring follow-up.
