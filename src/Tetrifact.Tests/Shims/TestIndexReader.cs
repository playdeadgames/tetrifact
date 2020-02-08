﻿using System.Collections.Generic;
using System.IO;
using Tetrifact.Core;

namespace Tetrifact.Tests
{
    public class TestIndexReader : IIndexReader
    {
        #region FIELDS 

        /// <summary>
        /// Use to retrieve current instance of shim.
        /// </summary>
        public static TestIndexReader Instance;

        private readonly Dictionary<string, string> _matchingHashPackage = new Dictionary<string, string>();

        #endregion

        #region PROPERTIES

        public bool Test_PackageIdExists { get; set; }

        public Package Test_Manifest { get; set; }

        public Stream Test_PackageItem { get; set; }

        public Stream Test_PackageArchive { get; set; }

        public string Test_Head { get; set; }

        #endregion

        #region CTORS

        public TestIndexReader() 
        {
            Instance = this;
        }

        #endregion

        #region METHODS

        public bool PackageNameInUse(string project, string id)
        {
            return this.Test_PackageIdExists;
        }

        public void AddHash(string path, string hash, string package)
        {
            _matchingHashPackage.Add($"{path}:{hash}", package);
        }

        public void Initialize()
        {
            // no need to do anything here
        }

        public Package GetPackage(string project, string packageId)
        {
            return this.Test_Manifest;
        }

        public GetFileResponse GetFile(string project, string id)
        {
            return new GetFileResponse(this.Test_PackageItem, id);
        }

        public Stream GetPackageAsArchive(string project, string packageId)
        {
            return this.Test_PackageArchive;
        }

        public void PurgeOldArchives()
        {
            // do nothing
        }

        public string GetTempArchivePath(string project, string packageId)
        {
            return $"{packageId}.zip.tmp";
        }

        public string GetArchivePath(string project, string packageId)
        {
            return $"{packageId}.zip";
        }

        public void CleanRepository()
        {
            // do nothing
        }

        public string GetHead(string project) 
        {
            return this.Test_Head;
        }

        public void AddTag(string packageId, string tag)
        {
            
        }

        public void RemoveTag(string packageId, string tag)
        {
            
        }

        public string RehydrateOrResolveFile(string project, string package, string filePath)
        {
            return null;
        }

        public DirectoryInfo GetActiveTransactionInfo(string project)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<string> GetManifestPaths(string project)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<DirectoryInfo> GetRecentTransactionsInfo(string project, int count)
        {
            throw new System.NotImplementedException();
        }

        public string GetItemPathOnDisk(string project, string package, string path) 
        {
            return null;
        }

        public bool ProjectExists(string project)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
