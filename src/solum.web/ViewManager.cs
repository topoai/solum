using HandlebarsDotNet;
using Newtonsoft.Json;
using solum.core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.web
{
    public class ViewManager : Component, IViewManager
    {
        const string DEFAULT_TEMPLATE_DIRECTORY = "./templates/";

        public ViewManager(string templatesDirectory = DEFAULT_TEMPLATE_DIRECTORY)
        {
            this.TemplatesDirectory = templatesDirectory;
        }

        [JsonProperty("templates-directory")]
        public string TemplatesDirectory { get; private set; }

        public void RegisterTemplates()
        {
            if (Directory.Exists(TemplatesDirectory) == false)
            {
                Log.Warning("Templates directory not found: {0}", TemplatesDirectory);
                return;
            }

            var templateFiles = Directory.GetFiles(TemplatesDirectory);

            Log.Verbose("Found {0:N} files in the templates directory.", templateFiles.Length);
            foreach (var templateFileName in templateFiles)
            {
                Log.Debug("Processing template file... {0}", templateFileName);

                var templateName = Path.GetFileNameWithoutExtension(templateFileName);
                Log.Verbose("Processing template name... {0}", templateName);

                Log.Verbose("Reading file contents...");
                var contents = File.ReadAllText(templateFileName);                

                using (var reader = new StringReader(contents))
                {
                    Log.Verbose("Compiling template...", templateName);
                    var template = Handlebars.Compile(reader);

                    Log.Verbose("Registering template...");
                    Handlebars.RegisterTemplate(templateName, template);
                }
            }
        }
    }
}
