﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using AgileConfig.Client;
using AgileConfig.Protocol;
using System.Threading;
using System.Threading.Tasks;

namespace Agile.Config.Client.Tests
{
    [TestClass()]
    public class ConfigClientTests
    {
        [TestMethod()]
        public void LoadConfigsTest()
        {
            var client = new ConfigClient("1","2","http://", "DEV");

            client.LoadConfigs(null);

            var item = client.Get("x");
            Assert.IsNull(item);
            var item1 = client["x"];
            Assert.IsNull(item1);

            var items = client.GetGroup("x");
            Assert.IsNotNull(items);
            Assert.AreEqual(items.Count , 0);

            var configs = new List<ConfigItem>();
            client.LoadConfigs(configs);
            item = client.Get("x");
            Assert.IsNull(item);
            item1 = client["x"];
            Assert.IsNull(item1);

            items = client.GetGroup("x");
            Assert.IsNotNull(items);
            Assert.AreEqual(items.Count, 0);

            configs = new List<ConfigItem>() { 
                new ConfigItem
                {
                    key = "a",
                    value = "b",
                }
            };
            client.LoadConfigs(configs);
            item = client.Get("x");
            Assert.IsNull(item);
            item1 = client["x"];
            Assert.IsNull(item1);

            items = client.GetGroup("x");
            Assert.IsNotNull(items);
            Assert.AreEqual(items.Count, 0);

            item = client.Get("a");
            Assert.IsNotNull(item);
            Assert.AreEqual("b", item);
            item1 = client["a"];
            Assert.IsNotNull(item1);
            Assert.AreEqual("b", item1);

            configs = new List<ConfigItem>() {
                new ConfigItem
                {
                    key = "a",
                    value = "b",
                },
                  new ConfigItem
                {
                    key = "a1",
                    value = "b1",
                    group = "g"
                }
            };
            client.LoadConfigs(configs);
            item = client.Get("x");
            Assert.IsNull(item);
            item1 = client["x"];
            Assert.IsNull(item);

            items = client.GetGroup("x");
            Assert.IsNotNull(items);
            Assert.AreEqual(items.Count, 0);

            item = client.Get("a");
            Assert.IsNotNull(item);
            Assert.AreEqual("b", item);
            item1 = client["a"];
            Assert.IsNotNull(item1);
            Assert.AreEqual("b", item1);

            item = client.Get("g:a1");
            Assert.IsNotNull(item);
            Assert.AreEqual("b1", item);
            item1 = client["g:a1"];
            Assert.IsNotNull(item1);

            items = client.GetGroup("g");
            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.Count);

            var config = items[0];
            Assert.IsNotNull(config);
            Assert.AreEqual(config.value, "b1");
        }

        [TestMethod]
        public void HighFrequencyTest()
        {
            var client = new ConfigClient("1", "2", "http://", "DEV");

            var source = new CancellationTokenSource();

            int seed = 0;
            Task.Run(() =>
            {
                while (!source.IsCancellationRequested)
                {
                    var configs = new List<ConfigItem>();
                    Interlocked.Increment(ref seed);
                    configs.Add(new ConfigItem() { key = "seed", value = seed.ToString() });
                    client.LoadConfigs(configs);
                    Thread.Sleep(1);
                }
            }, source.Token);

            int times = 0;
            int lastNumber = 0;
            while (times++ < 100)
            {
                Thread.Sleep(1);
                var val = client.Get("seed");
                Assert.IsTrue(int.TryParse(val, out var num),$"seed val:{val}");
                Assert.IsTrue(num >= lastNumber, $"num:{num} lastNumber:{lastNumber}");
                lastNumber = num;
            }
            source.Cancel();
        }
    }
}