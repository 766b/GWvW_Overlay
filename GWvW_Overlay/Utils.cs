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

        public string fileSize(string file)
        {
            if (!File.Exists(file))
                return "0";

            FileInfo info = new FileInfo(file);
            return readableFileSize(info.Length);
        }

        public string readableFileSize(long size) 
        {
            if(size <= 0) return "0";
            string[] units = new string[] { "B", "KB", "MB", "GB", "TB" };
            int digitGroups = (int) (Math.Log10(size)/Math.Log10(1024));
            return string.Format("{0:0.00} {1}", size / Math.Pow(1024, digitGroups), units[digitGroups]);
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
