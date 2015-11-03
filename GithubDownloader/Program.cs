﻿using System;
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
            if(args.Length < 4)
            {
                ShowUsage();
                return false;
            }

            _baseUri = "https://api.github.com";
            _user = args[0];
            _repo = args[1];
            _token = args[2];
            _token = args[3];

            return true;
        }

        private static void ShowUsage()
        {
            Console.WriteLine("{0} {1} {2} {3} {4}",
                Assembly.GetExecutingAssembly().GetName().Name,
                "RepoOwner",
                "RepoName",
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

            Console.WriteLine(fullUrl);

            var githubDownloader = new GithubDownloader(fullUrl, _token, _userAgent);

            var response = githubDownloader.DownloadReleases();

            var releases = JsonConvert.DeserializeObject<ICollection<GithubRelease>>(response);

/*
            var json = JArray.Parse(response);

            */
            foreach (var release in releases)
            {
                var releaseName = $"{release.id}-{release.name}-{release.tag_name}";
                Console.WriteLine(releaseName);

                var releasePath = $"releases\\{_user}\\{_repo}\\{releaseName}";

                CheckAndCreateFolder(releasePath);

                SaveReleaseComments(release.body, releasePath);

                foreach (var asset in release.assets)
                {
                    Console.WriteLine("\t{0} - {1}", asset.id, asset.name);
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