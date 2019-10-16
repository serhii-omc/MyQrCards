using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CardsPCL.Models;
using Newtonsoft.Json;

namespace CardsPCL.CommonMethods
{
    public class Companies
    {
        string main_url = Constants.public_url + "/companies";
        public async Task<string> CreateCompanyCard(string accessJwt, string udid, CompanyCardModel company_card_obj, int? logo_id = null)
        {
            using (HttpClient client = new HttpClient())
            {
                //string response_result;
                if (company_card_obj != null)
                {
                    string myContent = "";
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessJwt);

                    client.DefaultRequestHeaders.Add(Constants.XClientIdentifier, udid);

                    if (logo_id != null && logo_id != 0)
                    {
                        company_card_obj.LogoAttachmentID = Convert.ToInt32(logo_id);
                        myContent = JsonConvert.SerializeObject(company_card_obj);
                    }
                    else
                    {
                        CompanyCardModelWithoutLogo companyCardModelWithoutLogo_obj = new CompanyCardModelWithoutLogo();// as company_card_obj
                        companyCardModelWithoutLogo_obj.Activity = company_card_obj.Activity;
                        companyCardModelWithoutLogo_obj.Customers = company_card_obj.Customers;
                        companyCardModelWithoutLogo_obj.Email = company_card_obj.Email;
                        companyCardModelWithoutLogo_obj.Fax = company_card_obj.Fax;
                        companyCardModelWithoutLogo_obj.Name = company_card_obj.Name;
                        companyCardModelWithoutLogo_obj.Phone = company_card_obj.Phone;
                        companyCardModelWithoutLogo_obj.SiteUrl = company_card_obj.SiteUrl;
                        companyCardModelWithoutLogo_obj.FoundedYear = company_card_obj.FoundedYear;
                        companyCardModelWithoutLogo_obj.Location = company_card_obj.Location;
                        companyCardModelWithoutLogo_obj.SocialNetworks = company_card_obj.SocialNetworks;
                        myContent = JsonConvert.SerializeObject(companyCardModelWithoutLogo_obj);
                    }

                    var content = new StringContent(myContent.ToString(), Encoding.UTF8, "application/json");
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var res = await client.PostAsync(main_url, content);
                    if (res.StatusCode.ToString().ToLower().Contains(Constants.status_code401) || res.StatusCode.ToString().ToLower().Contains("401"))
                        return Constants.image_upload_status_code401.ToString(); //means that we got 401
                    return await res.Content.ReadAsStringAsync();
                    //var response_result = content_response.Result;
                    //return response_result;
                }
                else
                    return null;
            }
        }
        public async Task<string> UpdateCompanyCard(string accessJwt, string udid, CompanyCardModel company_card_obj, int? company_id, int? logo_id = null)
        {
            using (HttpClient client = new HttpClient())
            {
                string myContent = "";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessJwt);

                client.DefaultRequestHeaders.Add(Constants.XClientIdentifier, udid);
                //myContent = JsonConvert.SerializeObject(companyCardModelWithoutLogo_obj);

                if (logo_id != null)
                {
                    company_card_obj.LogoAttachmentID = Convert.ToInt32(logo_id);
                    //myContent = JsonConvert.SerializeObject(company_card_obj);
                }
                else
                {
                    company_card_obj.LogoAttachmentID = null;
                    //CompanyCardModelWithoutLogo companyCardModelWithoutLogo_obj = new CompanyCardModelWithoutLogo();// as company_card_obj
                    //companyCardModelWithoutLogo_obj.Activity = company_card_obj.Activity;
                    //companyCardModelWithoutLogo_obj.Customers = company_card_obj.Customers;
                    //companyCardModelWithoutLogo_obj.Email = company_card_obj.Email;
                    //companyCardModelWithoutLogo_obj.Fax = company_card_obj.Fax;
                    //companyCardModelWithoutLogo_obj.Name = company_card_obj.Name;
                    //companyCardModelWithoutLogo_obj.Phone = company_card_obj.Phone;
                    //companyCardModelWithoutLogo_obj.SiteUrl = company_card_obj.SiteUrl;
                    //companyCardModelWithoutLogo_obj.FoundedYear = company_card_obj.FoundedYear;
                    //companyCardModelWithoutLogo_obj.Location = company_card_obj.Location;
                    //companyCardModelWithoutLogo_obj.SocialNetworks = company_card_obj.SocialNetworks;
                    //myContent = JsonConvert.SerializeObject(companyCardModelWithoutLogo_obj);
                }
                myContent = JsonConvert.SerializeObject(company_card_obj);

                var content = new StringContent(myContent.ToString(), Encoding.UTF8, "application/json");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var res = await client.PutAsync(main_url + "/" + company_id, content);
                if (res.StatusCode.ToString().ToLower().Contains(Constants.status_code401) || res.StatusCode.ToString().ToLower().Contains("401"))
                    return Constants.image_upload_status_code401.ToString(); //means that we got 401
                var content_response = await res.Content.ReadAsStringAsync();
                var response_result = content_response;
                return response_result;
            }
        }
    }
}
