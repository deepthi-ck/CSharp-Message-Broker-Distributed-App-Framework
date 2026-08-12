using MessageBroker.Shared;
using Xunit;

namespace MessageBroker.Shared.Tests;

public class MessageEntryTest
{
    [Fact]
    public void Clone_IsIndependent()
    {
        var e = new MessageEntry { MessageId = "order:1001", Topic = "orders.created", Payload = "Visvantha" };
        var c = e.Clone();
        c.Payload = "Other";
        Assert.Equal("Visvantha", e.Payload);
    }
}

public class VersionInfoTest
{
    [Fact]
    public void FromEnvironment_SetsFields()
    {
        var v = VersionInfo.FromEnvironment("6", "8", "CSharp_FE6_BE8");
        Assert.Equal("6", v.FrontendDotnet);
        Assert.Equal("8", v.BackendDotnet);
        Assert.Equal("ready", v.Broker);
    }

    [Fact]
    public void ParseBranch_RejectsSameVersion() =>
        Assert.Throws<InvalidOperationException>(() => BuildContext.ParseBranch("CSharp_FE8_BE8"));

    [Fact]
    public void ParseBranch_MapsCorrectly()
    {
        var ctx = BuildContext.ParseBranch("CSharp_FE6_BE8");
        Assert.Equal("net6.0", ctx.FrontendTfm);
        Assert.Equal("net8.0", ctx.BackendTfm);
        Assert.Equal(".NET 6 / .NET 8", ctx.CustomerVersion);
    }
}
