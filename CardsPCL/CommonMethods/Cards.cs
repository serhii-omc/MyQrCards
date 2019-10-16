using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CardsPCL.Database;
using CardsPCL.Models;
using Newtonsoft.Json;

namespace CardsPCL.CommonMethods
{
    public class Cards
    {
        DatabaseMethodsIOS databaseMethods = new DatabaseMethodsIOS();
        string main_url = Constants.public_url + "/cards";
        public static string ETagNew { get; private set; }
        public async Task<string> CreatePersonalCard(string accessJwt, PersonalCardModel personal_card_obj, List<int> attachments, string udid)
        {
            personal_card_obj.SubscriptionID = null;
            using (HttpClient client = new HttpClient())
            {
                if (attachments != null)
                    if (attachments.Count != 0)
                        personal_card_obj.Person.Attachments = attachments;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessJwt);

                client.DefaultRequestHeaders.Add(Constants.XClientIdentifier, udid);

                personal_card_obj.Employment.Email = null;
                var myContent = JsonConvert.SerializeObject(personal_card_obj/*new { pushToken = "any uniqueidentifier", clientName = "Application" }*/);
                var content = new StringContent(myContent.ToString(), Encoding.UTF8, "application/json");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var res = await client.PostAsync(main_url, content);
                if (res.StatusCode.ToString().ToLower().Contains(Constants.status_code401) || res.StatusCode.ToString().ToLower().Contains("401"))
                    return Constants.image_upload_status_code401.ToString(); //means that we got 401
                return await res.Content.ReadAsStringAsync();
                //var response_result = content_response.Result;

                //return response_result;
            }
        }

        public async Task<string> CardsListGetEtag(string accessJwt, string udid)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessJwt);

                client.DefaultRequestHeaders.Add(Constants.XClientIdentifier, udid);

                string EtagOld = databaseMethods.GetEtag();
                if (!String.IsNullOrEmpty(EtagOld))
                    client.DefaultRequestHeaders.TryAddWithoutValidation("If-None-Match", EtagOld);
                string response = "";
                //string ETagNew = "";
                try
                {
                    //TODO
                    //await Task.Delay(7000);
                    var responseMessage = await client.GetAsync(main_url);
                    response = await responseMessage.Content.ReadAsStringAsync();

                    if (responseMessage.StatusCode == System.Net.HttpStatusCode.NotModified)
                        return Constants.status_code304;
                    ETagNew = responseMessage.Headers.GetValues("ETag").FirstOrDefault();
                    //databaseMethods.InsertEtag(ETagNew);
                }
                catch (HttpRequestException re)
                {
                    if (re.Message.ToLower().Contains(Constants.status_code401))
                        return Constants.status_code401;
                }
                return response;
            }
        }

        public async Task<string> CardsListGet(string accessJwt, string udid)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessJwt);

                client.DefaultRequestHeaders.Add(Constants.XClientIdentifier, udid);

                string response = "";
                try
                {
                    response = await client.GetStringAsync(main_url);
                    //response.
                }
                catch (HttpRequestException re)
                {
                    if (re.Message.ToLower().Contains(Constants.status_code401))
                        return Constants.status_code401;
                }
                //
                //if (response.StatusCode.ToString().ToLower() == Constants.status_code409)
                //return Constants.status_code409;


                return response;
            }
        }
        public async Task<string> CardDataGet(string accessJwt, int card_id, string udid)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessJwt);

                client.DefaultRequestHeaders.Add(Constants.XClientIdentifier, udid);

                string response = "";
                try
                {
                    response = await client.GetStringAsync(main_url + "/" + card_id);
                }
                catch (HttpRequestException re)
                {
                    if (re.Message.ToLower().Contains(Constants.status_code401))
                        return Constants.status_code401;
                    if (re.Message.ToLower().Contains(Constants.status_code409))
                        return Constants.status_code409;
                }
                catch (ArgumentNullException)
                {
                    return null;
                }

                return response;
            }
        }
        public async Task<HttpResponseMessage> CardDelete(string accessJwt, int card_id, string udid)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessJwt);

                client.DefaultRequestHeaders.Add(Constants.XClientIdentifier, udid);

                HttpResponseMessage response = null;
                try
                {
                    response = await client.DeleteAsync(main_url + "/" + card_id);
                }
                catch (HttpRequestException re)
                {

                }

                return response;
            }
        }
        public async Task<HttpResponseMessage> CardUpdate(string accessJwt, int card_id, PersonalCardModel personal_card_obj, bool is_primary, List<SocialNetworkModel> socialNetworksList, List<int> attachments, string udid)
        {
            using (HttpClient client = new HttpClient())
            {
                personal_card_obj.SubscriptionID = null;
                if (attachments.Count != 0)
                    personal_card_obj.Person.Attachments = attachments;
                if (personal_card_obj == null)
                {
                    var res_card_data = await CardDataGet(databaseMethods.GetAccessJwt(), card_id, udid);
                    personal_card_obj = JsonConvert.DeserializeObject<PersonalCardModel>(res_card_data);
                }
                string myContent;
                if (personal_card_obj.Person.SocialNetworks == null)
                    personal_card_obj.Person.SocialNetworks = socialNetworksList;
                else if (personal_card_obj.Person.SocialNetworks.Count == 0)
                    personal_card_obj.Person.SocialNetworks = socialNetworksList;
                personal_card_obj.Employment.Email = null;
                if (is_primary)
                {
                    personal_card_obj.IsPrimary = true;
                    myContent = JsonConvert.SerializeObject(personal_card_obj);//new { pushToken = "any uniqueidentifier", clientName = "Application" });
                }
                else
                {
                    PersonalCardModelWithoutPrimary cardModelWithoutPrimary = new PersonalCardModelWithoutPrimary();
                    //cardModelWithoutPrimary.SubscriptionID = personal_card_obj.SubscriptionID;
                    cardModelWithoutPrimary.Person = personal_card_obj.Person;
                    cardModelWithoutPrimary.Name = personal_card_obj.Name;
                    cardModelWithoutPrimary.Employment = personal_card_obj.Employment;
                    cardModelWithoutPrimary.Culture = personal_card_obj.Culture;
                    myContent = JsonConvert.SerializeObject(cardModelWithoutPrimary);
                }

                var content = new StringContent(myContent.ToString(), Encoding.UTF8, "application/json");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessJwt);

                client.DefaultRequestHeaders.Add(Constants.XClientIdentifier, udid);

                HttpResponseMessage response = null;
                try
                {
                    response = await client.PutAsync(main_url + "/" + card_id, content);
                    var content_response = await response.Content.ReadAsStringAsync();
                }
                catch (HttpRequestException re)
                {

                }

                return response;
            }
        }
    }
}
