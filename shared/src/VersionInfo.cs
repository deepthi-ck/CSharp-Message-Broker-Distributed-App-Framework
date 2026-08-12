namespace MessageBroker.Shared;

public sealed class VersionInfo
{
    public string Application { get; set; } = "C# Message Broker / Distributed App Framework";
    public string FrontendDotnet { get; set; } = string.Empty;
    public string BackendDotnet { get; set; } = string.Empty;
    public string Branch { get; set; } = string.Empty;
    public string Broker { get; set; } = "ready";

    public static VersionInfo FromEnvironment(string fe, string be, string branch, string broker = "ready") => new()
    { FrontendDotnet = fe, BackendDotnet = be, Branch = branch, Broker = broker };
}
