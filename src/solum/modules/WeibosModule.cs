using solum.core;
using solum.core.storage;
using solum.web;
using solum.web.responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.modules
{
    public class WeibosModule : WebModule
    {        
        public override void RegisterRoutes()
        {
            Get("/weibos/", (request) =>
            {
                return View.FromFile("views/weibos.html");
            });
        }
    }
}
