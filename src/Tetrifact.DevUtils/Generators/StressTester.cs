﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Tetrifact.Dev;

namespace Tetrifact.DevUtils
{
    public class StressTester
    {
        Process _serverProcess;
        Thread _serverThread;
        List<string> packageNames = new List<string>();
        const int maxPackages = 100;
        const string url = "http://127.0.0.1:3000";
        string _workingDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "stressTests");

        public string Curl(string command, string workingDirectory, bool returnOutPut = false) 
        {
            // force display of headers in output, we want this for HTTP status codes
            if (returnOutPut)
                command = $"-s {command}";
            else
                command = $"-s -D - {command}";

            ProcessStartInfo serverStartInfo = new ProcessStartInfo("curl");
            if (workingDirectory != null)
                serverStartInfo.WorkingDirectory = workingDirectory;
            serverStartInfo.Arguments = command;

            Process process = new Process();
            process.StartInfo = serverStartInfo;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            process.WaitForExit();

            string result = process.StandardOutput.ReadToEnd();
            if (returnOutPut)
                return result;

            if (!result.Contains("200 OK") && !result.Contains("404 Not Found"))
                throw new Exception($"Server call failed : {result}");

            return null;
        }

        public void Start() 
        {
            int threads = 10;

            if (Directory.Exists(_workingDirectory))
                Directory.Delete(_workingDirectory, true);

            Thread.Sleep(100);
            Directory.CreateDirectory(_workingDirectory);


            StartServer();

            Thread.Sleep(5000);
            Curl($"-X DELETE -H \"Content-length:0\" {url}/v1/projects/stressTest", null);
            Curl($"-X POST -H \"Content-length:0\" {url}/v1/projects/stressTest", null);

            // start x nr of threads
            for (int i = 0; i < threads; i++) 
            {
                Thread thread = new Thread(Work);
                thread.Name = $"Worker {i}";
                thread.Start();
            }

            // thread can either add, delete or retrieve a package
        }

        private void StartServer() 
        {
            ProcessStartInfo serverStartInfo = new ProcessStartInfo("cmd");
            serverStartInfo.WorkingDirectory = "../../../../";
            serverStartInfo.Arguments = $" /C \"SET TRANSACTION_HISTORY_DEPTH=1000 && dotnet run --project Tetrifact.Web\"";

            _serverProcess = new Process();
            _serverProcess.StartInfo = serverStartInfo;
            _serverProcess.EnableRaisingEvents = true;
            _serverProcess.Exited += Server_Exited;

            _serverThread = new Thread(new ThreadStart(delegate () {
                Console.WriteLine("Starting server");
                _serverProcess.Start();
            }));

            _serverThread.Start();
        }

        private async void Server_Exited(object sender, EventArgs e)
        {
            Console.WriteLine("server exited");
        }

        private void Create() 
        {
            string rawJson = Curl($"{url}/v1/packages/stressTest?size={2*maxPackages}", null, true);
            string[] existingPackageIds = JsonConvert.DeserializeObject<string[]>(rawJson);
            if (existingPackageIds.Length > maxPackages) 
            {
                Console.WriteLine($"Max packages ({maxPackages}) reached, waiting");
                return;
            }
                
            List<DummyFile> inFiles = new List<DummyFile>();
            for (int i = 0; i < 50; i++)
                inFiles.Add(new DummyFile
                {
                    Data = DataHelper.GetRandomData(1, 100),
                    Path = $"mah/files/{i}"
                });

            string packageName = Guid.NewGuid().ToString();
            lock (packageNames) 
            {
                packageNames.Add(packageName);
            }

            string filename = $"{packageName}.zip";

            // create package from files array, zipped up
            using (Stream zipStream = ArchiveHelper.ZipStreamFromFiles(inFiles)) 
            {
                using (Stream fileStream = File.Create(Path.Combine(_workingDirectory, filename)))
                {
                    zipStream.Seek(0, SeekOrigin.Begin);
                    zipStream.CopyTo(fileStream);
                    fileStream.Close();
                }
            }
            Console.WriteLine($"{Thread.CurrentThread.Name} creating package {packageName}");

            Curl($"-X POST -H \"Content-Type: multipart/form-data\" -F \"Files=@{filename}\" {url}/v1/packages/stressTest/{packageName}?isArchive=true ", _workingDirectory);

            File.Delete(Path.Combine(_workingDirectory, filename));
        }

        private void Retrieve()
        {
            Random random = new Random();
            string packageToRetrieveId = null;

            if (packageNames.Count > 0)
                lock (packageNames)
                {
                    packageToRetrieveId = packageNames[random.Next(0, packageNames.Count - 1)];
                }

            if (packageToRetrieveId == null)
                return;

            string targetfile = Path.Join(_workingDirectory, $"dl-{packageToRetrieveId}.zip");
            if (File.Exists(targetfile))
                return;

            Console.WriteLine($"{Thread.CurrentThread.Name} downloading package {packageToRetrieveId}");
            Curl($"-o {targetfile} {url}/v1/archives/stressTest/{packageToRetrieveId}", null);
            if (!File.Exists(targetfile))
                Console.WriteLine($"Download of package ${packageToRetrieveId} failed");

            File.Delete(targetfile);
        }

        private void Delete()
        {
            string packageToDeleteId = null;
            Random random = new Random();
            if (packageNames.Count > 0)
                lock (packageNames)
                {
                    packageToDeleteId = packageNames[random.Next(0, packageNames.Count -1)];
                }

            Console.WriteLine($"{Thread.CurrentThread.Name} deleting package {packageToDeleteId}");
            Curl($"-X DELETE {url}/v1/packages/stressTest/{packageToDeleteId}", null);

            lock (packageNames)
            {
                packageNames.Remove(packageToDeleteId);
            }
        }

        private void Work() 
        {
            while (true) 
            {
                Random r = new Random();

                try
                {
                    int action = r.Next(0,3);
                    switch (action) 
                    {
                        case 0:
                            {
                                Create();
                                break;
                            }
                        case 1:
                            {
                                Retrieve();
                                break;
                            }
                        case 2:
                            {
                                Delete();
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                Thread.Sleep(r.Next(500, 1000));
            }
        }
    }
}
