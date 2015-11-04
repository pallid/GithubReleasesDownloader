using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GithubDownloader
{
    class Program
    {

        private static string _baseUri;
        private static string _user;
        private static string _repo;
        private static string _token;
        private static string _userAgent;

        private static bool SetDataFromArgs(string[] args)
        {
            if(args.Length < 3)
            {
                ShowUsage();
                return false;
            }


            var uri = new Uri(args[0]);

            _baseUri = GetBaseUri(uri);
            _user = GetUserFromUri(uri);
            _repo = GetRepoFromUri(uri);
            _token = args[1];
            _userAgent = args[2];

            return true;
        }

        private static string GetUserFromUri(Uri uri)
        {
            return !uri.LocalPath.Contains("/") ? string.Empty : uri.Segments[1].TrimEnd('/');
        }

        private static string GetRepoFromUri(Uri uri)
        {
            return !uri.LocalPath.Contains("/") ? string.Empty : uri.Segments[2].TrimEnd('/');
        }

        private static string GetBaseUri(Uri uri)
        {
            return uri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase) ? "https://api.github.com" : $"{uri.Scheme}://{uri.Host}/api/v3";
        }

        private static void ShowUsage()
        {
            Console.WriteLine("{0} {1} {2} {3}",
                Assembly.GetExecutingAssembly().GetName().Name,
                "RepoUri",
                "GithubToken",
                "UserAgentString"
                );
        }


        static void Main(string[] args)
        {
            if (!SetDataFromArgs(args))
            {
                Environment.Exit(-1);
            }

            var fullUrl = $"{_baseUri}/repos/{_user}/{_repo}";

            var githubDownloader = new GithubDownloader(fullUrl, _token, _userAgent);

            var response = githubDownloader.DownloadReleases();

            var releases = JsonConvert.DeserializeObject<ICollection<GithubRelease>>(response);

/*
            var json = JArray.Parse(response);

            */
            foreach (var release in releases)
            {
                var releaseName = $"{release.tag_name}";

                var releasePath = $"releases\\{_user}\\{_repo}\\{releaseName}";

                CheckAndCreateFolder(releasePath);

                SaveReleaseComments(release.body, releasePath);

                foreach (var asset in release.assets)
                {
                    var assetDl = githubDownloader.DownloadAsset(asset.id, releasePath + "\\" + asset.name);
                }
            }


            //Console.WriteLine(json);
        }

        private static void SaveReleaseComments(string comments, string folderName)
        {
            File.WriteAllText(folderName + "\\" + "comments.txt", comments);
        }

        private static void CheckAndCreateFolder(string folderName)
        {
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }
        }
    }
}
