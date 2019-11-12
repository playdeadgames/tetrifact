﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tetrifact.Core
{
    public class PackageCreate : IPackageCreate
    {
        #region FIELDS

        private readonly IIndexReader _indexReader;
        private readonly IWorkspace _workspace;
        private readonly ILogger<IPackageCreate> _log;

        #endregion

        #region CTORS

        public PackageCreate(IIndexReader indexReader, ILogger<IPackageCreate> log, IWorkspace workspace)
        {
            _indexReader = indexReader;
            _log = log;
            _workspace = workspace;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="manifest"></param>
        public PackageCreateResult CreatePackage(PackageCreateArguments newPackage)
        {
            List<string> transactionLog = new List<string>();
            StringBuilder hashes = new StringBuilder();

            try
            {
                // validate the contents of "newPackage" object
                if (!newPackage.Files.Any())
                    return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.MissingValue, PublicError = "Files collection is empty." };

                if (string.IsNullOrEmpty(newPackage.Id))
                    return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.MissingValue, PublicError = "Id is required." };

                // ensure package does not already exist
                if (_indexReader.PackageNameInUse(newPackage.Project, newPackage.Id))
                    return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.PackageExists };

                // if archive, ensure correct file count
                if (newPackage.IsArchive && newPackage.Files.Count() != 1)
                    return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.InvalidFileCount };

                // if archive, ensure correct file format 
                if (newPackage.IsArchive && newPackage.Format != "zip")
                    return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.InvalidArchiveFormat };

                // if branchFrom package is specified, ensure that package exists (read its manifest as proof)
                if (!string.IsNullOrEmpty(newPackage.BranchFrom) && _indexReader.GetManifest(newPackage.Project, newPackage.BranchFrom) == null) 
                    return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.InvalidDiffAgainstPackage };

                // write attachments to work folder 
                long size = newPackage.Files.Sum(f => f.Length);

                _workspace.Initialize(newPackage.Project);

                // if archive, unzip
                if (newPackage.IsArchive)
                    _workspace.AddArchiveContent(newPackage.Files.First().OpenReadStream());
                else
                    foreach (IFormFile formFile in newPackage.Files)
                        _workspace.AddIncomingFile(formFile.OpenReadStream(), formFile.FileName);

                // get all files which were uploaded, sort alphabetically for combined hashing
                string[] files = _workspace.GetIncomingFileNames().ToArray();
                Array.Sort(files, (x, y) => String.Compare(x, y));
                
                // prevent deletes of empty repository folders this package might need to write to
                LinkLock.Instance.Lock(newPackage.Id);

                foreach (string filePath in files)
                {
                    // get hash of incoming file
                    string fileHash = _workspace.GetIncomingFileHash(filePath);

                    hashes.Append(HashService.FromString(filePath));
                    hashes.Append(fileHash);

                    // todo : this would be a good place to confirm that existingPackageId is actually valid
                    _workspace.WriteFile(filePath, fileHash, newPackage.Id);
                }

                _workspace.Manifest.Description = newPackage.Description;

                // we calculate package hash from a sum of all child hashes
                _workspace.WriteManifest(newPackage.Project, newPackage.Id, HashService.FromString(hashes.ToString()));

                _workspace.UpdateHead(newPackage.Project, newPackage.Id, newPackage.BranchFrom);

                _workspace.Dispose();

                return new PackageCreateResult { Success = true, PackageHash = _workspace.Manifest.Hash };
            }
            catch (Exception ex)
            {
                _log.LogError(ex, string.Empty);
                Console.WriteLine($"Unexpected error : {ex}");
                return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.UnexpectedError };
            }
            finally
            {
                if (!string.IsNullOrEmpty(newPackage.Id))
                    LinkLock.Instance.Unlock(newPackage.Id);
            }
        }

        #endregion
    }
}
