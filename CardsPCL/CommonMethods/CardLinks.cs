using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CardsPCL.CommonMethods
{
    public class CardLinks
    {
        public async Task<HttpResponseMessage> CardsLinksGet(string accessJwt, string udid, string cardLinkId)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessJwt);

                client.DefaultRequestHeaders.Add(Constants.XClientIdentifier, udid);

                HttpResponseMessage response = null;
                try
                {
                    response = await client.GetAsync($"{Constants.public_url}/cardLinks/{cardLinkId}");
                }
                catch (HttpRequestException re)
                {
                    var errorResponseMessage = new HttpResponseMessage();
                    if (re.Message.ToLower().Contains(Constants.status_code401))
                        errorResponseMessage.StatusCode = System.Net.HttpStatusCode.Unauthorized;
                    if (re.Message.ToLower().Contains(Constants.status_code403))
                        errorResponseMessage.StatusCode = System.Net.HttpStatusCode.Forbidden;
                    if (re.Message.ToLower().Contains(Constants.status_code404))
                        errorResponseMessage.StatusCode = System.Net.HttpStatusCode.NotFound;
                    if (re.Message.ToLower().Contains(Constants.status_code409))
                        errorResponseMessage.StatusCode = System.Net.HttpStatusCode.Conflict;
                    if (re.Message.ToLower().Contains(Constants.status_code200))
                        errorResponseMessage.StatusCode = System.Net.HttpStatusCode.OK;
                    if (re.Message.ToLower().Contains(Constants.status_code204))
                        errorResponseMessage.StatusCode = System.Net.HttpStatusCode.NoContent;
                    return errorResponseMessage;
                }

                return response;
            }
        }

        public async Task<string> LinkCard(string accessJwt, string udid, string cardId, string cardLinkId)
        {
            using (HttpClient client = new HttpClient())
            {
                var myContent = JsonConvert.SerializeObject(new { clientName = udid });
                var content = new StringContent(myContent, Encoding.UTF8, "application/json");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessJwt);
                HttpResponseMessage response = null;
                try
                {
                    response = await client.PutAsync($"{Constants.public_url}/cards/{cardId}/links/{cardLinkId}", content);
                }
                catch (HttpRequestException re)
                {
                    if (re.Message.ToLower().Contains(Constants.status_code401))
                        return Constants.status_code401;
                    if (re.Message.ToLower().Contains(Constants.status_code403))
                        return Constants.status_code403;
                    if (re.Message.ToLower().Contains(Constants.status_code404))
                        return Constants.status_code404;
                    if (re.Message.ToLower().Contains(Constants.status_code200))
                        return Constants.status_code200;
                    if (re.Message.ToLower().Contains(Constants.status_code204))
                        return Constants.status_code204;
                }

                return response?.StatusCode.ToString();
            }
        }
    }
}
