using System;
using System.IO;
using System.Reflection;

namespace Tetrifact.Core
{
    public class FileHelper
    {
        public static double BytesToMegabytes(long bytes)
        {
            var megs = (bytes / 1024f) / 1024f;
            return Math.Round(megs, 0);
        }

        public static DiskUseStats GetDiskUseSats()
        {
            string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            DriveInfo drive = new DriveInfo(path);
            DiskUseStats stats = new DiskUseStats();
            
            stats.TotalBytes = drive.TotalSize;
            stats.FreeBytes = drive.AvailableFreeSpace;

            return stats; 
        }

        /// <summary>
        /// Moves all files and folders in source dir to target dir. Creates target dir if not exists.
        /// </summary>
        /// <param name="sourceSirectory"></param>
        /// <param name="targetDirectory"></param>
        public static void MoveDirectoryContents(string sourceDirectory, string targetDirectory) 
        {
            FileHelper.EnsureDirectoryExists(targetDirectory);

            foreach (string file in Directory.GetFiles(sourceDirectory))
                File.Move(file, Path.Combine(targetDirectory, Path.GetFileName(file)));
            
            foreach (string subDirectory in Directory.GetDirectories(sourceDirectory))
                MoveDirectoryContents(subDirectory, Path.Combine(targetDirectory, Path.GetFileName(subDirectory)));

        }

        /// <summary>
        /// Creates the directory structure for the given file path if that structure does not exist.
        /// </summary>
        /// <param name="filepath"></param>
        public static void EnsureParentDirectoryExists(string filepath) 
        {
            string dirPath = Path.GetDirectoryName(filepath);
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
        }

        /// <summary>
        /// Creates the specified directory path if it doesn't exist.
        /// </summary>
        /// <param name="dirPath"></param>
        public static void EnsureDirectoryExists(string dirPath)
        {
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
        }

        /// <summary>
        /// Writes the data to the path. Creates diretory structure if necessary.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        public static void WriteText(string path, object data) 
        {
            EnsureParentDirectoryExists(path);
            File.WriteAllText(path, data.ToString());
        }
    }
}


