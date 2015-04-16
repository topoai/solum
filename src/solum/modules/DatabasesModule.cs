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
    public class DatabasesModule : WebModule
    {
        public class Column
        {
            public string column { get; set; }
        }

        public class Value
        {
            public object value { get; set; }
        }

        public override void RegisterRoutes()
        {
            var requestNum = 0;

            Get("/databases/", (request) =>
            {
                requestNum++;

                var databases = Server.Current.Storage.Databases();
                var keyValueStores = Server.Current.Storage.KeyValueStores();

                // ** Tranform list of databases to columns and rows
                var databaseColumns = new string[] { "Name", "Num Records", "Is Opened" };
                Func<dynamic, Value[]> databaseToValues = database =>
                {
                    var values = new List<Value>();
                    values.Add(new Value { value = "<b><a href='#'>{0}</a></b>".format((string)database.Name) });
                    values.Add(new Value { value = database.NumRecords });
                    values.Add(new Value { value = database.IsOpened });

                    return values.ToArray();
                };

                var model = new
                {
                    databases = new
                    {
                        columns = databaseColumns.Select(col => new
                        {
                            name = col
                        }),
                        rows = new
                        {
                            values = databases.Select(databaseToValues).ToArray()
                        }
                    },
                    keyValueStores = new
                    {
                        columns = databaseColumns.Select(col => new
                        {
                            name = col
                        }),
                        rows = keyValueStores.Select(store => new
                        {
                            values = databaseToValues(store).ToArray()
                        })
                    }
                };

                return View.FromFile("views/databases.html", model);
            });
        }
    }
}
