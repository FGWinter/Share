using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System.Collections.Generic;
using System.Text.Json;
using System.Net.Http;

namespace RevitBuildChecker
{
    public class App : IExternalApplication
    {
        #region dictionary
        /*********************************
         **       DICTIONARY       **
         *********************************/
        public class RevitVersionsInfo
        {
            public Dictionary<string, string> Versions { get; set; }
        }
        
        private static readonly HttpClient client = new HttpClient();

        private static readonly Dictionary<string, string> revit2020Builds = new Dictionary<string, string>
{
    {"20190327_2315(x64)", "20.0.0.377"},
    {"20190412_1200(x64)", "20.0.1.2"},
    {"20190725_1135(x64)", "20.1.0.81"},
    {"20190731_1515(x64)", "20.1.1.1"},
    {"20191031_1115(x64)", "20.2.0.48"},
    {"20191106_1200(x64)", "20.2.1.1"},
    {"20200206_0915(x64)", "20.2.11.3"},
    {"20200210_1400(x64)", "20.2.12.1"},
    {"20200426_1515(x64)", "20.2.20.31"},
    {"20200826_1250(x64)", "20.2.30.42"},
    {"20210420_1515(x64)", "20.2.40.65"},
    {"20210804_1515(x64)", "20.2.50.77"},
    {"20211019_1515(x64)", "20.2.60.15"},
    {"20220112_1230(x64)", "20.2.70.6"},
    {"20220225_1515(x64)", "20.2.80.2"},
    {"20220517_1515(x64)", "20.2.90.12"}
};
        #endregion

        public Result OnStartup(UIControlledApplication application)
        {
            //AssemblyResolver.init();
            // Get the local version information
            ControlledApplication controlledApp = application.ControlledApplication;
            string localVersionNumber = controlledApp.VersionNumber; // Example format: "23.1.30.97"
            string localBuildVersion = controlledApp.VersionBuild; // Example format: "20230101_1500(x64)"
            // Extract the year from the version number (first two digits)
            string localYear = "20" + localBuildVersion.Substring(0, 2); // This should correctly format the year as "2023"
            string localYear2020 = "20" + localVersionNumber.Substring(0, 2);
            // URL to the JSON file on GitHub
            const string jsonUrl = "https://raw.githubusercontent.com/HoareLea/HlApps-RevitBuildChecker/main/RevitBuildChecker/dist/RevitVersionsInfo.json";
            
                // Fetch and parse JSON file from GitHub synchronously
                HttpResponseMessage
                    response = client.GetAsync(jsonUrl)
                        .Result; // Using .Result is generally not recommended except under specific constraints like this one.
                response.EnsureSuccessStatusCode();
                string jsonContent = response.Content.ReadAsStringAsync().Result; // Blocking call
                RevitVersionsInfo versionsInfo = JsonSerializer.Deserialize<RevitVersionsInfo>(jsonContent);
                const string tstLocalYear = "2020";


                // Get the build version from the JSON file for the corresponding year
                if (versionsInfo.Versions.TryGetValue(localYear, out string expectedVersion) &
                    localYear != tstLocalYear)
                {

                    TaskDialog mainDialog = new TaskDialog("Critical Update Alert");
                    mainDialog.MainInstruction =
                        "CRITICAL ALERT\r\nUpdate your Revit software to prevent model corruption.";
                    mainDialog.MainContent = $"Your Version: {localBuildVersion}\nLatest Version: {expectedVersion}";
                    mainDialog.FooterText = "Contact DigitalSupport@hoarelea.com if you have any issues.";
                    mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Open Software Center");

                    TaskDialogResult result = mainDialog.Show();

                    // Check if the command link was clicked
                    if (result != TaskDialogResult.CommandLink1) return Result.Succeeded;
                    TaskDialog.Show("IMPORTANT", "Please close Revit before updating.");
                    // Attempt to start the external application
                    System.Diagnostics.Process.Start(@"C:\windows\CCM\SCClient.exe",
                        "softwarecenter:Page=AvailableSoftware FilterType=4");
                    return Result.Succeeded;

                }
               
                if (localYear2020 != tstLocalYear)
                {
                    return Result.Failed;
                }

                if (!revit2020Builds.TryGetValue(localBuildVersion, out string officialRelease))
                    return Result.Succeeded;
                {
                    if (localBuildVersion.Equals("20220517_1515(x64)"))
                    {
                        //TaskDialog.Show("Version Check", $"Your Revit {localYear} version is up to date.\nLocal Version: {localBuildVersion}\nExpected version: 20220517_1515(x64)");
                        return Result.Failed;
                    }

                    TaskDialog mainDialog = new TaskDialog("Critical Update Alert");
                    mainDialog.MainInstruction =
                        "CRITICAL ALERT\r\nUpdate your Revit software to prevent model corruption.";
                    mainDialog.MainContent =
                        $"Your Version: {localBuildVersion}\nLatest Version: 20220517_1515(x64)";
                    mainDialog.FooterText = "Contact DigitalSupport@hoarelea.com if you have any issues.";
                    mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Open Software Center");

                    TaskDialogResult result = mainDialog.Show();

                    // Check if the command link was clicked
                    if (result == TaskDialogResult.CommandLink1)
                    {
                        TaskDialog.Show("IMPORTANT", "Please close Revit before updating.");
                        // Attempt to start the external application
                        System.Diagnostics.Process.Start(@"C:\windows\CCM\SCClient.exe",
                            "softwarecenter:Page=AvailableSoftware FilterType=4");
                    }
                    //TaskDialog.Show("NOTLATEST", "Local build version:" + localBuildVersion + " officialRelease:" + officialRelease+ " localrelease: "); 
                    return Result.Succeeded;
                }
                
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }

    
}
