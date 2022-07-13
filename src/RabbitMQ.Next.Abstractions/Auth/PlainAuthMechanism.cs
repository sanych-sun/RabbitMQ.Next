namespace RabbitMQ.Next.Auth;

public class PlainAuthMechanism : IAuthMechanism
{
    public PlainAuthMechanism(string userName, string password)
    {
        this.UserName = userName;
        this.Password = password;
    }

    public string Type => "PLAIN";

    public string ToResponse() => $"\0{this.UserName}\0{this.Password}";

    public string UserName { get; }

    public string Password { get; }
}