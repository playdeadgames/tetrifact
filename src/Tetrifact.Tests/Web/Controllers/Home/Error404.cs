﻿using Xunit;
using W=Tetrifact.Web;

namespace Tetrifact.Tests.Web.Controllers.Home
{
    public class Error404 : FileSystemBase
    {
        /// <summary>
        /// coverage
        /// </summary>
        [Fact]
        public void Happy_path()
        {
            W.HomeController controller = NinjectHelper.Get<W.HomeController>("settings", Settings);
            controller.Error404();
        }
    }
}
