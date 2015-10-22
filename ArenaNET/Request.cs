using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaNET
{
    public static class Request
    {
        public static String ApiKey = "";
        public static String Lang = "en";

        public static T GetResource<T>(params String[] parameters) where T : ANetResource<T>, IANetResource, new()
        {

            var temp = new T();

            var result = temp.GetResource(parameters);

            return result;
        }


        public static List<String> GetResourceList<T>(params String[] parameters) where T : ANetResource<T>, IANetResource, new()
        {
            var temp = new T();

            var result = temp.GetResourceList();

            return result;
        }
    }

}
