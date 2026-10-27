# Private NuGet dependency submission demo

A .NET 10 console app that consumes
`Octoana.DependencySubmissionDemo.Greetings` version `1.0.0` from the private
**octoana organization** GitHub Packages feed. It uses a real `PackageReference`,
not a project reference or a local package source.

Producer: https://github.com/octoana/private-nuget-demo-producer

## One-time setup

1. In the producer, run **Actions > Publish private NuGet package** with `1.0.0`.
2. On the published package, open **Package settings > Manage Actions access**.
   Add this consumer repository with **Read** access. Keep package visibility
   **Private**. This grants access to the consumer's Actions and Dependabot jobs;
   repository membership alone is not a substitute for this grant.
3. Ensure the consumer's dependency graph is enabled in its repository settings.
4. Run **Actions > Build and submit dependencies** here.

The workflow restores using `GITHUB_TOKEN` with `packages: read`, builds, runs a
greeting and a self-test, and submits the resolved NuGet dependency graph using
Component Detection. Submission requires `contents: write`. It runs only on
`main` pushes or manual dispatch, not untrusted pull requests.

No long-lived feed credentials are committed or required by these workflows.
Organization policies must allow Actions, the pinned actions used here, package
publishing, and dependency submission.

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

## Verify dependency submission

After a successful run, open **Insights > Dependency graph**. Confirm that
`Octoana.DependencySubmissionDemo.Greetings` at `1.0.0` is present and inspect
the submitted dependency source. A successful build alone does not prove that
submission succeeded.

The feed intentionally contains only this private package source. Package-source
mapping limits it to `Octoana.DependencySubmissionDemo.*`; if adding other
dependencies, configure their sources and mappings explicitly.

## Test Dependabot later

Publish `1.0.1` using the producer workflow, leaving this app on `1.0.0`.
In this repository's Dependabot view, request an update check. Expect a PR
updating the package reference.

Dependabot uses the package's **Manage Actions access** grant for GitHub-hosted
registry authentication; the configuration therefore contains no PAT or
top-level registry credentials. If your deployment does not support automatic
access, follow GitHub's private registry configuration documentation instead.

Dependency submission populates the dependency graph; Dependabot version updates
open update PRs. This custom package has no published security advisory, so this
test does not by itself produce vulnerability alerts.

## References

- [GitHub Packages NuGet registry](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry)
- [Dependency submission API](https://docs.github.com/en/code-security/how-tos/secure-your-supply-chain/secure-your-dependencies/use-dependency-submission-api)
- [Dependabot private registry access](https://docs.github.com/en/code-security/how-tos/secure-your-supply-chain/manage-your-dependency-security/configure-access-to-private-registries)
