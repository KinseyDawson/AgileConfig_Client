﻿namespace AgileConfig.Client.RegisterCenter
{
    public static class ConfigClientExtension
    {
        public static IDiscoveryService DiscoveryService(this IConfigClient client)
        {
            return RegisterCenter.DiscoveryService.Instance;
        }
    }
}
