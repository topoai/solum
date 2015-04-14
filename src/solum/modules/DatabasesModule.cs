using solum.web;
using solum.web.responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.modules
{
    public class DatabasesModule : WebModule
    {
        public override void RegisterRoutes()
        {
            var requestNum = 0;

            Get("/databases/", (request) =>
            {
                requestNum++;
                return View.FromFile("views/databases.html", request);
            });
        }
    }
}
