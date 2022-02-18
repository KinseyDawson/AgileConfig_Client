﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AgileConfig.Client.RegisterCenter
{
    public interface IRegisterService
    {
        Task RegisterAsync();
        Task UnRegisterAsync();
    }

    public class RegisterService : IRegisterService
    {
        private ConfigClientOptions _options;
        private ILogger _logger;
        private string _uniqueId = "";
        private CancellationTokenSource _cancellationTokenSource;

        public RegisterService(ConfigClientOptions options, ILogger logger)
        {
            _options = options;
            _logger = logger;
        }

        //是否注册成功
        public bool Registered { get; private set; }


        public async Task RegisterAsync()
        {
            var regInfo = _options?.RegisterInfo;
            if (regInfo == null)
            {
                _logger?.LogInformation("NO ServiceRegisterInfo STOP register .");

                return;
            }

            //post registerinfo to server
            await TryRegisterAsync();
        }

        public async Task UnRegisterAsync()
        {
            if (_cancellationTokenSource != null)
            {
                _logger.LogInformation("CANCEL register longtime task .");
                _cancellationTokenSource.Cancel();
            }

            if (!Registered || string.IsNullOrEmpty(_uniqueId))
            {
                _logger.LogInformation("NO service UNIQUEID so do nothing .");

                return;
            }

            var json = JsonConvert.SerializeObject(new
            {
                serviceId = _options.RegisterInfo.ServiceId,
                serviceName = _options.RegisterInfo.ServiceName
            });
            var data = Encoding.UTF8.GetBytes(json);
            var random = new RandomServers(_options.Nodes);
            while (!random.IsComplete)
            {   //随机一个节点尝试移除
                var host = random.Next();
                var postUrl = host + (host.EndsWith("/") ? "" : "/") + $"api/registercenter/{_uniqueId}";
                try
                {
                    var resp = await HttpUtil.DeleteAsync(postUrl, null, data, _options.HttpTimeout, "application/json");
                    var content = await HttpUtil.GetResponseContentAsync(resp);
                    _logger.LogInformation($"UNREGISTER service info from server:{host} then server response result:{content} status:{resp.StatusCode}");

                    if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Registered = false;
                        _uniqueId = JsonConvert.DeserializeObject<RegisterResult>(content).uniqueId;

                        _logger.LogInformation($"UNREGISTER service info to server {host} success , uniqueId:{_uniqueId} serviceId:{_options.RegisterInfo.ServiceId} serviceName:{_options.RegisterInfo.ServiceName}");
                        return;
                    }
                }
                catch (System.Exception ex)
                {
                    _logger?.LogError(ex, $"try to UNREGISTER service info failed . uniqueId:{_uniqueId} url:{postUrl} , serviceId:{_options.RegisterInfo.ServiceId} serviceName:{_options.RegisterInfo.ServiceName}");
                }
            }

        }

        private Task TryRegisterAsync()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            Task.Factory.StartNew(async () =>
            {
                var json = JsonConvert.SerializeObject(_options.RegisterInfo);
                var data = Encoding.UTF8.GetBytes(json);

                while (!token.IsCancellationRequested)
                {
                    if (!Registered)
                    {
                        var random = new RandomServers(_options.Nodes);
                        while (!random.IsComplete)
                        {   //随机一个节点尝试注册
                            var host = random.Next();
                            var postUrl = host + (host.EndsWith("/") ? "" : "/") + $"api/registercenter";
                            try
                            {
                                var resp = await HttpUtil.PostAsync(postUrl, null, data, _options.HttpTimeout, "application/json");
                                var content = await HttpUtil.GetResponseContentAsync(resp);
                                _logger.LogInformation($"REGISTER service info to server:{host} then server response result:{content} status:{resp.StatusCode}");

                                if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                                {
                                    Registered = true;
                                    _uniqueId = JsonConvert.DeserializeObject<RegisterResult>(content).uniqueId;

                                    _logger.LogInformation($"REGISTER service info to server {host} success , uniqueId:{_uniqueId} serviceId:{_options.RegisterInfo.ServiceId} serviceName:{_options.RegisterInfo.ServiceName}");
                                }
                            }
                            catch (System.Exception ex)
                            {
                                _logger?.LogError(ex, $"try to REGISTER service info failed . uniqueId:{_uniqueId} url:{postUrl} , serviceId:{_options.RegisterInfo.ServiceId} serviceName:{_options.RegisterInfo.ServiceName}");
                            }
                        }
                    }
                    await Task.Delay(5000);
                }
            }, TaskCreationOptions.LongRunning).ConfigureAwait(false);

            return Task.CompletedTask;
        }

        class RegisterResult{
            public string uniqueId { get; set; }
        }
    }
}
