using System.Diagnostics;
using System.IO;

namespace UpdateBuildVersion
{
    [Command(PackageIds.MyCommand)]
    internal sealed class MyCommand : BaseCommand<MyCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            //var sols = await VS.Solutions.GetAllProjectsAsync();
            var proj = await VS.Solutions.GetActiveProjectAsync();

            // Get Last Build Date 
            string lastBuildDate = await proj.GetAttributeAsync("LastBuildDate");
            if (string.IsNullOrEmpty(lastBuildDate)) lastBuildDate = DateTime.Now.ToString();

            // Defaults
            int currentBuildNum = 0;
            string defaultVersionTemplate = $"{DateTime.Now.ToString("yy")}.{DateTime.Now.ToString("MM")}.{DateTime.Now.ToString("dd")}";

            var currentVersion = await proj.GetAttributeAsync("Version");

            if(!String.IsNullOrEmpty(currentVersion))
            {
                // We have values to work with
                var versionSplit = currentVersion.Split('.');

                if(versionSplit.Length < 3)
                {
                    // Build octet doesn't exist
                    currentBuildNum = 0;
                }
                else
                {
                    int diff = (DateTime.Now - DateTime.Parse(lastBuildDate)).Days;

                    if(diff > 0)
                    {
                        currentBuildNum = 0;
                    }
                    else
                    {
                        currentBuildNum = int.Parse(versionSplit[3]);
                    }
                }
            }

            // Update version attributes and start build
            string newVersion = $"{defaultVersionTemplate}.{currentBuildNum + 1}";

            // Set new versions to attributes
            await proj.TrySetAttributeAsync("FileVersion", newVersion);
            await proj.TrySetAttributeAsync("AssemblyVersion", newVersion);
            await proj.TrySetAttributeAsync("Version", newVersion);

            // Start build
            await VS.Build.BuildSolutionAsync(BuildAction.Build);

            // Update build date in the settings
            await proj.TrySetAttributeAsync("LastBuildDate", DateTime.Now.ToString());

            // Show message box
            await VS.MessageBox.ShowAsync($"Version {newVersion} build is complete!");

        }
    }
}
