# Private NuGet dependency submission demo

A .NET 10 console app that consumes
`Octoana.DependencySubmissionDemo.Greetings` version `1.0.0` from the private
**octoana organization** GitHub Packages feed. It uses a real `PackageReference`,
not a project reference or a local package source.

Producer: https://github.com/octoana/private-nuget-demo-producer

## Current phase: automatic submission baseline

The producer publishes version `1.0.0` to GitHub Packages. This consumer
intentionally has **no manual dependency-submission workflow**, no Dependabot
version-update configuration, and no committed registry credentials.

The root-level `PrivateFeedDemo.sln` references the app under `src/PrivateFeedDemo`.
GitHub's .NET automatic submission requires a supported manifest at the repository
root, or a Dependabot configuration declaring NuGet. The solution provides that
discovery entry point without introducing a manual submission workflow.

Enable and test GitHub's automatic dependency submission separately through
repository settings. Inspect the automatic job's logs to determine whether it
fails to restore the private package. Failure is a test hypothesis, not a
guaranteed outcome: available authentication and organization settings can affect
the result.

Do not grant this consumer repository package access or add registry credentials
for this initial test. The NuGet feed URL alone does not grant access.

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

## Later phase: manual submission (not configured)

Only after the automatic-submission test is confirmed, add a manual workflow
that explicitly configures the feed URL and authenticates using GitHub Actions
secrets. It should restore, build, run the self-test, and submit the resolved
NuGet dependencies. Never put literal credentials in the workflow.

Verify both the workflow logs and the submitted dependency source in
**Insights > Dependency graph**. Dependabot version-update PRs and vulnerability
alerts are separate tests and are not needed for this scenario.

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
