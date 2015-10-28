using System;
using ArenaNET;
using ArenaNET.DataStructures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Arena.NET_Tests
{
    [TestClass]
    public class Request
    {
        [TestMethod]
        public void WvWMatchDeserialization()
        {
            var result = JsonConvert.DeserializeObject<ArenaNET.WvWMatch>(TestJSON);
            Assert.IsNotNull(result);
            result.Maps.ForEach(m => m.Objectives.ForEach(o =>
            {
                Assert.IsNotNull(o);
                Console.WriteLine(o.Name);
                Assert.IsFalse(String.IsNullOrEmpty(o.Name));
            }));
            Console.WriteLine(result);
        }


        [TestMethod]
        public void WvWMatch()
        {
            var result = ArenaNET.Request.GetResource<ArenaNET.WvWMatch>("2-1");
            Assert.IsNotNull(result);
            result.Maps.ForEach(m => m.Objectives.ForEach(o =>
            {
                Assert.IsNotNull(o);
                Console.WriteLine(o.Name);
                Assert.IsFalse(String.IsNullOrEmpty(o.Name));
            }));
            Console.WriteLine(result);

        }
        [TestMethod]
        public void WvWMatches()
        {
            var result = ArenaNET.Request.GetResourceList<ArenaNET.WvWMatch>();
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 1);
            Console.WriteLine(result);
        }


        [TestMethod]
        public void World()
        {
            var result = ArenaNET.Request.GetResourceList<ArenaNET.World>();
            Assert.AreEqual(51, result.Count);
            ArenaNET.Request.Lang = "en";
            var sresult = ArenaNET.Request.GetResource<ArenaNET.World>("1001");
            Assert.AreEqual("Anvil Rock", sresult.Name);
        }

        [TestMethod]
        public void WorldLazyLoad()
        {
            var w = new World { Id = 2202 };
            Assert.IsFalse(String.IsNullOrEmpty(w.Name));
        }

        [TestMethod]
        public void Map()
        {
            var map = ArenaNET.Request.GetResource<Map>("1099");
            Assert.IsFalse(String.IsNullOrEmpty(map.Name));
        }

        [TestMethod]
        public void MapLazyLoad()
        {
            var map = new Map() { Id = 1099 };
            Assert.IsFalse(String.IsNullOrEmpty(map.Name));
        }

        const String TestJSON = "{\"id\":\"2-1\",\"start_time\":\"2015-10-24T18:00:00Z\",\"end_time\":\"2015-10-31T18:00:00Z\",\"scores\":{\"red\":53111,\"blue\":55740,\"green\":91156},\"worlds\":{\"red\":2010,\"blue\":2007,\"green\":2201},\"maps\":[{\"id\":1099,\"type\":\"RedHome\",\"scores\":{\"red\":23733,\"blue\":7931,\"green\":14762},\"bonuses\":[],\"objectives\":[{\"id\":\"1099-99\",\"type\":\"Camp\",\"owner\":\"Red\",\"last_flipped\":\"2015-10-26T11:49:46Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1099-100\",\"type\":\"Camp\",\"owner\":\"Blue\",\"last_flipped\":\"2015-10-26T14:55:19Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1099-101\",\"type\":\"Camp\",\"owner\":\"Green\",\"last_flipped\":\"2015-10-26T15:10:22Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1099-102\",\"type\":\"Tower\",\"owner\":\"Red\",\"last_flipped\":\"2015-10-26T12:34:26Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1099-104\",\"type\":\"Tower\",\"owner\":\"Red\",\"last_flipped\":\"2015-10-26T13:23:45Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1099-106\",\"type\":\"Keep\",\"owner\":\"Green\",\"last_flipped\":\"2015-10-26T13:45:34Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1099-109\",\"type\":\"Camp\",\"owner\":\"Red\",\"last_flipped\":\"2015-10-26T10:16:56Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1099-113\",\"type\":\"Keep\",\"owner\":\"Red\",\"last_flipped\":\"2015-10-26T12:58:32Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1099-115\",\"type\":\"Camp\",\"owner\":\"Red\",\"last_flipped\":\"2015-10-26T15:27:57Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1099-105\",\"type\":\"Tower\",\"owner\":\"Green\",\"last_flipped\":\"2015-10-26T14:45:23Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1099-110\",\"type\":\"Tower\",\"owner\":\"Green\",\"last_flipped\":\"2015-10-26T13:32:53Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1099-114\",\"type\":\"Keep\",\"owner\":\"Red\",\"last_flipped\":\"2015-10-26T13:47:19Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1099-116\",\"type\":\"Camp\",\"owner\":\"Green\",\"last_flipped\":\"2015-10-26T14:26:53Z\",\"claimed_by\":null,\"claimed_at\":null}],\"deaths\":{\"red\":0,\"blue\":0,\"green\":0},\"kills\":{\"red\":0,\"blue\":0,\"green\":0}},{\"id\":1102,\"type\":\"GreenHome\",\"scores\":{\"red\":6664,\"blue\":4017,\"green\":39580},\"bonuses\":[],\"objectives\":[{\"id\":\"1102-100\",\"type\":\"Camp\",\"owner\":\"Red\",\"last_flipped\":\"2015-10-26T14:43:30Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1102-105\",\"type\":\"Tower\",\"owner\":\"Red\",\"last_flipped\":\"2015-10-26T14:04:44Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1102-110\",\"type\":\"Tower\",\"owner\":\"Red\",\"last_flipped\":\"2015-10-26T14:47:28Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1102-116\",\"type\":\"Camp\",\"owner\":\"Red\",\"last_flipped\":\"2015-10-26T15:12:34Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1102-101\",\"type\":\"Camp\",\"owner\":\"Green\",\"last_flipped\":\"2015-10-26T15:28:11Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1102-99\",\"type\":\"Camp\",\"owner\":\"Green\",\"last_flipped\":\"2015-10-25T22:22:49Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1102-102\",\"type\":\"Tower\",\"owner\":\"Green\",\"last_flipped\":\"2015-10-25T22:22:49Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1102-104\",\"type\":\"Tower\",\"owner\":\"Green\",\"last_flipped\":\"2015-10-25T22:22:49Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1102-106\",\"type\":\"Keep\",\"owner\":\"Green\",\"last_flipped\":\"2015-10-25T22:22:49Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1102-109\",\"type\":\"Camp\",\"owner\":\"Green\",\"last_flipped\":\"2015-10-26T14:22:55Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1102-113\",\"type\":\"Keep\",\"owner\":\"Green\",\"last_flipped\":\"2015-10-25T22:22:49Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1102-114\",\"type\":\"Keep\",\"owner\":\"Green\",\"last_flipped\":\"2015-10-25T22:22:49Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1102-115\",\"type\":\"Camp\",\"owner\":\"Green\",\"last_flipped\":\"2015-10-26T15:16:27Z\",\"claimed_by\":null,\"claimed_at\":null}],\"deaths\":{\"red\":0,\"blue\":0,\"green\":0},\"kills\":{\"red\":0,\"blue\":0,\"green\":0}},{\"id\":1143,\"type\":\"BlueHome\",\"scores\":{\"red\":6992,\"blue\":21510,\"green\":16672},\"bonuses\":[],\"objectives\":[{\"id\":\"1143-101\",\"type\":\"Camp\",\"owner\":\"Green\",\"last_flipped\":\"2015-10-26T15:34:58Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1143-110\",\"type\":\"Tower\",\"owner\":\"Green\",\"last_flipped\":\"2015-10-26T15:30:58Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1143-113\",\"type\":\"Keep\",\"owner\":\"Green\",\"last_flipped\":\"2015-10-26T14:57:04Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1143-116\",\"type\":\"Camp\",\"owner\":\"Green\",\"last_flipped\":\"2015-10-26T14:40:16Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1143-99\",\"type\":\"Camp\",\"owner\":\"Blue\",\"last_flipped\":\"2015-10-25T22:22:49Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1143-109\",\"type\":\"Camp\",\"owner\":\"Blue\",\"last_flipped\":\"2015-10-26T15:25:54Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1143-100\",\"type\":\"Camp\",\"owner\":\"Blue\",\"last_flipped\":\"2015-10-26T15:33:26Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1143-102\",\"type\":\"Tower\",\"owner\":\"Blue\",\"last_flipped\":\"2015-10-26T03:38:34Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1143-104\",\"type\":\"Tower\",\"owner\":\"Green\",\"last_flipped\":\"2015-10-26T14:35:14Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1143-105\",\"type\":\"Tower\",\"owner\":\"Green\",\"last_flipped\":\"2015-10-26T02:06:18Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1143-106\",\"type\":\"Keep\",\"owner\":\"Red\",\"last_flipped\":\"2015-10-26T15:14:30Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1143-114\",\"type\":\"Keep\",\"owner\":\"Green\",\"last_flipped\":\"2015-10-25T22:22:49Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"1143-115\",\"type\":\"Camp\",\"owner\":\"Blue\",\"last_flipped\":\"2015-10-26T09:28:24Z\",\"claimed_by\":null,\"claimed_at\":null}],\"deaths\":{\"red\":0,\"blue\":0,\"green\":0},\"kills\":{\"red\":0,\"blue\":0,\"green\":0}},{\"id\":38,\"type\":\"Center\",\"scores\":{\"red\":15722,\"blue\":22282,\"green\":20142},\"bonuses\":[],\"objectives\":[{\"id\":\"38-1\",\"type\":\"Keep\",\"owner\":\"Red\",\"last_flipped\":\"2015-10-26T11:45:30Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"38-17\",\"type\":\"Tower\",\"owner\":\"Red\",\"last_flipped\":\"2015-10-26T11:29:17Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"38-19\",\"type\":\"Tower\",\"owner\":\"Red\",\"last_flipped\":\"2015-10-26T14:45:44Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"38-20\",\"type\":\"Tower\",\"owner\":\"Red\",\"last_flipped\":\"2015-10-26T13:16:13Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"38-2\",\"type\":\"Keep\",\"owner\":\"Blue\",\"last_flipped\":\"2015-10-26T13:47:55Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"38-5\",\"type\":\"Camp\",\"owner\":\"Red\",\"last_flipped\":\"2015-10-26T15:21:44Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"38-6\",\"type\":\"Camp\",\"owner\":\"Red\",\"last_flipped\":\"2015-10-26T15:29:26Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"38-7\",\"type\":\"Camp\",\"owner\":\"Blue\",\"last_flipped\":\"2015-10-26T14:10:06Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"38-8\",\"type\":\"Camp\",\"owner\":\"Blue\",\"last_flipped\":\"2015-10-26T13:50:03Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"38-9\",\"type\":\"Castle\",\"owner\":\"Blue\",\"last_flipped\":\"2015-10-26T14:06:04Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"38-10\",\"type\":\"Camp\",\"owner\":\"Red\",\"last_flipped\":\"2015-10-26T15:32:54Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"38-15\",\"type\":\"Tower\",\"owner\":\"Blue\",\"last_flipped\":\"2015-10-25T22:22:49Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"38-16\",\"type\":\"Tower\",\"owner\":\"Blue\",\"last_flipped\":\"2015-10-26T11:40:25Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"38-18\",\"type\":\"Tower\",\"owner\":\"Red\",\"last_flipped\":\"2015-10-26T15:27:37Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"38-21\",\"type\":\"Tower\",\"owner\":\"Blue\",\"last_flipped\":\"2015-10-26T13:52:43Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"38-22\",\"type\":\"Tower\",\"owner\":\"Blue\",\"last_flipped\":\"2015-10-26T12:12:22Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"38-3\",\"type\":\"Keep\",\"owner\":\"Green\",\"last_flipped\":\"2015-10-25T22:22:49Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"38-4\",\"type\":\"Camp\",\"owner\":\"Green\",\"last_flipped\":\"2015-10-26T15:18:01Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"38-11\",\"type\":\"Tower\",\"owner\":\"Green\",\"last_flipped\":\"2015-10-25T22:22:49Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"38-12\",\"type\":\"Tower\",\"owner\":\"Green\",\"last_flipped\":\"2015-10-25T22:58:28Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"38-13\",\"type\":\"Tower\",\"owner\":\"Green\",\"last_flipped\":\"2015-10-25T22:50:17Z\",\"claimed_by\":null,\"claimed_at\":null},{\"id\":\"38-14\",\"type\":\"Tower\",\"owner\":\"Green\",\"last_flipped\":\"2015-10-26T08:51:40Z\",\"claimed_by\":null,\"claimed_at\":null}],\"deaths\":{\"red\":0,\"blue\":0,\"green\":0},\"kills\":{\"red\":0,\"blue\":0,\"green\":0}}]}";
    }
}
