using solum.core.http;
using solum.core.http.handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace solum.core.http.handlers
{
    public class StaticDirectoryHandler : AsyncHandler
    {
        public StaticDirectoryHandler(string prefix, string directory)
        {
            m_prefix = sanitizePrefix(prefix);
            m_directory = directory;
        }

        /// <summary>
        /// Match any requests with the specified file path
        /// </summary>
        string m_prefix;
        /// <summary>
        /// The directory to map the prefix to
        /// </summary>
        string m_directory;

        protected override bool OnAcceptRequest(HttpListenerRequest request)
        {
            var requestPath = request.Url.LocalPath;

            return matchPrefix(m_prefix, requestPath);
        }

        // TODO: Add caching support
        protected override Task OnHandleRequestAsync(HttpListenerRequest request, HttpListenerResponse response)
        {
            var requestPath = sanitizePrefix(request.Url.LocalPath).TrimEnd('/');

            // ** Substitude the prefix with the local directory content
            var noPrefix = requestPath.Replace(m_prefix, "");

            //var filePath = "{0}/{1}".format(m_directory, noPrefix);
            var filePath = Path.Combine(m_directory, noPrefix); // "{0}/{1}".format(m_directory, noPrefix);
            var fileInfo = new FileInfo(filePath);

            // TODO: Return 404
            if (!fileInfo.Exists)
                throw new FileNotFoundException("The resource {0} was not found.".format(request.Url));
                
            // ** Map file extension to content type
            var contentType = ContentTypes.GetContentType(filePath);

            // ** Write the file contents to the response stream
            var fileLength = fileInfo.Length;
            var fileStream = fileInfo.OpenRead();

            return response.WriteAsync(contentType, fileLength, fileStream);
        }

        private static string sanitizePrefix(string prefix)
        {
            prefix = prefix.Trim().Replace('\\', '/');

            if (!prefix.StartsWith("/"))
                prefix = "/" + prefix;

            if (!prefix.EndsWith("/"))
                prefix += '/';

            return prefix;
        }
        private static bool matchPrefix(string prefix, string matchWith)
        {
            // Match EXACT
            if (prefix.Equals(matchWith, StringComparison.InvariantCultureIgnoreCase))
                return true;

            // Sanitize
            matchWith = sanitizePrefix(matchWith);

            // Check Exact matching after cleaning up the prefix
            if (prefix.Equals(matchWith, StringComparison.InvariantCultureIgnoreCase))
                return true;

            // Check if this starts with
            if (matchWith.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
                return true;

            return false;
        }
    }
}
