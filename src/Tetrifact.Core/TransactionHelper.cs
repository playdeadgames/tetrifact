﻿using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tetrifact.Core
{
    public class TransactionHelper
    {
        #region FIELDS

        private readonly IIndexReader _indexReader;

        #endregion

        #region CTORS

        public TransactionHelper(IIndexReader indexReader)
        {
            _indexReader = indexReader;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Gets an object containing all data structures currently at "head" of project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public ProjectRecentHistory GetRecentProjectHistory(string project) 
        {
            // get a list of the transacations we want to keep (our history)
            IList<DirectoryInfo> recentHistory = _indexReader.GetRecentTransactionsInfo(project, Settings.TransactionHistoryDepth).ToList();

            // shorten the history to make it easier to do .include string checks
            IEnumerable<string> recentHistoryDirectoryNames = recentHistory.Select(r => r.Name);

            // list of shards and manifests that make up history
            List<string> shardsInRecentHistory = new List<string>();
            List<string> manifestsInRecentHistory = new List<string>();

            // extract all pointers from transactions in history  
            foreach (DirectoryInfo transaction in recentHistory)
            {
                IEnumerable<FileInfo> transactionFiles = transaction.GetFiles();
                foreach (FileInfo transactionFile in transactionFiles)
                {

                    if (Path.GetFileName(transactionFile.FullName).StartsWith(Constants.LockFragment))
                        continue;

                    string pointer = File.ReadAllText(transactionFile.FullName);
                    if (transactionFile.Name.EndsWith("_manifest"))
                        manifestsInRecentHistory.Add(pointer);
                    else if (transactionFile.Name.EndsWith("_shard"))
                        shardsInRecentHistory.Add(pointer);
                }
            }

            return new ProjectRecentHistory(manifestsInRecentHistory, recentHistoryDirectoryNames, shardsInRecentHistory);
        }

        #endregion
    }
}
