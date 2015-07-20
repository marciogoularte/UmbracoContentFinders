using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Web.Configuration;
using System.Xml;
using Umbraco.Web.Routing;

namespace Codevos.Umbraco.Web.Routing
{
    /// <summary>
    /// Adds HTTP redirect support, e.g. for updating search engine indexes after site migrations.
    /// </summary>
    public class SeoRedirectContentFinder : IContentFinder
    {
        #region Private properties

        private static ConcurrentDictionary<string, RedirectInfo> RedirectFromTo = new ConcurrentDictionary<string, RedirectInfo>(StringComparer.OrdinalIgnoreCase);

        #endregion


        #region Public methods

        public bool TryFindContent(PublishedContentRequest contentRequest)
        {
            EnsureDictionaryLoaded(contentRequest);
            return TryRedirect(contentRequest, true) || TryRedirect(contentRequest, false);
        }

        #endregion


        #region Private methods

        private static void EnsureDictionaryLoaded(PublishedContentRequest contentRequest)
        {
            if (RedirectFromTo.Count == 0)
            {
                string serverPath = WebConfigurationManager.AppSettings["SeoRedirectContentFinder.RedirectFilePath"];
                if (string.IsNullOrWhiteSpace(serverPath))
                    serverPath = "~/Config/SeoRedirect.config";

                string configPath = contentRequest.RoutingContext.UmbracoContext.HttpContext.Server.MapPath(serverPath);
                
                if (File.Exists(configPath))
                {
                    XmlDocument xmlDocument = new XmlDocument();

                    using (StreamReader reader = new StreamReader(configPath, Encoding.UTF8))
                    {
                        xmlDocument.Load(reader);
                    }

                    foreach (XmlElement domainElement in xmlDocument.DocumentElement.ChildNodes)
                    {
                        string domain = domainElement.GetAttribute("name");

                        foreach (XmlElement redirectElement in domainElement.ChildNodes)
                        {
                            string from = redirectElement.GetAttribute("from");

                            int statusCode;
                            int.TryParse(redirectElement.GetAttribute("status"), out statusCode);

                            RedirectFromTo[string.Format("{0}{1}", domain, from)] = new RedirectInfo()
                            {
                                From = from,
                                To = redirectElement.GetAttribute("to"),
                                StatusCode = statusCode > 0 ? statusCode : 302
                            };
                        }
                    }
                }
            }
        }

        private static bool TryRedirect(PublishedContentRequest contentRequest, bool includeQuery)
        {
            bool redirect = false;
            string path = string.Format("{0}{1}{2}", contentRequest.Uri.Authority
                                                   , contentRequest.Uri.AbsolutePath
                                                   , includeQuery ? contentRequest.Uri.Query : String.Empty
                                       );

            RedirectInfo redirectInfo;
            
            if (RedirectFromTo.TryGetValue(path, out redirectInfo) && !string.IsNullOrWhiteSpace(redirectInfo.To))
            {
                contentRequest.SetRedirect(redirectInfo.To, redirectInfo.StatusCode);
                redirect = true;
            }

            return redirect;
        }

        #endregion
    }

    class RedirectInfo
    {
        public string From { get; set; }
        public string To { get; set; }
        public int StatusCode { get; set; }
    }
}
