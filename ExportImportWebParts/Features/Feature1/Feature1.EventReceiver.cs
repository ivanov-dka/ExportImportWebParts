using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.SharePoint;
using System.IO;
using System.Xml;
using Microsoft.Office.Server.Search.WebControls;
using System.Linq;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint.Publishing;

namespace ExportImportWebParts.Features.Feature1
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("d5b87a0c-7122-4d59-a25e-b05fd5655605")]
    public class Feature1EventReceiver : SPFeatureReceiver
    {
        // Uncomment the method below to handle the event raised after a feature has been activated.

        const string wpName = "CustomSearchResults.webpart";
        const string wpTitle = "Результаты поиска. Преднастроенная";
        const string pageName = "TestPublishingPage.aspx";

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            SPSite site = properties.Feature.Parent as SPSite;

            PublishingWeb pWeb = PublishingWeb.GetPublishingWeb(site.RootWeb);
            bool pageNotCreated = !pWeb.GetPublishingPages().Cast<PublishingPage>().Any(p => p.Name == pageName);
            if (pageNotCreated)
            {
                //если такой страницы еще нет - создаем ее
                PublishingPage pPage = pWeb.GetPublishingPages().Add(pageName, null);

                pPage.CheckOut();

                using (Microsoft.SharePoint.WebPartPages.SPLimitedWebPartManager wpManager = site.RootWeb.GetLimitedWebPartManager(pPage.Url, PersonalizationScope.Shared))
                {
                    ResultScriptWebPart searchResults = GetImportedWebPart(site, wpName) as ResultScriptWebPart;
                    searchResults.Title = wpTitle;
                    searchResults.ChromeType = PartChromeType.None;

                    wpManager.AddWebPart(searchResults, "Top", 0);
                }

                pPage.CheckIn(string.Empty);
            }
        }


        /// <summary>
        /// Получение импортированной веб части
        /// </summary>
        /// <param name="site">Сайт коллекция</param>
        /// <param name="wpName">Имя веб части</param>
        /// <returns>Объект веб части</returns>
        public static WebPart GetImportedWebPart(SPSite site, string wpName)
        {
            string error = String.Empty;
            var wpSetttings = site.RootWeb.GetFileAsString(String.Format("{0}/_catalogs/wp/{1}", site.RootWeb.Url, wpName));

            using (Microsoft.SharePoint.WebPartPages.SPLimitedWebPartManager wpManager = site.RootWeb.GetLimitedWebPartManager(site.RootWeb.RootFolder.WelcomePage, PersonalizationScope.Shared))
            {
                using (var stringReader = new StringReader(wpSetttings))
                {
                    using (XmlTextReader reader = new XmlTextReader(stringReader))
                    {
                        var wp = wpManager.ImportWebPart(reader, out error);

                        return wp;
                    }
                }
            }
        }

        // Uncomment the method below to handle the event raised before a feature is deactivated.

        //public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        //{
        //}


        // Uncomment the method below to handle the event raised after a feature has been installed.

        //public override void FeatureInstalled(SPFeatureReceiverProperties properties)
        //{
        //}


        // Uncomment the method below to handle the event raised before a feature is uninstalled.

        //public override void FeatureUninstalling(SPFeatureReceiverProperties properties)
        //{
        //}

        // Uncomment the method below to handle the event raised when a feature is upgrading.

        //public override void FeatureUpgrading(SPFeatureReceiverProperties properties, string upgradeActionName, System.Collections.Generic.IDictionary<string, string> parameters)
        //{
        //}
    }
}
