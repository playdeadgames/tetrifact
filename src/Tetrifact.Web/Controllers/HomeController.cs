﻿using Microsoft.AspNetCore.Mvc;
using Tetrifact.Core;
using System.Text;
using System.Collections.Generic;
using System;

namespace Tetrifact.Web
{
    public class HomeController : Controller
    {
        #region FIELDS

        private readonly IIndexReader _indexReader;
        private readonly IPackageList _packageList;

        #endregion

        #region CTORS

        public HomeController(IIndexReader indexReader, IPackageList packageList)
        {
            _indexReader = indexReader;
            _packageList = packageList;
        }

        #endregion

        #region METHODS

        [ServiceFilter(typeof(ReadLevel))]
        [Route("")]
        public IActionResult Index()
        {
            return View(new ContentSummaryModel(_packageList.GetProjects()));
        }

        [ServiceFilter(typeof(ReadLevel))]
        [Route("projects/{project}")]
        public IActionResult Project(string project)
        {
            return View(new ProjectModel(
                _packageList.GetPopularTags(project, Settings.IndexTagListLength),
                _packageList.Get(project, 0, Settings.ListPageSize),
                project));
        }

        [ServiceFilter(typeof(ReadLevel))]
        [Route("addPackage/{project}")]
        public IActionResult AddPackage(string project)
        {
            string hostname = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            return View(new AddPackageModel { Project = project, HostName = hostname });
        }

        [ServiceFilter(typeof(ReadLevel))]
        [Route("addProject")]
        public IActionResult AddProject()
        {
            return View();

        }
        
        [ServiceFilter(typeof(ReadLevel))]
        [Route("api")]
        public IActionResult Api()
        {
            return View();
        }


        [ServiceFilter(typeof(ReadLevel))]
        [Route("package/{project}/{packageId}")]
        public IActionResult Package(string project, string packageId)
        {
            Package package = _indexReader.GetPackage(project, packageId);
            Package child = _indexReader.GetChild(project, packageId);
            bool canDelete = package.DiffState != DiffStates.Undiffed && (child == null || child.DiffState != DiffStates.Undiffed);

            if (package == null)
                return View("Error404");

            return View(new PackageModel(project, packageId, package, canDelete));
        }


        [ServiceFilter(typeof(ReadLevel))]
        [Route("packages/{project}/{page?}")]
        public IActionResult Packages(string project, int page)
        {
            // user-facing page values start at 1 instead of 0, reset to 0 
            if (page != 0)
                page--;

            PageableData<Package> packages  = _packageList.GetPage(project, page, Settings.ListPageSize);

            Pager pager = new Pager();
            string pagerString = pager.Render<Package>(packages, Settings.PagesPerPageGroup, $"/packages/{project}", "page");

            return View(new PackageListModel(project, pagerString, packages));
        }


        [ServiceFilter(typeof(ReadLevel))]
        [Route("packagesWithTag/{project}/{tag}")]
        public IActionResult PackagesWithTag(string project, string tag)
        {
            try
            {
                IEnumerable<Package> packages = _packageList.GetWithTag(project, tag, 0, Settings.ListPageSize);
                return View(new PackagesWithTagModel(project, tag, packages));
            }
            catch (TagNotFoundException)
            {
                return NotFound();
            }
        }


        [Route("error/404")]
        public IActionResult Error404()
        {
            return View();
        }

        [Route("error/500")]
        public IActionResult Error500()
        {
            return View();
        }

        [Route("isAlive")]
        public IActionResult IsAlive()
        {
            return Ok("200");
        }

        [Route("spacecheck")]
        public IActionResult SpaceCheck()
        {
            DiskUseStats useStats = FileHelper.GetDiskUseSats();
            double freeMegabytes = FileHelper.BytesToMegabytes(useStats.FreeBytes);

            StringBuilder s = new StringBuilder();
            s.AppendLine($"Drive size : {FileHelper.BytesToMegabytes(useStats.TotalBytes)}M");
            s.AppendLine($"Space available :  {freeMegabytes}M ({useStats.ToPercent()}%)");

            if (freeMegabytes > Settings.SpaceSafetyThreshold){
                return Ok(s.ToString());
            }

            s.AppendLine($"Insufficient space for safe operation - minimum allowed is {Settings.SpaceSafetyThreshold}M.");

            return Responses.InsufficientSpace(s.ToString());
        }

        #endregion
    }
}

