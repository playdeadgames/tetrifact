﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace Tetrifact.Core
{
    public class TetriSettings : ITetriSettings
    {
        #region FIELDS

        private readonly ILogger<ITetriSettings> _log;

        #endregion

        #region PROPERTIES

        public string ProjectsPath { get; set; }

        public string LogPath { get; set; }

        public string TempPath { get; set; }

        public string RepositoryPath { get; set; }

        public string ArchivePath { get; set; }

        public int ArchiveAvailablePollInterval { get; set; }

        public int ArchiveWaitTimeout { get; set; }

        public int ListPageSize { get; set; }

        public int IndexTagListLength { get; set; }

        public int PagesPerPageGroup { get; set; }

        public int CacheTimeout { get; set; }

        public int LinkLockWaitTime { get; set; }

        public int MaxArchives { get; set; }

        public long SpaceSafetyThreshold { get; set; }

        public AuthorizationLevel AuthorizationLevel { get; set; }

        public IEnumerable<string> AccessTokens { get; set; }

        #endregion

        #region CTORS

        public TetriSettings(ILogger<ITetriSettings> log)
        {
            _log = log;

            // defaults
            this.ArchiveAvailablePollInterval = 1000;   // 1 second
            this.ArchiveWaitTimeout = 10 * 60;          // 10 minutes
            this.LinkLockWaitTime = 1000;               // 1 second
            this.CacheTimeout = 60 * 60;                // 1 hour
            this.ListPageSize = 20;
            this.IndexTagListLength = 20;
            this.PagesPerPageGroup = 20;
            this.MaxArchives = 10;
            this.AuthorizationLevel = AuthorizationLevel.None;

            // get settings from env variables
            this.ProjectsPath = Environment.GetEnvironmentVariable("PROJECTS_PATH");
            this.TempPath = Environment.GetEnvironmentVariable("TEMP_PATH");
            this.RepositoryPath = Environment.GetEnvironmentVariable("HASH_INDEX_PATH");
            this.ArchivePath = Environment.GetEnvironmentVariable("ARCHIVE_PATH");
            this.ListPageSize = this.GetSetting("LIST_PAGE_SIZE", this.ListPageSize);
            this.MaxArchives = this.GetSetting("MAX_ARCHIVES", this.MaxArchives);
            this.AuthorizationLevel = this.GetSetting("AUTH_LEVEL", this.AuthorizationLevel);
            this.SpaceSafetyThreshold = this.GetSetting("SPACE_SAFETY_THRESHOLD", this.SpaceSafetyThreshold);

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ACCESS_TOKENS"))) 
                this.AccessTokens = Environment.GetEnvironmentVariable("ACCESS_TOKENS").Split(",");

            // fall back to defaults
            if (string.IsNullOrEmpty(this.ProjectsPath))
                this.ProjectsPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "projects");

            if (string.IsNullOrEmpty(this.LogPath))
                this.LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "logs", "log.txt");

            if (string.IsNullOrEmpty(TempPath))
                TempPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "temp");

            if (string.IsNullOrEmpty(RepositoryPath))
                RepositoryPath = Path.Join(this.ProjectsPath, Constants.RepositoryFragment);

            if (string.IsNullOrEmpty(ArchivePath))
                ArchivePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "archives");
        }

        /// <summary>
        /// Safely gets integer setting from environment variable. Logs error if value is invalid.
        /// </summary>
        /// <param name="settingsName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private int GetSetting(string settingsName, int defaultValue)
        {
            string settingsRawVariable = Environment.GetEnvironmentVariable(settingsName);
            if (settingsRawVariable == null)
                return defaultValue;

            if (!int.TryParse(settingsRawVariable, out defaultValue))
                _log.LogError($"Environment variable for {settingsName} ({settingsRawVariable}) is not a valid integer.");

            return defaultValue;
        }

        /// <summary>
        /// Safely gets long setting from environment variable. Logs error if value is invalid.
        /// </summary>
        /// <param name="settingsName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>

        private long GetSetting(string settingsName, long defaultValue)
        {
            string settingsRawVariable = Environment.GetEnvironmentVariable(settingsName);
            if (settingsRawVariable == null)
                return defaultValue;

            if (!long.TryParse(settingsRawVariable, out defaultValue))
                _log.LogError($"Environment variable for {settingsName} ({settingsRawVariable}) is not a valid integer.");

            return defaultValue;
        }

        /// <summary>
        /// Safely gets enum setting from environment variable. Logs error if value is invalid.
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="settingsName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private TEnum GetSetting<TEnum>(string settingsName, TEnum defaultValue)
        {
            string settingsRawVariable = Environment.GetEnvironmentVariable(settingsName);
            if (settingsRawVariable == null)
                return defaultValue;

            // messy using try/catch instead of TryParse, but I can't figure out enum tryparse with generics
            try
            {
                defaultValue = (TEnum)Enum.Parse(typeof(TEnum), settingsRawVariable);
            }
            catch
            {
                _log.LogError($"Environment variable for {settingsName} ({settingsRawVariable}) is invalid, it must match one of {string.Join(",", Enum.GetNames(typeof(TEnum)))}.");
            }

            return defaultValue;
        }

        #endregion
    }
}
