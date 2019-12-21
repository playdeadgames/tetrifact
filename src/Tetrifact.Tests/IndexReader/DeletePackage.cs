﻿using System.IO;
using System.Text;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class DeletePackage : FileSystemBase
    {
        [Fact]
        public void BasicDelete()
        {
            TestPackage testPackage = base.CreatePackage();

            this.PackageDeleter.Delete("some-project", testPackage.Name);

            Assert.False(File.Exists(Path.Combine(this.Settings.ProjectsPath, "some-project", Constants.ManifestsFragment, "manifest.json" )));
        }
    
        // [Fact] disabled because this fails on travis
        public void DeleteWithLockedArchive()
        {
            TestPackage testPackage = base.CreatePackage();

            // mock archive
            string archivePath = base.IndexReader.GetPackageArchivePath("some-project", testPackage.Name);
            File.WriteAllText(archivePath, string.Empty);

            // force create dummy zip file in archive folder
            File.WriteAllText(archivePath, "dummy content");

            // open stream in write mode to lock it, then attempt to purge archives
            using (FileStream fs = File.OpenWrite(archivePath))
            {
                // force write something to stream to ensure it locks
                fs.Write(Encoding.ASCII.GetBytes("random"));

                this.PackageDeleter.Delete("some-project", testPackage.Name);

                Assert.Single(base.Logger.LogEntries);
                Assert.Contains("Failed to purge archive", base.Logger.LogEntries[0]);
            }
        }

        [Fact]
        public async void InvalidProject()
        {
            ProjectNotFoundException ex = await Assert.ThrowsAsync<ProjectNotFoundException>(async () => await this.PackageDeleter.Delete("some-project", "invalidId"));
            Assert.Equal("some-project", ex.Project);
        }

        [Fact]
        public async void InvalidPackage()
        {
            this.InitProject();
            string packageId = "invalidId";
            PackageNotFoundException ex = await Assert.ThrowsAsync<PackageNotFoundException>(async ()=> await this.PackageDeleter.Delete("some-project", packageId));
            Assert.Equal(ex.PackageId, packageId);
        }

    }
}
