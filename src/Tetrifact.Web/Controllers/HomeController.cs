﻿using Microsoft.AspNetCore.Mvc;
using Tetrifact.Core;
using System.Text;
using System.Linq;

namespace Tetrifact.Web
{
    public class HomeController : Controller
    {
        #region FIELDS

        private readonly ITetriSettings _settings;
        private readonly IIndexReader _indexService;
        private readonly IPackageList _packageList;

        #endregion

        #region CTORS

        public HomeController(ITetriSettings settings, IIndexReader indexService, IPackageList packageList)
        {
            _settings = settings;
            _indexService = indexService;
            _packageList = packageList;
        }

        #endregion

        #region METHODS

        [ServiceFilter(typeof(ReadLevel))]
        public IActionResult Index()
        {
            ViewData["packages"] = _packageList.Get(0, _settings.ListPageSize);
            ViewData["tags"] = _packageList.GetPopularTags(_settings.IndexTagListLength);
            return View();
        }


        [ServiceFilter(typeof(ReadLevel))]
        [Route("api")]
        public IActionResult Api()
        {
            return View();
        }


        [ServiceFilter(typeof(ReadLevel))]
        [Route("package/{packageId}/{page?}")]
        public IActionResult Package(string packageId, int page)
        {
            ViewData["packageId"] = packageId;
            Manifest manifest = _indexService.GetManifest(packageId);
            if (manifest == null)
                return View("Error404");

            ViewData["manifest"] = manifest;

            if (page != 0)
                page--;

            Pager pager = new Pager();
            PageableData<ManifestItem> filesPage = new PageableData<ManifestItem>(manifest.Files.Skip(page * _settings.ListPageSize).Take(_settings.ListPageSize), page, _settings.ListPageSize, manifest.Files.Count);
            ViewData["filesPage"] = filesPage;
            ViewData["filesPager"] = pager.Render(filesPage, _settings.PagesPerPageGroup, $"/package/{packageId}", "page", "#manifestFiles");
            return View();
        }


        [ServiceFilter(typeof(ReadLevel))]
        [Route("packages/{page?}")]
        public IActionResult Packages(int page)
        {
            // user-facing page values start at 1 instead of 0. reset
            if (page != 0)
                page--;

            Pager pager = new Pager();
            PageableData<Package> packages  = _packageList.GetPage(page, _settings.ListPageSize);
            ViewData["pager"] = pager.Render(packages, _settings.PagesPerPageGroup, "/packages", "page");
            ViewData["packages"] = packages;
            return View();
        }


        [ServiceFilter(typeof(ReadLevel))]
        [Route("packagesWithTag/{tag}")]
        public IActionResult PackagesWithTag(string tag)
        {
            try
            {
                ViewData["tag"] = tag;
                ViewData["packages"] = _packageList.GetWithTag(tag, 0, _settings.ListPageSize);
                return View();
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

            if (freeMegabytes > _settings.SpaceSafetyThreshold){
                return Ok(s.ToString());
            }

            s.AppendLine($"Insufficient space for safe operation - minimum allowed is {_settings.SpaceSafetyThreshold}M.");

            return Responses.InsufficientSpace(s.ToString());
        }

        #endregion
    }
}

