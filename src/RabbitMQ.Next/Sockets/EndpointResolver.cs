using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Exceptions;
using RabbitMQ.Next.Tasks;

namespace RabbitMQ.Next.Sockets;

internal static class EndpointResolver
{
    public static async Task<ISocket> OpenSocketAsync(IReadOnlyList<Endpoint> endpoints, CancellationToken cancellation)
    {
        cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token.Combine(cancellation);
        Dictionary<Uri, Exception> exceptions = null;

        foreach (var endpoint in endpoints)
        {
            try
            {
                return await OpenSocketAsync(endpoint, cancellation).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                // todo: report exception to diagnostic source
                exceptions ??= new Dictionary<Uri, Exception>();
                exceptions.Add(endpoint.ToUri(), e);
            }
        }

        throw new EndPointResolutionException(exceptions);
    }

    private static async Task<ISocket> OpenSocketAsync(Endpoint endpoint, CancellationToken cancellation)
    {
        IPAddress FindAddress(IReadOnlyList<IPAddress> address, AddressFamily family)
            => address.FirstOrDefault(a => a.AddressFamily == family);

        var addresses = await Dns.GetHostAddressesAsync(endpoint.Host, cancellation).ConfigureAwait(false);

        // 1. Try IP v6
        var ipV6Address = FindAddress(addresses, AddressFamily.InterNetworkV6);
        if (ipV6Address != null)
        {
            return await ConnectAsync(ipV6Address, endpoint, cancellation).ConfigureAwait(false);
        }

        // 2. Try IP v4
        var ipV4Address = FindAddress(addresses, AddressFamily.InterNetwork);
        if (ipV4Address != null)
        {
            return await ConnectAsync(ipV4Address, endpoint, cancellation).ConfigureAwait(false);
        }

        throw new NotSupportedException("Cannot connect to the endpoint: no supported protocols is available");
    }

    private static async Task<ISocket> ConnectAsync(IPAddress address, Endpoint endpoint, CancellationToken cancellation)
    {
        var ipEndPoint = new IPEndPoint(address, endpoint.Port);
        var socket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        await socket.ConnectAsync(ipEndPoint, cancellation).ConfigureAwait(false);
        var stream = ConfigureStream(socket, endpoint);
        return new SocketWrapper(socket, stream);
    }

    private static Stream ConfigureStream(Socket socket, Endpoint endpoint)
    {
        Stream stream = new NetworkStream(socket)
        {
            ReadTimeout = 60000,
            WriteTimeout = 60000,
        };

        if (endpoint.UseSsl)
        {
            var sslStream = new SslStream(stream, false);
            sslStream.AuthenticateAsClient(endpoint.Host);

            stream = sslStream;
        }

        return stream;
    }
}
