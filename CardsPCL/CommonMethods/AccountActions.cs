using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
//using UIKit;

namespace CardsPCL.CommonMethods
{
    public class AccountActions
    {
        string main_url = Constants.public_url + "//accountActions";
        public static bool cycledRequestCancelled = false;
        // Passed
        public async Task<string> AccountVerification(string clientName, string email, string udid/*, bool isAndroid = false*/)
        {
            using (HttpClient client = new HttpClient())
            {
                email = email.Replace(" ", string.Empty);
                var email_encoded = WebUtility.UrlEncode(email.ToLower());
                //var udid_encoded = WebUtility.UrlEncode("1338021C-F4D2-47BC-AEE9-D0F381264442");
                var guid_encoded = WebUtility.UrlEncode(udid);
                var textBytes = System.Text.Encoding.UTF8.GetBytes(email_encoded + ":" + guid_encoded);
                var authorization = System.Convert.ToBase64String(textBytes);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorization);
                string myContent;
                //if (!isAndroid)
                //    myContent = JsonConvert.SerializeObject(new { /*pushToken = "any uniqueidentifier",*/ clientName = UIDevice.CurrentDevice.Name });
                //else
                    myContent = JsonConvert.SerializeObject(new { /*pushToken = "any uniqueidentifier",*/ clientName = clientName });
                var content = new StringContent(myContent.ToString(), Encoding.UTF8, "application/json");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var res = await client.PostAsync(main_url + "/AccountVerification", content);
                return await res.Content.ReadAsStringAsync();
            }
        }
        public async Task<string> AccountActionsGet(string actionJwt, string udid)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", actionJwt);

                client.DefaultRequestHeaders.Add(Constants.XClientIdentifier, udid);

                string response = "";
                try
                {
                    response = await client.GetStringAsync(main_url);
                }
                catch (HttpRequestException re)
                {
                    if (re.Message.Contains("401"))
                        if (!cycledRequestCancelled)
                            response = await AccountActionsGet(actionJwt, udid);
                }
                if (!response.Contains("processed"))
                {
                    await Task.Delay(5000);
                    response = await AccountActionsGet(actionJwt, udid);
                }
                return response;
            }
        }
        public async Task<string> AccountPurge(string clientName, string email, string udid/*, bool isAndroid = false*/)
        {
            using (HttpClient client = new HttpClient())
            {
                email = email.Replace(" ", string.Empty);
                var email_encoded = WebUtility.UrlEncode(email.ToLower());
                //var udid_encoded = WebUtility.UrlEncode("1338021C-F4D2-47BC-AEE9-D0F381264442");
                var udid_encoded = WebUtility.UrlEncode(udid);
                var textBytes = System.Text.Encoding.UTF8.GetBytes(email_encoded + ":" + udid_encoded);
                var authorization = System.Convert.ToBase64String(textBytes);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorization);
                string myContent;
                //if (!isAndroid)
                //    myContent = JsonConvert.SerializeObject(new { /*pushToken = "any uniqueidentifier",*/ clientName = UIDevice.CurrentDevice.Name });
                //else
                    myContent = JsonConvert.SerializeObject(new { /*pushToken = "any uniqueidentifier",*/ clientName = clientName });
                var content = new StringContent(myContent.ToString(), Encoding.UTF8, "application/json");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var res = await client.PostAsync(main_url + "/AccountPurge", content);
                return await res.Content.ReadAsStringAsync();
                //var response_result = content_response.Result;
                //return response_result;
            }
        }
    }
}
