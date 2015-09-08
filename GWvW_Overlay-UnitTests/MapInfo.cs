using System;
using System.Runtime.ExceptionServices;
using GWvW_Overlay;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace GWvW_Overlay_UnitTests
{
    [TestClass]
    public class MapInfo
    {
        [TestMethod]
        public void GetMapData()
        {
            GWvW_Overlay.DataModel.MapInfo result = null;
            for (int id = 94; id < 98; id++)
            {
                var s = Utils.GetJson(String.Format("https://api.guildwars2.com/v2/maps/{0}?lang={1}", id, "fr"));

                result = JsonConvert.DeserializeObject<GWvW_Overlay.DataModel.MapInfo>(
                    s);

                Assert.IsNotNull(result, "Failed to Deserialize, API changed or server unreachable");

            }
        }

        [TestMethod]
        public void GetMapInfo()
        {
            var result = GWvW_Overlay.DataModel.MapInfo.GetMapInfo(95);
            Assert.AreEqual(95, result.Id);
            Assert.AreEqual(" Borderlands", result.Name);
            Assert.AreEqual(80, result.MinLevel);
            Assert.AreEqual(80, result.MaxLevel);
            Assert.AreEqual(3, result.DefaultFloor);
            CollectionAssert.AreEqual(new[] { 3 }, result.Floors);
            Assert.AreEqual(7, result.RegionId);
            Assert.AreEqual("World vs. World", result.RegionName);
            Assert.AreEqual(2, result.ContinentId);
            Assert.AreEqual("Mists", result.ContinentName);
        }

    }
}
