using System;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Examples.DemoSaslAuthMechanism;

internal class RabbitCrDemoAuthMechanism : IAuthMechanism
{
    private readonly string username;
    private readonly string password;

    public RabbitCrDemoAuthMechanism(string username, string password)
    {
        this.username = username;
        this.password = password;
    }
    
    public string Type => "RABBIT-CR-DEMO";

    public ValueTask<ReadOnlyMemory<byte>> StartAsync() 
        => ValueTask.FromResult(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(this.username)));


    public ValueTask<ReadOnlyMemory<byte>> HandleChallengeAsync(ReadOnlySpan<byte> challenge)
    {
        var serverChallenge = Encoding.UTF8.GetString(challenge);

        if (string.Equals("Please tell me your password", serverChallenge))
        {
            return ValueTask.FromResult(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes($"My password is {this.password}")));    
        }

        throw new InvalidOperationException();
    }
}