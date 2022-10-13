using DatabaseConverter.Core.Model;
using DatabaseConverter.Core;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseManager.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DatabaseManager.Core;
using SqlAnalyser.Model;
using NetTopologySuite.Triangulate;
using System.Text.RegularExpressions;
using System.Dynamic;

namespace DatabaseManager.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;      

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Translate";
            return View();
        }      

        public IActionResult Translate()
        {
            string source = this.Request.Form["source"];
            var sourceDbType = (DatabaseType)Enum.Parse(typeof(DatabaseType), this.Request.Form["sourceDatabaseType"]);
            var targetDbType = (DatabaseType)Enum.Parse(typeof(DatabaseType), this.Request.Form["targetDatabaseType"]);
            
            try
            {
                TranslateManager translateManager = new TranslateManager();

                string result = translateManager.Translate(sourceDbType, targetDbType, source);

                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message);
            }
        }
   

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }  
}
