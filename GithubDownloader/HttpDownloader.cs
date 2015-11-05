using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace GithubDownloader
{
    public class GithubDownloader
    {
        private readonly string _baseUri;

        private readonly string _accessToken;

        private readonly string _userAgent;

        private readonly string _releaseUri;

        public GithubDownloader(string baseUri, string accessToken, string userAgent)
        {
            _baseUri = baseUri;
            _accessToken = accessToken;
            _userAgent = userAgent;
            _releaseUri = GetReleaseUri();
        }

        public ICollection<GithubRelease> GetDataForAllReleases()
        {
            var requestingUri = GetAccessTokenUri(_releaseUri);
            DownloadReleases(requestingUri);
            return new List<GithubRelease>();
        }

        public string DownloadReleases(string requestingUri)
        {
            var request = (HttpWebRequest) WebRequest.Create(new Uri(requestingUri));
            request.UserAgent = _userAgent;

            var response = request.GetResponse();
            Console.WriteLine(((HttpWebResponse) response).StatusDescription);

            // Get the stream containing content returned by the server.

           var responseFromServer = ReadResponseFromServer(response);
            // Clean up the streams and the response.
            response.Close();

            Console.WriteLine(responseFromServer);

            return responseFromServer;
        }

        private string GetReleaseUri()
        {
            var releaseUri = $"{_baseUri}/releases";
            return releaseUri;
        }

        private static string ReadResponseFromServer(WebResponse response)
        {
            using (var dataStream = response.GetResponseStream())
            {
                // Open the stream using a StreamReader for easy access.
                using (var reader = new StreamReader(dataStream))
                {
                    // Read the content.
                    return reader.ReadToEnd();
                }
            }
        }

        private string GetAssetsUriForId(string id)
        {
            var assetUri = $"{_releaseUri}/assets/{id}";
            return assetUri;
        }

        private string GetAccessTokenUri(string uri)
        {
            return _accessToken == string.Empty ? uri : uri += $"?access_token={_accessToken}";
        }

        public bool DownloadAsset(string id, string path)
        {
            var assetUri = GetAccessTokenUri(GetAssetsUriForId(id));

            var request = (HttpWebRequest)WebRequest.Create(new Uri(assetUri));
            request.Accept = "application/octet-stream";
            request.UserAgent = "mwhitis";

            var response = request.GetResponse();

            GetBinaryResponseFromResponse(path, response);


            return true;
        }

        private static void GetBinaryResponseFromResponse(string path, WebResponse response)
        {
            long total = 0;
            long received = 0;
            byte[] buffer = new byte[1024];

            using (var fileStream = File.OpenWrite(path))
            {
                using (var input = response.GetResponseStream())
                {
                    //total = input.Length;

                    int size = input.Read(buffer, 0, buffer.Length);
                    while (size > 0)
                    {
                        fileStream.Write(buffer, 0, size);
                        received += size;

                        size = input.Read(buffer, 0, buffer.Length);
                    }
                }

                fileStream.Flush();
            }
        }
    }
}
