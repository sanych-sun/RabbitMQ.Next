using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;

namespace RabbitMQ.Next.Sockets
{
    internal static class EndpointResolver
    {
        public static async Task<ISocket> OpenSocketAsync(IReadOnlyList<Endpoint> endpoints)
        {
            IPAddress FindAddress(IReadOnlyList<IPAddress> addresses, AddressFamily family)
                => addresses.FirstOrDefault(a => a.AddressFamily == family);

            for (var i = 0; i < endpoints.Count; i++)
            {
                var endpoint = endpoints[i];
                var addresses = await Dns.GetHostAddressesAsync(endpoint.Host);

                // 1. Try IP v6
                var ipV6Address = FindAddress(addresses, AddressFamily.InterNetworkV6);
                if (ipV6Address != null)
                {
                    var socket = await TryConnectAsync(ipV6Address, endpoint.Port);
                    if (socket != null)
                    {
                        return new SocketWrapper(socket, endpoint);
                    }
                }

                // 2. Try IP v4
                var ipV4Address = FindAddress(addresses, AddressFamily.InterNetwork);
                if (ipV4Address != null)
                {
                    var socket = await TryConnectAsync(ipV4Address, endpoint.Port);
                    if (socket != null)
                    {
                        return new SocketWrapper(socket, endpoint);
                    }
                }
            }

            // todo: throw more appropriate exception here
            throw new InvalidOperationException();
        }

        private static async Task<Socket> TryConnectAsync(IPAddress address, int port)
        {
            try
            {
                var endpoint = new IPEndPoint(address, port);
                var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                await socket.ConnectAsync(endpoint);
                return socket;

            }
            catch (Exception e)
            {
                // todo: report error to diagnostic source
            }

            return null;
        }
    }
}