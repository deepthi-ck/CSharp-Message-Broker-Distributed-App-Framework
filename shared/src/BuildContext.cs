namespace MessageBroker.Shared;

public sealed class BuildContext
{
    public string Branch { get; set; } = string.Empty;
    public int FrontendVersion { get; set; }
    public int BackendVersion { get; set; }
    public string FrontendTfm { get; set; } = string.Empty;
    public string BackendTfm { get; set; } = string.Empty;
    public string CustomerVersion { get; set; } = string.Empty;

    public static BuildContext ParseBranch(string branch)
    {
        var parts = branch.Split('_');
        if (parts.Length != 3 || parts[0] != "CSharp" || !parts[1].StartsWith("FE") || !parts[2].StartsWith("BE"))
            throw new ArgumentException($"Invalid branch format: {branch}");
        var fe = int.Parse(parts[1][2..]);
        var be = int.Parse(parts[2][2..]);
        if (fe == be) throw new InvalidOperationException("Same-version FE/BE branches are forbidden.");
        return new BuildContext
        {
            Branch = branch, FrontendVersion = fe, BackendVersion = be,
            FrontendTfm = $"net{fe}.0", BackendTfm = $"net{be}.0",
            CustomerVersion = $".NET {fe} / .NET {be}"
        };
    }
}
