using Xunit;
using Grpc.Core;
using Grpc.Core.Utils;

namespace Medical.GrpcService.Tests.Helpers;

/// <summary>Minimal in-process ServerCallContext for unit testing.</summary>
internal sealed class TestServerCallContext : ServerCallContext
{
    private TestServerCallContext()
    {
    }

    public static ServerCallContext Create() => new TestServerCallContext();

    protected override string MethodCore => "/test/Method";
    protected override string HostCore => "localhost";
    protected override string PeerCore => "ipv4:127.0.0.1:0";
    protected override DateTime DeadlineCore => DateTime.MaxValue;
    protected override Metadata RequestHeadersCore => [];
    protected override CancellationToken CancellationTokenCore => CancellationToken.None;
    protected override Metadata ResponseTrailersCore => [];
    protected override Status StatusCore { get; set; }
    protected override WriteOptions? WriteOptionsCore { get; set; }

    protected override AuthContext AuthContextCore => new(null, new Dictionary<string, List<AuthProperty>>());

    protected override ContextPropagationToken CreatePropagationTokenCore(
        ContextPropagationOptions? options) =>
        throw new NotImplementedException();

    protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders) =>
        Task.CompletedTask;
}
