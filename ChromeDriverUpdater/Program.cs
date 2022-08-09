using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Collections.Generic;

namespace ChromeDriverUpdater
{
  internal class Program
  {
    static readonly HttpClient client = new HttpClient();

    static async Task Main()
    {
      try
      {
        XmlDocument doc = new XmlDocument();
        doc.Load("config.xml");

        var xmlDoc = doc.DocumentElement;
        var content = new Dictionary<string, dynamic>();

        for (var i = 0; i < xmlDoc.ChildNodes.Count; i++)
        {
          content.Add(xmlDoc.ChildNodes[i].Attributes.GetNamedItem("name").Value, xmlDoc.ChildNodes[i].Attributes.GetNamedItem("value").Value);
        }

        HttpResponseMessage response = await client.GetAsync("https://chromedriver.chromium.org/");
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();

        var pathTools = content["driverPath"];
        var nameOfFile = "chromedriver_win32.zip";
        var nameOfExeFile = "chromedriver.exe";
        var versionFile = content["version"];
        var parsedStableVersionSpan = responseBody.Substring(responseBody.IndexOf("stable"), 300);
        var startOfLink = parsedStableVersionSpan.IndexOf("https");
        var enfOfLink = (parsedStableVersionSpan.IndexOf("/\" ") + 1) - startOfLink;
        var linkOfFolder = parsedStableVersionSpan.Substring(startOfLink, enfOfLink);
        var pathVersionParsed = linkOfFolder.IndexOf("=") + 1;
        var version = linkOfFolder.Substring(pathVersionParsed, linkOfFolder.Length - pathVersionParsed - 1);
        var pathFromDownload = $"https://chromedriver.storage.googleapis.com/{version}/{nameOfFile}";
        
        Console.WriteLine($"Last version of chromedriver is {version}");

        //Checks before update.
        if (!File.Exists($"{pathTools}\\{versionFile}"))
        {
          Console.WriteLine("Version file not found. Begin to update chromedriver.");
          File.WriteAllText($"{pathTools}\\{versionFile}", version);
          UpdateChromeDriver(pathTools, pathFromDownload, nameOfExeFile, nameOfFile);
        }
        else
        {
          var versionNow = File.ReadAllText($"{pathTools}\\{versionFile}");

          if (version == versionNow)
          {
            if (!File.Exists($"{pathTools}\\{nameOfExeFile}"))
            {
              Console.WriteLine("Driver not found, downloading...");
              UpdateChromeDriver(pathTools, pathFromDownload, nameOfExeFile, nameOfFile);
            }
            Console.WriteLine($"You have last version of {nameOfExeFile}");
          }
          else
          {
            Console.WriteLine($"Your version is {versionNow}. Begin to update.");
            File.WriteAllText($"{pathTools}\\{versionFile}", version);
            UpdateChromeDriver(pathTools, pathFromDownload, nameOfExeFile, nameOfFile);
          }
        }
      }
      catch (HttpRequestException e)
      {
        Console.WriteLine("Exception Caught!");
        Console.WriteLine("Message :{0} ", e.Message);
      }
      Console.ReadLine();
    }

    private static void UpdateChromeDriver(string pathTools, string pathDownload, string nameOfExeFile, string nameOfFile)
    {
      //Check .exe file before update
      if (File.Exists($"{pathTools}\\{nameOfExeFile}"))
        File.Delete($"{pathTools}\\{nameOfExeFile}");

      Console.WriteLine("Begin update chromdriver");

      //Download the file.
      using (var client = new WebClient())
      {
        client.DownloadFile(pathDownload, $"{pathTools}\\{nameOfFile}");
      }

      //Extract .zip archieve
      ZipFile.ExtractToDirectory($"{pathTools}\\{nameOfFile}", pathTools);

      //Remove .zip file
      File.Delete($"{pathTools}\\{nameOfFile}");
      Console.WriteLine("Done!");
    }
  }
}
