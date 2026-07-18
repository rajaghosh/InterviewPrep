# Deployment Concepts

---

## Deployment Patterns

Deployment patterns are automated methods for introducing new application features to users.

### Classical Deployment Pipeline
```
DEV → TEST → STAGING → PRODUCTION
```

### Modern Deployment Strategies

#### 1. Canary Release
Release to a **small group first**, monitor performance, then roll out to everyone.

- Identifies issues before they affect all consumers.
- One of the main enablers of continuous deployments.

#### 2. Blue/Green Deployment
Run **two identical environments** simultaneously — only one is live at a time.

- **Blue** = current live environment
- **Green** = new version being tested
- Flip the load balancer to switch traffic; easy rollback by switching back.
- **Red/Black** = newer synonym (stricter — only one gets traffic at any time).

#### 3. Feature Toggles (Feature Flags)
Turn features **on/off at runtime** without redeployment. Enables continuous deployment by splitting releases from deployments.

#### 4. A/B Testing
**Compare two versions** of an app simultaneously using statistical analysis to determine which performs better.

#### 5. Dark Launches
Release to a **subset of users silently** to gather data and fix issues before the general launch.

### Summary

| Strategy | Key Benefit |
|----------|-------------|
| Canary Release | Early issue detection on small audience |
| Blue/Green | Near-zero downtime + instant rollback |
| Feature Toggles | Decouple deployment from release |
| A/B Testing | Data-driven decision on user preferences |
| Dark Launches | Silent testing on real users |

> **Most commonly preferred**: Blue/Green deployment

---

## Branching Strategy

A branching strategy defines how and when developers branch and merge code.

### Why You Need One
- Reduces complexity in the delivery pipeline.
- Allows developers to focus on changes in relevant branches only.
- Determines what actions trigger a build/test/review.

### Branching Strategy Types

#### Release Branching
All code meant to be deployed together lives on the same branch.

#### Feature Branching
One branch per individual feature. Branches off `develop` and merges back when complete.

#### Story/Task Branching
Similar to feature branching but at a finer granularity (per ticket/task).

#### Trunk-Based Development
Everyone commits directly to `main`/`trunk` at least once a day. Most aligned with CI/CD principles.

### Git Platform Strategies

#### Git Flow
- **`master`** — Production-ready code.
- **`develop`** — Pre-production development.
- **`feature-*`** — New features; branches from `develop`.
- **`hotfix-*`** — Production bug fixes; branches from `master`, merges back to both.
- **`release-*`** — Aggregates fixes for a release; merges to both `develop` and `master`.

#### GitHub Flow
- Single `master` branch (always deployable).
- Any change is made in a descriptive new branch.
- PR created → reviewed → tested → merged to `master`.

#### GitLab Flow
Combines feature-driven development with environmental branches:
- **`development`** — Active development.
- **`pre-production`** — Staging/QA.
- **`production`** — Deployable production code.

---

## Code Review Best Practices

A code review checklist ensures structured quality checks:

| Check | Question |
|-------|----------|
| **Readability** | Are there any redundant comments? |
| **Security** | Does the code expose the system to attack? |
| **Test Coverage** | Are there enough test cases? |
| **Architecture** | Does it use proper encapsulation/modularization? |
| **Reusability** | Does it use reusable components, functions, and services? |

---

## Pull Request Best Practices

- **Write small PRs** — easier to review, less room for bugs.
- **Review your own PR first** — catch errors and typos early.
- **Provide context** — include purpose, overview of changes, and links to tracking issues.
- **Guide reviewers** — suggest file review order for large PRs.

---

## Git Merge Conflicts

### Resolving Conflicts on GitHub
1. Open the PR with the conflict.
2. Click **"Resolve conflicts"**.
3. Edit the conflicting sections.
4. Mark as resolved → submit PR.

### How to Avoid Conflicts
- **Pull changes frequently** to stay up to date.
- **Use descriptive commit messages**.
- **Use Git branches** to isolate your changes.
- **Break large changes into smaller ones**.
- **Review changes before merging**.
- **Communicate with the team** to avoid overlapping work.

### Code Push Strategy
```
FETCH → PULL → CHECK FOR CONFLICT → PUSH
```

---

## User Story to Deployment Workflow

```
1. Get User Story
2. Check for dependencies → Reuse existing functionality where possible
3. Implement the feature
4. Create unit test cases + run tests
5. FETCH → PULL → Check for merge conflicts + resolve
6. Create feature branch
7. Review code changes
8. Push changes
9. Create PR + document it
10. Get PR approved
11. Deploy
```
