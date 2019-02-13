using Sitecore.Diagnostics;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace Sitecore.HabitatHome.Feature.CoveoSearch.UsageAnalytics
{
    public class CoveoUsageAnalyticsClient
    {
        private HttpRequest _IncomingHttpRequest;
        private string _EndpointUrl;

        public CoveoUsageAnalyticsClient()
        {
            _IncomingHttpRequest = HttpContext.Current.Request;
            _EndpointUrl = _IncomingHttpRequest.Url.GetLeftPart(UriPartial.Authority) + "/coveo/rest/coveoanalytics/rest/v15/analytics/custom";
        }

        public void Send(AnalyticsEvent usageAnalyticeEvent) {
            HttpWebRequest usageAnalyticsRequest = (HttpWebRequest) WebRequest.Create(_EndpointUrl);
            usageAnalyticsRequest.Method = "POST";
            usageAnalyticsRequest.ContentType = "application/json";
            usageAnalyticsRequest.CookieContainer = new CookieContainer();
            CopyIncomingRequestCookiesToCookieContainer(usageAnalyticsRequest.CookieContainer, usageAnalyticsRequest.RequestUri.Host);
            
            MemoryStream requestContentStream = new MemoryStream(Encoding.UTF8.GetBytes(usageAnalyticeEvent.ToString()));
            requestContentStream.CopyTo(usageAnalyticsRequest.GetRequestStream());

            HttpWebResponse response = GetResponseWhenPossible(usageAnalyticsRequest);
            response.Close();
        }

        private void CopyIncomingRequestCookiesToCookieContainer(CookieContainer p_CookieContainer, string p_RequestHostName)
        {
            Assert.ArgumentNotNull(p_CookieContainer, "p_CookieContainer");
            Assert.ArgumentNotNullOrEmpty(p_RequestHostName, "p_RequestHostName");

            foreach (string cookieName in _IncomingHttpRequest.Cookies.Keys)
            {
                HttpCookie httpCookie = _IncomingHttpRequest.Cookies[cookieName];
                try
                {
                    Cookie netCookie = new Cookie(httpCookie.Name,
                                                  EnsureValidCookieValue(httpCookie.Value),
                                                  "/",
                                                  p_RequestHostName);
                    p_CookieContainer.Add(netCookie);
                }
                catch (CookieException exc)
                {
                    Log.Warn(String.Format("The cookie \"{0}\" is invalid. It will not be forwarded to the Search API.", cookieName), exc, this);
                }
            }
        }

        private string EnsureValidCookieValue(string p_CookieValue)
        {
            string cookieValue = p_CookieValue ?? "";

            if (cookieValue.Contains(",") || cookieValue.Contains(";"))
            {
                cookieValue = EnsureCookieValueIsEnclosedInDoubleQuotes(cookieValue);
            }

            return cookieValue;
        }

        private string EnsureCookieValueIsEnclosedInDoubleQuotes(string p_CookieValue)
        {
            char[] charToTrim = {
                ' ',
                '"'
            };
            string cookieValue = p_CookieValue.Trim(charToTrim);
            cookieValue = '"' + cookieValue + '"';
            return cookieValue;
        }

        private HttpWebResponse GetResponseWhenPossible(HttpWebRequest p_Request)
        {
            try
            {
                return (HttpWebResponse) p_Request.GetResponse();
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    return (HttpWebResponse) ex.Response;
                }

                throw;
            }
        }
    }
}
