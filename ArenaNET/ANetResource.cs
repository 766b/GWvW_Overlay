using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Arena.NET;
using Newtonsoft.Json;

namespace ArenaNET
{
    public abstract class ANetResource<T> where T : ANetResource<T>, IANetResource, new()
    {
        public const String ApiBase = @"https://api.guildwars2.com/v2/";

        public virtual T GetResource(params String[] parameters)
        {
            return GetResource<T>(parameters);

        }

        public virtual List<String> GetResourceList(params String[] parameters)
        {
            return GetResource<List<String>>(parameters);
        }

        private TK GetResource<TK>(params String[] parameters) where TK : new()
        {
            var r = new T();
            if (parameters.Length > 0) throw new ArgumentException("Should not contain any parameter");


            String json = "";

            HttpStatusCode status = HttpStatusCode.ServiceUnavailable;

            try
            {
                status = GetJSON(r.EndPoint(), out json);
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
