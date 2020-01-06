using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using TimesOld;

namespace Times.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PillController : ControllerBase
    {
        private readonly ILogger<TimesController> _logger;

        private readonly IHostingEnvironment _host;
       

        public PillController(ILogger<TimesController> logger, IHostingEnvironment host)
        {
            _logger = logger;
            _host = host;
        }

        [HttpGet]
        public IActionResult Get(string s)
        {
            var path = $"{_host.ContentRootPath}\\pill.json";

            if (String.IsNullOrEmpty(s))
            {
                FileInfo fi = new FileInfo(path);
                string sa = "";
                using (StreamReader sr = fi.OpenText())
                {
                    
                    while ((sa = sr.ReadLine()) != null)
                    {
                       
                    }

                    var test = "";
                    
                }
                return Ok(sa);
            }
            else
            {

                
                var obj = new Response
                {
                    Value = s
                };

                var json = JsonConvert.SerializeObject(obj);

                using (StreamWriter writer = new StreamWriter(path))
                {
                    writer.Write(json);
                }

                return Ok();
            }
            
        }

        

    }

    public class Response
    {
        public string Value { get; set; }
    }
}
