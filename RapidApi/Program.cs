using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace ODataCLI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("-------------");
                Console.WriteLine("Odata CLI Tool to bootstrap an Odata service");
                Console.WriteLine("-------------");
                Console.WriteLine();
                Console.WriteLine("OPTIONS");
                Console.WriteLine("--schema <Path to xml schema file>");
                Console.WriteLine("--subscriptionId <The Azure Subscription Id>");
                Console.WriteLine("--app <The name of the app service to create.>");
                return;
            }
            var rootCommand = new RootCommand("Odata CLI Tool to bootstrap an Odata service")
            {
                new Option(new string[] {"--csdl", "--metadata", "--schema"}, "The path to the xml schema file.")
                {
                    Argument = new Argument<FileInfo>()
                },
                new Option(new string[] { "--subscriptionId", "--id" }, "The Azure Subscription Id.")
                {
                    Argument = new Argument<string>()
                },
                new Option(new string[] { "--appServiceName", "--app" }, "The name of the App Service to create.")
                {
                    Argument = new Argument<string>()
                },
            };

            rootCommand.Handler = CommandHandler.Create<FileInfo, string, string>(Bootstrap);
            await rootCommand.InvokeAsync(args);
        }

        static void Bootstrap(FileInfo csdl, string subscriptionId, string appServiceName)
        {
            if (subscriptionId != null)
            {
                Console.WriteLine($"Subscription Id: {subscriptionId}");
            }

            if (csdl != null)
            {
                Console.WriteLine($"Schema Path: {csdl.FullName}");
            }

            if (appServiceName != null)
            {
                Console.WriteLine($"App service name: {appServiceName}");
            }

            Console.WriteLine("Preparing your API service...");
            string _args = "https://github.com/habbes/ODataApiServiceHackathon.git";

            string tempWorkingDir = Path.GetTempPath() + appServiceName;

            while (Directory.Exists(tempWorkingDir))
            {
                tempWorkingDir += "-"+RandomString(3);
            }

            Repository.Clone(_args, tempWorkingDir);

            var projectPreparer = new ProjectPreparer(tempWorkingDir, csdl.FullName, appServiceName);
            projectPreparer.Prepare();

            var parametersJsonPath = projectPreparer.ParamsPath;
            var projectZipPath = projectPreparer.ZipPath;

            // We experienced issues using PowerShell, propbably because it's a .Net Framework library
            // We also had issues related to execution policies
            // We resorted to call the powershell script from the Process.ExecuteAsync instead
            // We can restore this (and reinstall the NuGet package) once we figure out how to get it to work
            //Execute the powershell script
            //using (PowerShell PowerShellInst = PowerShell.Create())
            //{

            //    //string path = System.IO.Path.GetDirectoryName(@"C:\Temp\") + "\\Get-EventLog.ps1";
            //    string path = @"RapidApi\deploy.ps1";

            //    if (!string.IsNullOrEmpty(path))
            //    {
            //        PowerShellInst
            //        //.AddScript("Set-ExecutionPolicy RemoteSigned")
            //        .AddScript(File.ReadAllText(path))
            //        .AddParameter("subscriptionId", subscriptionId)
            //        .AddParameter("projectFilePath", projectZipPath)
            //        .AddParameter("parametersFilePath", parametersJsonPath)
            //        .AddParameter("resourceGroupName", $"rg_{appServiceName}")
            //        .AddParameter("deploymentName", $"dpl_{appServiceName}")
            //        .Invoke(new[] { "Set - ExecutionPolicy Unrestricted - Scope Process" });


            //    }
            //}

            var args = $"--subscription {subscriptionId} --projectFilePath {projectZipPath} --parametersFilePath {parametersJsonPath} --resourceGroupName rg{appServiceName} --deploymentName dpl_{appServiceName} --templateFilePath RapidApi\\azuredeploy.json";
            Process.ExecuteAsync("powershell", $" -ExecutionPolicy RemoteSigned -File \"RapidApi\\deploy.ps1\" {args}", Environment.CurrentDirectory, stdOut =>
            {
                Console.WriteLine(stdOut);
            }, stdErr => {
                Console.Error.WriteLine(stdErr);
            }).Wait();

            DeleteDirectory(tempWorkingDir);
        }

        private static Random random = new Random();

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static void DeleteDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            var files = Directory.GetFiles(directoryPath);
            var directories = Directory.GetDirectories(directoryPath);

            foreach (var file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (var dir in directories)
            {
                DeleteDirectory(dir);
            }

            File.SetAttributes(directoryPath, FileAttributes.Normal);

            Directory.Delete(directoryPath, false);
        }

    }
}
