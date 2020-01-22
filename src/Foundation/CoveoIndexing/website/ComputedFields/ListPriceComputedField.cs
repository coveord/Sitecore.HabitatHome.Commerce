using Coveo.Framework.CNL;
using Coveo.Framework.Items;
using Coveo.Framework.Log;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.ComputedFields;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;

namespace Sitecore.HabitatHome.Foundation.CoveoIndexing.ComputedFields
{
    public class ListPriceComputedField : IComputedIndexField
    {
        private static readonly ILogger s_Logger = CoveoLogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        [ThreadStatic]
        private static HttpClient s_HttpClient = new HttpClient();

        private const string PRODUCT_ID_FIELD = "ProductId";
        private const string PRODUCT_TEMPLATE_ID = "225F8638-2611-4841-9B89-19A5440A1DA1";
        private const string HABITAT_CATALOG_NAME = "habitat_master";
        private const string LIST_PRICE_RESPONSE_AMOUNT_SUBSTRING = "Amount\":";

        public string FieldName { get; set; }

        public string ReturnType
        {
            get
            {
                return "Number";
            }
            set
            {
            }
        }

        public object ComputeFieldValue(IIndexable p_Indexable)
        {
            s_Logger.Trace("Coveo computed field: Resolving item list price");
            Precondition.NotNull(p_Indexable, () => () => p_Indexable);

            IItem item = new ItemWrapper(new IndexableWrapper(p_Indexable));
            object value = null;

            if (item.TemplateID.ToString().ToUpper() == PRODUCT_TEMPLATE_ID)
            {
                value = GetListPrice(item);
            }

            return value;
        }

        private string GetListPrice(IItem p_Item) {
            string listPrice = null;

            string catalogName = GetCatalogName(p_Item);
            if (String.IsNullOrEmpty(catalogName))
            {
                return null;
            }

            string productSku = p_Item.GetFieldValue(PRODUCT_ID_FIELD);
            if (String.IsNullOrEmpty(productSku))
            {
                return null;
            }

            string token = GetToken();
            if (String.IsNullOrEmpty(token))
            {
                return null;
            }

            listPrice = GetPriceFromApi(catalogName, productSku, token);

            return listPrice;
        }

        private string GetPriceFromApi(string catalogName, string productSku, string token)
        {
            string price = null;

            var putData = "{ \"itemIds\": [\"" + catalogName + "|" + productSku + "|\"] }";
            var data = Encoding.ASCII.GetBytes(putData);

            var request = WebRequest.CreateHttp("https://habitathome.coveodemo.com:5000/api/GetBulkPrices()");
            request.ContentType = "application/json";
            request.Headers.Add("ShopName", "CommerceEngineDefaultStorefront");
            request.Headers.Add("ShopperId", "ShopperId");
            request.Headers.Add("Language", "en-US");
            request.Headers.Add("Currency", "USD");
            request.Headers.Add("Environment", "HabitatAuthoring");
            request.Headers.Add("GeoLocation", "IpAddress=1.0.0.0");
            request.Headers.Add("Authorization", "Bearer " + token);
            request.Method = "PUT";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            // Example 8.2.1:
            //{
            //  "@odata.context":"https://habitathome.coveodemo.com:5000/Api/$metadata#Collection(Sitecore.Commerce.Plugin.Catalog.SellableItemPricing)","value":[
            //    {
            //      "Name":"Mens Axelion Running Shoe","Policies":[

            //      ],"ItemId":"ffs504","ListPrice":{
            //        "CurrencyCode":"USD","Amount":74.99
            //      },"SellPrice":{
            //        "CurrencyCode":"USD","Amount":74.99
            //      },"Variations":[
            //        {
            //          "Name":"Mens Axelion Running Shoe 9 - White","Policies":[

            //          ],"ItemId":"4237","ListPrice":{
            //            "CurrencyCode":"USD","Amount":74.99
            //          },"SellPrice":{
            //            "CurrencyCode":"USD","Amount":74.99
            //          }
            //        }
            //      ]
            //    }
            //  ]
            //}

            // Example 9.0.2:
            //{
            //  "@odata.context":"https://habitathome.coveodemo.com:5000/Api/$metadata#Collection(Sitecore.Commerce.Plugin.Catalog.SellableItemPricing)","value":[
            //    {
            //      "Name":"Habitat Spectra 65\u201d 4K LED Ultra HD Television","Policies":[

            //      ],"ItemId":"6042264","ListPrice":{
            //        "CurrencyCode":"USD","Amount":649.5
            //      },"SellPrice":{
            //        "CurrencyCode":"USD","Amount":649.5
            //      },"Variations":[
            //        {
            //          "Name":"Habitat Spectra 65\u201d 4K LED Ultra HD Television","Policies":[

            //          ],"ItemId":"56042264","ListPrice":{
            //            "CurrencyCode":"USD","Amount":649.5
            //          },"SellPrice":{
            //            "CurrencyCode":"USD","Amount":649.5
            //          }
            //        }
            //      ]
            //    }
            //  ]
            //}

            int indexOfProductSkuStart = responseString.IndexOf(productSku);
            int indexOfListPriceStart = responseString.IndexOf(LIST_PRICE_RESPONSE_AMOUNT_SUBSTRING, indexOfProductSkuStart + productSku.Length) + LIST_PRICE_RESPONSE_AMOUNT_SUBSTRING.Length;
            int indexOfListPriceEnd = responseString.IndexOf("\r", indexOfListPriceStart);
            price = responseString.Substring(indexOfListPriceStart, indexOfListPriceEnd - indexOfListPriceStart);

            return price;
        }

        private string GetToken()
        {
            string token = null;
            string password = "b";
            var postData = "password=" + password + "&grant_type=password&username=sitecore\\admin&client_id=postman-api&scope=openid EngineAPI postman_api";
            var data = Encoding.ASCII.GetBytes(postData);

            var request = WebRequest.CreateHttp("https://habitathome.coveodemo.com:5050/connect/token");
            request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "application/json";
            request.Method = "POST";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream()) {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse) request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            // Example: {"access_token":"eyJhbGciOiJSUzI1NiIsImtpZCI6Ijk3RTkwOTE5RTAyNEEzQ0I3MzBERTNFQTVERDI3MTNEODkyQjJDRkIiLCJ0eXAiOiJKV1QiLCJ4NXQiOiJsLWtKR2VBa284dHpEZVBxWGRKeFBZa3JMUHMifQ.eyJuYmYiOjE1MjAwMDM2NzgsImV4cCI6MTUyMDAwNzI3OCwiaXNzIjoiaHR0cHM6Ly9sb2NhbGhvc3Q6NTA1MCIsImF1ZCI6WyJodHRwczovL2xvY2FsaG9zdDo1MDUwL3Jlc291cmNlcyIsIkVuZ2luZUFQSSIsInBvc3RtYW5fYXBpIl0sImNsaWVudF9pZCI6InBvc3RtYW4tYXBpIiwic3ViIjoiNjhmZmZhYTIxZmZlNDAwNmI2NjFhOGI2YjgwYzgxZGUiLCJhdXRoX3RpbWUiOjE1MjAwMDM2NzgsImlkcCI6ImxvY2FsIiwibmFtZSI6InNpdGVjb3JlXFxBZG1pbiIsImVtYWlsIjoiIiwicm9sZSI6WyJzaXRlY29yZVxcQ29tbWVyY2UgQnVzaW5lc3MgVXNlciIsInNpdGVjb3JlXFxQcmljZXIiLCJzaXRlY29yZVxcUHJpY2VyIE1hbmFnZXIiLCJzaXRlY29yZVxcUHJvbW90aW9uZXIiLCJzaXRlY29yZVxcUHJvbW90aW9uZXIgTWFuYWdlciIsInNpdGVjb3JlXFxRQSJdLCJzY29wZSI6WyJvcGVuaWQiLCJFbmdpbmVBUEkiLCJwb3N0bWFuX2FwaSJdLCJhbXIiOlsicHdkIl19.AwJCjN8qkys3-s2X06oQkjKvehhZmtYV-m3iQYSHKsnkZJZEswuLK_pRz-6Q_ZhsQ9vMn4Dp10ECEQLRqtwhnOl5lBzrIxLCaDGxKcz-SY-K9rSC2R2bScRFgeOx3vMxm7PWJ50WQKx6kqTinrsvmZzzxAZlrAp8BNPFtPXV1LNbVBJRS4Xen5NXOudeeoLXEBU-sRy_IAOGkYi8ANqPem9ZxK-oBDVQywB-sOIJAHYLoPf3cXGiAfqbDBwUiFKQAg3oSuY34G-i8nF-nDmM1j69uF14ZeZ4GG5zj7r_XywMbZPOU0XN3d2qztDX-OQFgJAyK-oci0sQH_kGHAH4nA","expires_in":3600,"token_type":"Bearer"}

            int indexOfTokenStart = responseString.IndexOf(":") + 2;
            int indexOfTokenEnd = responseString.IndexOf("\"", indexOfTokenStart);
            token = responseString.Substring(indexOfTokenStart, indexOfTokenEnd - indexOfTokenStart);

            return token;
        }

        private string GetCatalogName(IItem p_Item)
        {
            if (p_Item.Paths.FullPath.ToLower().Contains(HABITAT_CATALOG_NAME))
            {
                return HABITAT_CATALOG_NAME;
            }

            return null;
        }
    }
}
