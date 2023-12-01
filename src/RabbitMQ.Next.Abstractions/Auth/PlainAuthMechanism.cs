using System;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Auth;

public class PlainAuthMechanism : IAuthMechanism
{
    public PlainAuthMechanism(string userName, string password)
    {
        this.UserName = userName;
        this.Password = password;
    }

    public string Type => "PLAIN";
    public ValueTask<ReadOnlyMemory<byte>> HandleChallengeAsync(ReadOnlySpan<byte> challenge)
    {
        if (!challenge.IsEmpty)
        {
            throw new NotSupportedException("PlainAuthMechanism does not support challenges.");
        }

        ReadOnlyMemory<byte> response = Encoding.UTF8.GetBytes($"\0{this.UserName}\0{this.Password}");
        return ValueTask.FromResult(response);
    }

    public string UserName { get; }

    public string Password { get; }
}