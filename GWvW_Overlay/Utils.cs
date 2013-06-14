using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Windows.Resources;
using System.IO;
using System.Windows;

namespace GWvW_Overlay
{
    class Utils
    {
        public void saveJson(string file, string data)
        {
            using (StreamWriter sw = new StreamWriter(string.Format("{0}", file)))
            {
                sw.WriteLine(data);
            }
        }
        public string getJSON(string file)
        {
            string s;
            if (file.StartsWith("http"))
            {
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        s = client.DownloadString(@file);
                    }
                    catch (WebException e)
                    {
                        return null;
                    }
                }
            }
            else
            {
                Uri uri = new Uri(file, UriKind.Relative);
                StreamResourceInfo contentStream = Application.GetContentStream(uri);
                s = contentStream.ToString();
                StreamReader sr = new StreamReader(contentStream.Stream);
                s = sr.ReadToEnd();
            }

            return s;
        }
    }
}
