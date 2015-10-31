using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ArenaNET;
using Newtonsoft.Json;

namespace ArenaNET
{
    public abstract class ANetResource<T> where T : ANetResource<T>, IANetResource, new()
    {
        public const String ApiBase = @"https://api.guildwars2.com/v2/";
        protected const String BulkExtension = "?ids={0}";
        protected const String LangParam = "&lang={1}";
        protected const String LangSpec = "?lang={0}";

        public virtual T GetResource(params String[] parameters)
        {
            return GetResource<T>(parameters);

        }

        public virtual List<String> GetResourceList(params String[] parameters)
        {
            return GetResource<List<String>>(parameters);
        }

        protected String BulkParametersConverter(params String[] parameters)
        {
            if (parameters == null || parameters.Length == 0) return "";
            if (parameters.Contains("all"))
            {
                return "all";
            }
            var sb = new StringBuilder();

            for (int i = 0; i < parameters.Length; i++)
            {
                sb.Append(parameters[i]);
                if (i != parameters.Length - 1)
                {
                    sb.Append(",");
                }
            }

            return sb.ToString();
        }

        public abstract List<T> GetResourceBulk(params String[] parameters);

        private TK GetResource<TK>(params String[] parameters) where TK : new()
        {
            var r = new T();
            if (parameters.Length > 0) throw new ArgumentException("Should not contain any parameter");


            String json = "";

            HttpStatusCode status = HttpStatusCode.ServiceUnavailable;

            try
            {
                status = GetJSON(String.Format(r.EndPoint() + LangSpec, Request.ApiKey), out json);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (status != HttpStatusCode.OK) throw new ANetException(status);

            var result = JsonConvert.DeserializeObject<TK>(json);

            return result;
        }

        protected HttpStatusCode GetJSON(string endPoint, out string requestResult)
        {
            HttpStatusCode result;
            var client = WebRequest.CreateHttp(ApiBase + endPoint);

            if (!String.IsNullOrEmpty(Request.ApiKey))
            {
                client.Headers = new WebHeaderCollection()
                    {
                        new NameValueCollection()
                        {
                            {"Authorization", "Bearer " + Request.ApiKey}
                        }
                    };
            }
            using (var response = (HttpWebResponse)client.GetResponse())
            {
                result = response.StatusCode;
                using (var stream = response.GetResponseStream())
                {
                    using (var sr = new StreamReader(stream, Encoding.UTF8))
                    {
                        requestResult = sr.ReadToEnd();
                    }
                }
            }

            return result;
        }
    }
}
