using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Transport;

namespace RabbitMQ.Next.Sockets
{
    internal static class EndpointResolver
    {
        public static async Task<ISocket> OpenSocketAsync(Endpoint endpoint)
        {
            IPAddress FindAddress(IReadOnlyList<IPAddress> address, AddressFamily family)
                => address.FirstOrDefault(a => a.AddressFamily == family);

            var addresses = await Dns.GetHostAddressesAsync(endpoint.Host);

            // 1. Try IP v6
            var ipV6Address = FindAddress(addresses, AddressFamily.InterNetworkV6);
            if (ipV6Address != null)
            {
                var socket = await ConnectAsync(ipV6Address, endpoint.Port);
                if (socket != null)
                {
                    return new SocketWrapper(socket, endpoint);
                }
            }

            // 2. Try IP v4
            var ipV4Address = FindAddress(addresses, AddressFamily.InterNetwork);
            if (ipV4Address != null)
            {
                var socket = await ConnectAsync(ipV4Address, endpoint.Port);
                if (socket != null)
                {
                    return new SocketWrapper(socket, endpoint);
                }
            }

            throw new NotSupportedException("Cannot connect to the endpoint: no supported protocols is available");
        }

        private static async Task<Socket> ConnectAsync(IPAddress address, int port)
        {
            var endpoint = new IPEndPoint(address, port);
            var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(endpoint);
            return socket;
        }
    }
}