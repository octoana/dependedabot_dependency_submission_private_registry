# Private NuGet dependency submission demo

A .NET 10 console app that consumes
`Octoana.DependencySubmissionDemo.Greetings` version `1.0.0` from the private
**octoana organization** GitHub Packages feed. It uses a real `PackageReference`,
not a project reference or a local package source.

Producer: https://github.com/octoana/private-nuget-demo-producer

## Automatic submission baseline: confirmed

The producer publishes version `1.0.0` to GitHub Packages. The initial automatic
submission test ran without consumer credentials or a package-access grant.
[The automatic run](https://github.com/octoana/dependedabot_dependency_submission_private_registry/actions/runs/40595029062)
failed during restore with **401 Unauthorized** from the private NuGet feed.

The root-level `PrivateFeedDemo.sln` references the app under `src/PrivateFeedDemo`.
GitHub's .NET automatic submission requires a supported manifest at the repository
root, or a Dependabot configuration declaring NuGet. The solution provides that
discovery entry point. Disable automatic dependency submission before proceeding
with the manual test below.

Static manifest analysis may already show the direct package reference in the
dependency graph. Its presence does not prove that automatic submission restored
the package or submitted a resolved dependency snapshot.

## Run locally

Install the .NET 10 SDK. Create a GitHub PAT **classic** with `read:packages` and
access to this private package; authorize it for organization SSO if required.
Supply it through an environment variable rather than modifying NuGet.Config:

```sh
read -r -s -p "GitHub Packages token: " GITHUB_PACKAGES_TOKEN; printf '\n'
export NuGetPackageSourceCredentials_github="Username=YOUR_GITHUB_LOGIN;Password=$GITHUB_PACKAGES_TOKEN;ValidAuthenticationTypes=Basic"
unset GITHUB_PACKAGES_TOKEN
dotnet restore src/PrivateFeedDemo/PrivateFeedDemo.csproj --configfile NuGet.Config
dotnet run --project src/PrivateFeedDemo --configuration Release -- "GitHub"
dotnet run --project src/PrivateFeedDemo --configuration Release -- --self-test
unset NuGetPackageSourceCredentials_github
```

The prompt example uses **bash**. Never paste tokens into source files, command
arguments, workflow logs, or chat. An existing `gh` login does not necessarily
have package scopes and is not automatically used by NuGet.

Expected output:

```text
Hello, GitHub, from a private NuGet package!
PASS: private package greeting and input validation.
```

The feed intentionally contains only this private package source. Package-source
mapping limits it to `Octoana.DependencySubmissionDemo.*`; if adding other
dependencies, configure their sources and mappings explicitly.

## Current phase: manual submission with authenticated restore

The [manual workflow](.github/workflows/dependency-submission.yml) runs only via
**Actions > Manual NuGet dependency submission > Run workflow**. Select `main`.
It does not run on pushes or pull requests.

### Configure credentials

1. Create a GitHub **personal access token (classic)** with `read:packages`.
   The token owner must have read access to the private package. Authorize the
   token for the octoana organization's SSO if required, and ensure organization
   token policies permit it. Use a short expiration; no publishing or deletion
   scopes are needed.
2. In this **consumer** repository, open **Settings > Secrets and variables >
   Actions**.
3. Under **Secrets**, add a repository secret named `NUGET_READ_TOKEN` containing
   the PAT. Do not store it under Variables or paste it into source code/chat.
4. Under **Variables**, add `NUGET_USERNAME` containing the PAT owner's GitHub
   login, not the organization name unless that is actually the user's login.

The workflow explicitly configures
`https://nuget.pkg.github.com/octoana/index.json` as the `github` source, then
supplies NuGet credentials through a step-scoped environment variable. It does
not write the PAT to NuGet.Config. No consumer **Manage Actions access** grant is
needed for this PAT-based test, provided the PAT owner can read the package.

The PAT is used **only for package restore**. Dependency submission uses the
workflow's built-in `GITHUB_TOKEN` with `contents: write`; it does not use the PAT.
Keep dependency graph enabled. Organization Actions policies must allow the
pinned checkout, setup-dotnet, and Advanced Security submission actions.

### Execute and verify

After adding the variable and secret, run the workflow. Confirm these steps pass:

1. Authenticated restore from GitHub Packages.
2. Release build and the app's self-test.
3. Advanced Security Component Detection submission of resolved NuGet dependencies.

Then open **Insights > Dependency graph** and confirm
`Octoana.DependencySubmissionDemo.Greetings` version `1.0.0`, inspecting the
submitted source rather than relying on a statically detected package reference.
The submission correlator is `private-nuget-manual`; the action is configured to
fail if it detects no resolved dependencies.

Missing credentials produce an explicit setup error. A 401/403 during restore
means the token, its scopes, SSO authorization, or package access needs attention.
Do not treat the workflow as validated until the authenticated run and submission
have succeeded.

Dependabot version-update PRs and vulnerability alerts are separate tests and are
not configured here.

## Setup history

An initial setup mistakenly included a manual submission workflow and Dependabot
configuration. Both were removed for the automatic-submission baseline. The
initial manual workflow failed; it was not a test of automatic submission.
Earlier workflow runs remain in Actions history and should not be confused with
the new baseline test.

## References

- [GitHub Packages NuGet registry](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry)
- [Dependency submission API](https://docs.github.com/en/code-security/how-tos/secure-your-supply-chain/secure-your-dependencies/use-dependency-submission-api)
- [.NET automatic submission requirements](https://docs.github.com/en/code-security/reference/supply-chain-security/automatic-dependency-submission#net-projects)
- [Dependabot private registry access](https://docs.github.com/en/code-security/how-tos/secure-your-supply-chain/manage-your-dependency-security/configure-access-to-private-registries)
