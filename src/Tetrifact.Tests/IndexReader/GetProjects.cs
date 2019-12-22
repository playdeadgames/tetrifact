﻿using System;
using System.Collections.Generic;
using System.IO;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class GetProjects // No base - we need a totally
    {
        ITetriSettings Settings;
        IIndexReader IndexReader;

        #region CTOR

        /// <summary>
        /// Set up clean base file system, don't create any existi
        /// </summary>
        public GetProjects() 
        {
            // we need a folder to work in
            string testFolder = Path.Join(AppDomain.CurrentDomain.BaseDirectory, this.GetType().FullName);
            if (Directory.Exists(testFolder))
                Directory.Delete(testFolder, true);

            Directory.CreateDirectory(testFolder);

            // bind settings to that folder
            Settings = new TetriSettings(new TestLogger<TetriSettings>())
            {
                ProjectsPath = Path.Combine(testFolder, Constants.ProjectsFragment),
                TempPath = Path.Combine(testFolder, "temp"),
                TempBinaries = Path.Combine(testFolder, "temp_binaries"),
                ArchivePath = Path.Combine(testFolder, "archives")
            };
            
            // initialize app, this is always needed
            AppLogic appLogic = new AppLogic(Settings);
            appLogic.Start();

            // we'll be using indexreader for all tests
            IndexReader = new Core.IndexReader(Settings, new TestLogger<IIndexReader>());
        }

        #endregion

        /// <summary>
        /// Retrieves a project
        /// </summary>
        [Fact]
        public void Basic() 
        {
            string project = "my-project";
            Directory.CreateDirectory(Path.Combine(Settings.ProjectsPath, Obfuscator.Cloak(project)));

            IEnumerable<string> projects = IndexReader.GetProjects();
            Assert.Single(projects);
            Assert.Contains(project, projects);
        }

        /// <summary>
        /// A project name can contain characters which cannot be written to file system
        /// </summary>
        [Fact]
        public void IllegalCharacterSupport()
        {
            string project = "* ! : \\ //";
            Directory.CreateDirectory(Path.Combine(Settings.ProjectsPath, Obfuscator.Cloak(project)));

            IEnumerable<string> projects = IndexReader.GetProjects();
            Assert.Single(projects);
            Assert.Contains(project, projects);
        }

        /// <summary>
        /// GetProjects returns an empty list if no projects exist.
        /// </summary>
        [Fact]
        public void Empty()
        {
            IEnumerable<string> projects = IndexReader.GetProjects();
            Assert.Empty(projects);
        }
    }
}
