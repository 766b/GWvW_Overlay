using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Resources;

namespace GWvW_Overlay
{
    public class Utils
    {
        public void SaveJson(string file, string data)
        {
            using (var sw = new StreamWriter(string.Format("{0}", file)))
            {
                sw.WriteLine(data);
            }
        }

        public string FileSize(string file)
        {
            if (!File.Exists(file))
                return "0";

            var info = new FileInfo(file);
            return ReadableFileSize(info.Length);
        }

        public string ReadableFileSize(long size)
        {
            if (size <= 0) return "0";
            var units = new[] { "B", "KB", "MB", "GB", "TB" };
            var digitGroups = (int)(Math.Log10(size) / Math.Log10(1024));
            return string.Format("{0:0.00} {1}", size / Math.Pow(1024, digitGroups), units[digitGroups]);
        }

        public string GetJson(string file)
        {
            string s = null;
            if (file.StartsWith("http"))
            {
                using (var client = new WebClient())
                {
                    try
                    {
                        client.Encoding = Encoding.UTF8;
                        s = client.DownloadString(@file);
                    }
                    catch (WebException)
                    {
                        return null;
                    }
                }
            }
            else
            {
                var uri = new Uri(file, UriKind.Relative);
                StreamResourceInfo contentStream = Application.GetContentStream(uri);
                if (contentStream != null)
                {
                    var sr = new StreamReader(contentStream.Stream);
                    s = sr.ReadToEnd();
                }
            }

            return s;
        }


        static string GetContents(Uri uri)
        {
            using (var response = WebRequest.Create(uri).GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
                return reader.ReadToEnd();
        }
    }
}
