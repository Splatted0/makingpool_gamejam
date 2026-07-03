---
name: commit
description: Create a git commit following conventional commit format with type/scope, amend detection, and AI-mention guard.
user-invocable: true
argument-hint: [optional message hint]
---

Create a git commit following conventional commit format.

## Pre-commit gate (mandatory)

Before committing, check that the staged changes are clean:

```bash
git diff --cached --stat
```

If there are obvious issues (unresolved merge markers, debug artifacts, committed secrets), STOP and resolve them first.

## Steps

1. Run `git status` and `git diff --cached` (if nothing staged, run `git diff`).
2. Run `git log --oneline -1` to check the last commit.
3. Analyze the changes and determine:
   - **TYPE**: `feat`, `fix`, `chore`, `refactor`, `perf`, `revert`, or `docs`
   - **scope**: affected module/service/package name (omit if repo-wide)
4. **Amend decision**: If the staged/unstaged changes are closely related to the last commit (same scope, continuation of same work), propose `--amend` to the user. Warn about force-push implications.
5. Draft commit message:
   - Subject: `type(scope): description` — max 72 chars, imperative mood, no period
   - Body: optional but recommended for non-trivial changes — `-` bullet points, 72 chars/line, explains *why* not *what*
   - If amending, update the message to cover both original and new changes
6. ❌ NEVER include AI/Claude/Anthropic mentions or `Co-Authored-By` lines.
7. Stage relevant files (prefer specific files over `git add -A`).
8. Commit using heredoc format (add `--amend --no-edit` or `--amend` if amend was agreed):

```bash
git commit -m "type(scope): description" -m "- bullet body line"
```

Or with heredoc for multi-line:

```bash
git commit -F - <<'EOF'
type(scope): short description

- why this change was made
- what problem it solves
EOF
```

If `$ARGUMENTS` is provided, use it as a hint for the commit message.

## Commit type guide

| Type | When to use |
|---|---|
| `feat` | New user-visible feature |
| `fix` | Bug fix |
| `chore` | Tooling, deps, config — no behavior change |
| `refactor` | Code restructure — no behavior change |
| `perf` | Performance improvement |
| `docs` | Documentation only |
| `revert` | Reverts a previous commit |

## After commit

Report the result: `commit: <short-hash> <subject>`.
