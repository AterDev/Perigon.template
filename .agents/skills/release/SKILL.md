---
name: release
description: "Prepare, package, or publish a Perigon template release. Use when bumping the package version, writing release notes, producing a NuGet package, merging a release into the nuget branch, or publishing the template package."
---

# Release

Prepare and validate a release of `Perigon.templates`, then merge the completed release into the `nuget` branch. Treat a push to `nuget` as a publication action because the repository workflow publishes the package from that branch.

## Preflight

- Inspect the current branch, worktree, remotes, and recent release history. Preserve unrelated or pre-existing changes; do not hide, discard, or include them in the release.
- Determine the release change set from Git history and the user's stated scope. Do not derive release notes only from the current uncommitted diff.
- Read the package project before editing. In this repository the authoritative package metadata is normally in `Pack.csproj`.
- If the source branch, intended commits, or publication target cannot be identified safely, stop before merging or pushing and ask for clarification.

## Version and release notes

- Honor an explicitly requested version exactly after checking that it is valid and newer than the published/current version.
- Otherwise increment the final numeric component of the current version (for example, `1.3.12` to `1.3.13`). Do not silently change the major or middle component.
- Update the package version and the existing `ReleaseNotes` or `PackageReleaseNotes` property. In this repository these are normally `<Version>` and `<PackageReleaseNotes>` in `Pack.csproj`.
- Write concise, user-facing release notes based on the actual release changes. Emphasize behavior, fixes, compatibility, configuration, and migration impact; omit internal implementation detail unless users need it.
- Search the repository for other authoritative references to the old package version, such as installation examples in `README.md`, and update those that are meant to track the latest release.

## Documentation check

- Locate `Perigon.docs`, normally as a sibling of this repository. Read its `webinfo.json` before choosing document paths or versions.
- Assess whether the release changes affect user-visible behavior, setup commands, template structure, configuration, dependencies, compatibility, deployment, or upgrade guidance.
- Follow the sibling [`update-docs`](../update-docs/SKILL.md) skill for the assessment, bilingual updates, and validation.
- A package version bump must at least synchronize the Perigon `TemplatePackageVersion` and every latest-version reference enforced by the documentation validator, even when the release needs no additional narrative documentation.
- Keep documentation changes in the `Perigon.docs` repository separate from the package repository's commit history.

## Validate the release

- Run checks proportionate to the changes and compatible with the repository CI. For this repository, inspect `.github/workflows/build.yml` and normally restore, build, test, and run `dotnet pack Pack.csproj -c Release`.
- Inspect the generated package metadata and contents. Confirm the package ID, version, release notes, and expected template files; do not treat a successful local pack as a successful NuGet publication.
- Validate documentation using the commands required by `Perigon.docs`; normally run its `build.ps1`, or at minimum `scripts/validate-docs.ps1` when the full documentation toolchain is unavailable.
- Review final diffs and status in both repositories. Report skipped or unavailable checks.

## Merge and publish

1. Ensure only the intended release changes are committed on the source branch and required checks pass.
2. Update the local `nuget` branch safely from its configured remote, then merge the release source branch or exact release commit into it without rewriting shared history.
3. Resolve conflicts from the actual source content and rerun affected checks. Never discard unrelated work to make the merge succeed.
4. Verify that the resulting `nuget` tip contains the intended version, release notes, and release commits.
5. Push `nuget` only when the user requested publication or otherwise explicitly authorized the push. In this repository that push triggers `.github/workflows/build.yml` and publishes to NuGet.

Finish with the version, release-note summary, package/check results, documentation changes, merge commit, and whether publication was triggered and completed.
