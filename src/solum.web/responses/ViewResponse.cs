using HandlebarsDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.web.responses
{
    public static class View
    {
        public static ViewResponse<object> FromFile(string filePath)
        {
            return new ViewResponse<object>(filePath, null);
        }

        public static ViewResponse<T> FromFile<T>(string filePath, T model)
        {
            return new ViewResponse<T>(filePath, model);
        }
    }

    public class ViewResponse<T> : WebResponse
    {
        public ViewResponse(string viewPath, T model) : base("text/html")
        {
            this.ViewPath = viewPath;
            this.Model = model;
        }

        public string ViewPath { get; private set; }
        public T Model { get; private set; }

        /// <summary>
        /// The compiled view to render
        /// </summary>
        Func<object, string> m_template;

        public override System.IO.Stream Content
        {
            get
            {
                CompileTemplate();

                var content = m_template(Model);
                return content.toStream();
            }
        }

        void CompileTemplate()
        {
            if (!File.Exists(ViewPath))
                throw new FileNotFoundException("The template file was not found: {0}".format(ViewPath));

            var template = File.ReadAllText(ViewPath);
            try
            {
                Log.Debug("Compiling view... {0}", ViewPath);
                this.m_template = Handlebars.Compile(template);
            }
            catch (Exception ex)
            {
                throw new Exception("Error compiling template: {0}".format(ex.Message), ex);
            }
        }
    }
}
