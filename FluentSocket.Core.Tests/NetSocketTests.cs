using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using FluentSocket;
using FluentSocket.Reactive;
using Xunit;

namespace Tests
{
    public class NetSocketTests : IDisposable
    {
        const int Port = 3010;
        private ISocketServer Server { get; } = NetSocketFactory.Default.CreateServer();
        private HashSet<ISocket> Clients { get; } = new HashSet<ISocket>();
        private CompositeDisposable Disposes { get; } = new CompositeDisposable();
        
        public NetSocketTests()
        {
            Server.OnConnections()
              .Subscribe(x => x.AddInto(Clients))
              .AddInto(Disposes);

            Server.ListenAsync(Port).ConfigureAwait(false);
        }

        public void Dispose()
        {
            Disposes.Dispose();

            Clients.ForEach(x => {
                x.Close();
                x.Dispose();
            });

            Server?.Close();
            Server?.Dispose();
        }

        [Fact]
        public async Task TestEndPoint()
        {
            using (var client = await NetSocketFactory.Default.ConnectAsync("127.0.0.1", Port))
            {
                Assert.NotNull(client.LocalEndPoint);
                Assert.NotNull(client.RemoteEndPoint);
                client.Close();
            }
        }

        [Fact]
        public async Task TestSetBufferSize()
        {
            using (var client = await NetSocketFactory.Default.ConnectAsync("127.0.0.1", Port))
            {
                Assert.Equal(8196, client.SetSendBufferSize(8196).SendBufferSize);
                Assert.Equal(65536, client.SetReceiveBufferSize(65536).ReceiveBufferSize);
                client.Close();
            }
        }

        [Fact]
        public async Task TestGetStreams()
        {
            using (var client = await NetSocketFactory.Default.ConnectAsync("127.0.0.1", Port))
            using (var inputStream = client.GetInputStream())
            using (var outputStream = client.GetOutputStream())
            {
                Assert.NotNull(inputStream);
                Assert.NotNull(outputStream);
                client.Close();
            }
        }

    }

}