# Contributing to Open.IdentityServer
Contributions are welcome, especially those that address existing bugs, target features on our roadmap, or improve the code or documentation.

## Before You Start
For non-trivial changes, open an issue before beginning work. This gives an opportunity to discuss the proposed change, its scope, and its design before implementation begins.

Small fixes, documentation updates, and other straightforward changes may go directly to a pull request.

Please review the [AI Policy](AI_POLICY.md) prior to making a code contribution.

## Issues
When opening an issue:

- Search existing issues first to avoid duplicates.
- Describe the problem clearly and include reproduction steps where applicable.
- Where relevant, include versions, environment details, logs, and code samples.
- For feature requests, preferably link to the relevant spec, or explain the use case and the desired outcome.

## Branches
Create a branch for each change. Branch names should begin with a short category describing the work:

- `feat/` for new features
- `fix/` for bug fixes
- `docs` for documentation changes
- `refactor/` for code refactoring
- `test/` for changes that only affect tests
- `build/` or `ci/` for build and CI changes
- `chore/` for maintenance tasks
Where a corresponding issue exists, include its number in the branch name.

Examples:
```text
feat/123-device-authorization
fix/456-token-validation-error
docs/789-update-quickstart
```

## Commit Messages
Commit messages should follow the [Conventional Commits](https://www.conventionalcommits.org/) specification.

The general format is:
```text
<type>[optional scope]: <description>
```
Examples:
```text
feat: add device authorization endpoint
fix: reject expired refresh tokens
docs: clarify Entity Framework configuration
```

## Pull Requests
Each pull request should address one thing. Keep the scope focused so that the change is easy to review, test, and revert if necessary.

A pull request should:
- Explain what changed and why.
- Reference the related issue where one exists.
- Include tests for behavioral changes.
- Update documentation when the public behavior or configuration changes.
- Avoid unrelated formatting changes or drive-by refactoring.
- Confirm that the relevant build and test checks pass.
- If a pull request grows to include multiple unrelated changes, split it into separate pull requests.

## Public API & Schema Compatibility
Open.IdentityServer aims to make migration to and from IdentityServer4 and Duende IdentityServer as straightforward as possible.

When changing a public API:

- Prefer preserving existing public types, method signatures, and configuration patterns.
- Keep breaking changes to the minimum necessary. Breaking changes should be deliberate, clearly justified and discussed in an issue beforehand.
- Document any unavoidable breaking change and include migration guidance.

## Code Quality
Contributions should follow the existing project structure, naming conventions, and coding style. Keep changes focused and avoid introducing new dependencies unless they are necessary and appropriately justified.

Before submitting a pull request, run the relevant build and test commands for the project.