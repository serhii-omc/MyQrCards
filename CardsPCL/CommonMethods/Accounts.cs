using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CardsPCL.Models;
using Newtonsoft.Json;
using Plugin.InAppBilling;
using Plugin.InAppBilling.Abstractions;

namespace CardsPCL.CommonMethods
{
    public class Accounts
    {
        string main_url = Constants.public_url + "//accounts";
        public async Task<string> AccountAuthorize(string accountClientJwt, string udid)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accountClientJwt);

                client.DefaultRequestHeaders.Add(Constants.XClientIdentifier, udid);

                var res = await client.PostAsync(main_url + "/authorize", null);
                var content_response = await res.Content.ReadAsStringAsync();
                var response_result = content_response;//.Result;
                if (res.StatusCode.ToString().ToLower() == Constants.status_code401)
                    return Constants.status_code401;
                if (res.StatusCode.ToString().ToLower() == Constants.status_code409)
                    return Constants.status_code409;
                return response_result;
            }
        }
        public async Task<string> ApplyUserTerms(string accessJwt, DateTime? dateTime, string udid)
        {
            using (HttpClient client = new HttpClient())
            {
                var myContent = JsonConvert.SerializeObject(new { timestamp = dateTime, clientName = udid/*UIDevice.CurrentDevice.Name.ToString() */});
                var content = new StringContent(myContent.ToString(), Encoding.UTF8, "application/json");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessJwt);
                var res = await client.PutAsync(main_url + "/this/acceptUserTerms", content);
                return res.StatusCode.ToString();
            }
        }
        public async Task<string> AccountSubscribe(string accountClientJwt, string SubscriptionId, InAppBillingPurchase purchase, DateTime ValidTill, string udid)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accountClientJwt);

                var token = JsonConvert.SerializeObject(new
                {
                    AutoRenewing = purchase.AutoRenewing,
                    Id = purchase.Id,
                    Payload = purchase.Payload,
                    ProductId = purchase.ProductId,
                    PurchaseToken = purchase.PurchaseToken,
                    State = purchase.State,
                    TransactionDateUtc = purchase.TransactionDateUtc
                });

                var tokenByteArray = System.Text.Encoding.UTF8.GetBytes(token);
                var tokenBase64 = Convert.ToBase64String(tokenByteArray);

                var myContent = JsonConvert.SerializeObject(new
                {
                    //clientName = udid,//UIDevice.CurrentDevice.Name.ToString(),
                    SubscriptionId = SubscriptionId,
                    SubscriptionToken = tokenBase64,
                    ValidTill = ValidTill
                });
                var content = new StringContent(myContent.ToString(), Encoding.UTF8, "application/json");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var res = await client.PostAsync(main_url + "/this/subscribe", content);
                var content_response = await res.Content.ReadAsStringAsync();
                var response_result = content_response;//.Result;
                if (res.StatusCode.ToString().ToLower() == Constants.status_code401)
                    return Constants.status_code401;
                if (res.StatusCode.ToString().ToLower() == Constants.status_code409)
                    return Constants.status_code409;
                return response_result;
            }
        }

        public async Task<string> AccountSubscribeAndroid(string accountClientJwt, int subscriptionId, string productId, string purchaseToken)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accountClientJwt);
                var receipt = JsonConvert.SerializeObject(new Receipt { ProductId = productId, PurchaseToken = purchaseToken });
                var receiptByteArray = System.Text.Encoding.UTF8.GetBytes(receipt);
                var receiptBase64 = Convert.ToBase64String(receiptByteArray);

                var myContent = JsonConvert.SerializeObject(new
                {
                    SubscriptionID = subscriptionId,
                    SubscriptionToken = receiptBase64
                });

                var content = new StringContent(myContent.ToString(), Encoding.UTF8, "application/json");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var res = await client.PostAsync(main_url + "/this/subscribe", content);
                var contentResponse = await res.Content.ReadAsStringAsync();
                var responseResult = contentResponse;//.Result;
                if (res.StatusCode.ToString().ToLower() == Constants.status_code401)
                    return Constants.status_code401;
                if (res.StatusCode.ToString().ToLower() == Constants.status_code409)
                    return Constants.status_code409;
                return responseResult;
            }
        }
    }
}
