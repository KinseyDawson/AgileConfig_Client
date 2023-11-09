using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace AgileConfig.Client
{
    class ClientShutdownHostService : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                // close websocket
                await ConfigClient.Instance?.DisconnectAsync();
            }
            catch
            {
            }
        }
    }
}
