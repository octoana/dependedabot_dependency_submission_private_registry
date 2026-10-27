using Octoana.DependencySubmissionDemo.Greetings;

if (args is ["--self-test"])
{
    const string expected = "Hello, dependency submission, from a private NuGet package!";
    var actual = Greeter.Greet("dependency submission");
    if (actual != expected)
    {
        throw new InvalidOperationException($"Expected '{expected}', got '{actual}'.");
    }

    foreach (var invalidName in new string?[] { null, "", " " })
    {
        var rejected = false;
        try
        {
            Greeter.Greet(invalidName!);
        }
        catch (ArgumentException)
        {
            rejected = true;
        }

        if (!rejected)
        {
            throw new InvalidOperationException("The private package accepted an invalid name.");
        }
    }

    const string expectedJson = "{\"Message\":\"Hello, Ada, from a private NuGet package!\",\"NameLength\":\"three\"}";
    if (Greeter.GreetAsJson("Ada") != expectedJson)
    {
        throw new InvalidOperationException("The private package's transitive dependencies produced unexpected JSON.");
    }

    Console.WriteLine("PASS: private package greeting, input validation, and transitive dependencies.");
    return;
}

if (args.Length > 1 || (args.Length == 1 && string.IsNullOrWhiteSpace(args[0])))
{
    Console.Error.WriteLine("Usage: PrivateFeedDemo [name | --self-test]");
    Environment.ExitCode = 1;
    return;
}

Console.WriteLine(Greeter.Greet(args.Length == 1 ? args[0] : "GitHub"));
