using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CardsPCL.Models;
using Newtonsoft.Json;

namespace CardsPCL.CommonMethods
{
    public class Attachments
    {
        string main_url = Constants.public_url + "/attachments";
        public async Task<AttachmentsUploadModel> UploadIOS(string accessJwt, string udid)
        {
            using (HttpClient client = new HttpClient())
            {
                var result_obj = new AttachmentsUploadModel();
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9.eyJpYXQiOjE1MzgwNzI0MjgsIkFjY291bnRDbGllbnRUb2tlbiI6IjVLOU1lbzNKbDNOIiwiQWNjb3VudElEIjoxODIsIkNsaWVudElEIjoiMTMzODAyMUMtRjREMi00N0JDLUFFRTktRDBGMzgxMjY0NDQyIiwiaXNzIjoibXlxcmNhcmRzLmNvbSIsImF1ZCI6ImRldi1hcGkubXlxcmNhcmRzLmNvbSJ9.Lses2r92WN2qNBss_TakK86EcKjO1f8YGTws3zNSXZw");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessJwt);
                client.DefaultRequestHeaders.Add(Constants.XClientIdentifier, udid);
                string result = null;
                #region logo upload
                var documentsLogo = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var logo_cache_dir = Path.Combine(documentsLogo, Constants.CardsLogo);
                if (Directory.Exists(logo_cache_dir))
                {
                    string[] filenames_logo = Directory.GetFiles(logo_cache_dir);
                    foreach (var img in filenames_logo)
                    {
                        byte[] b = File.ReadAllBytes(img);
                        var imageContent = new ByteArrayContent(b, 0, b.Length);
                        //NEEDS TO BE PNG HERE BECAUSE ITS BETTER THEN JPEG IN RESULT
                        imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                        MultipartFormDataContent formData = new MultipartFormDataContent();
                        //NEEDS TO BE PNG HERE BECAUSE ITS BETTER THEN JPEG IN RESULT
                        formData.Add(imageContent, "File", "image.jpeg");
                        var res = await client.PostAsync(main_url, formData);
                        if (res.StatusCode.ToString().ToLower().Contains(Constants.status_code401) || res.StatusCode.ToString().ToLower().Contains("401"))
                            return new AttachmentsUploadModel { logo_id = Constants.image_upload_status_code401 }; //means that we got 401
                        var content_response = await res.Content.ReadAsStringAsync();
                        result = content_response;
                        var logo_id = JsonConvert.DeserializeObject<CompanyLogoModel>(result).id;
                        result_obj.logo_id = logo_id;
                    }
                }
                #endregion logo upload 

                #region personal photos upload
                var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var cards_cache_dir = Path.Combine(documents, Constants.CardsPersonalImages);
                try
                {
                    string[] filenames = Directory.GetFiles(cards_cache_dir);
                    var ids_temp = new List<int>();
                    foreach (var img in filenames)
                    {
                        byte[] b = File.ReadAllBytes(img);
                        var imageContent = new ByteArrayContent(b, 0, b.Length);
                        //NEEDS TO BE PNG HERE BECAUSE ITS BETTER THEN JPEG IN RESULT
                        imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                        MultipartFormDataContent formData = new MultipartFormDataContent();
                        //NEEDS TO BE PNG HERE BECAUSE ITS BETTER THEN JPEG IN RESULT
                        formData.Add(imageContent, "File", "image.jpeg");
                        var res = await client.PostAsync(main_url, formData);
                        if (res.StatusCode.ToString().ToLower().Contains(Constants.status_code401) || res.StatusCode.ToString().ToLower().Contains("401"))
                            return new AttachmentsUploadModel { logo_id = Constants.image_upload_status_code401 }; //means that we got 401
                        var content_response = await res.Content.ReadAsStringAsync();
                        result = content_response;
                        var attachment_id = JsonConvert.DeserializeObject<CompanyLogoModel>(result).id;
                        ids_temp.Add(attachment_id);
                    }
                    result_obj.attachments_ids = ids_temp;
                }
                catch { }
                #endregion personal photos upload
                return result_obj;
            }
        }

        public async Task<AttachmentsUploadModel> UploadAndroid(string accessJwt, List<byte[]> imagesBytesList = null, byte[] logoBytes = null)
        {
            using (HttpClient client = new HttpClient())
            {
                var result_obj = new AttachmentsUploadModel();
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9.eyJpYXQiOjE1MzgwNzI0MjgsIkFjY291bnRDbGllbnRUb2tlbiI6IjVLOU1lbzNKbDNOIiwiQWNjb3VudElEIjoxODIsIkNsaWVudElEIjoiMTMzODAyMUMtRjREMi00N0JDLUFFRTktRDBGMzgxMjY0NDQyIiwiaXNzIjoibXlxcmNhcmRzLmNvbSIsImF1ZCI6ImRldi1hcGkubXlxcmNhcmRzLmNvbSJ9.Lses2r92WN2qNBss_TakK86EcKjO1f8YGTws3zNSXZw");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessJwt);
                string result = null;
                #region logo upload
                //var documentsLogo = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                //var logo_cache_dir = Path.Combine(documentsLogo, Constants.CardsLogo);
                if (logoBytes != null)//Directory.Exists(logo_cache_dir))
                {
                    //string[] filenames_logo = Directory.GetFiles(logo_cache_dir);
                    //foreach (var img in filenames_logo)
                    //{
                    //byte[] b = File.ReadAllBytes(img);
                    //var imageContent = new ByteArrayContent(b, 0, b.Length);
                    var imageContent = new ByteArrayContent(logoBytes, 0, logoBytes.Length);
                    //NEEDS TO BE PNG HERE BECAUSE ITS BETTER THEN JPEG IN RESULT
                    imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                    MultipartFormDataContent formData = new MultipartFormDataContent();
                    //NEEDS TO BE PNG HERE BECAUSE ITS BETTER THEN JPEG IN RESULT
                    formData.Add(imageContent, "File", "image.jpeg");
                    var res = await client.PostAsync(main_url, formData);
                    if (res.StatusCode.ToString().ToLower().Contains(Constants.status_code401) || res.StatusCode.ToString().ToLower().Contains("401"))
                        return new AttachmentsUploadModel { logo_id = Constants.image_upload_status_code401 }; //means that we got 401
                    var content_response = await res.Content.ReadAsStringAsync();
                    result = content_response;
                    var logo_id = JsonConvert.DeserializeObject<CompanyLogoModel>(result).id;
                    result_obj.logo_id = logo_id;
                    //}
                }
                #endregion logo upload 

                #region personal photos upload
                //var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                //var cards_cache_dir = Path.Combine(documents, Constants.CardsPersonalImages);
                //try
                //{
                //string[] filenames = Directory.GetFiles(cards_cache_dir);
                if (imagesBytesList != null)
                {
                    var ids_temp = new List<int>();
                    int i = 0;
                    foreach (var img in imagesBytesList)
                    {
                        //byte[] b = File.ReadAllBytes(img);
                        var imageContent = new ByteArrayContent(imagesBytesList[i], 0, imagesBytesList[i].Length);
                        //NEEDS TO BE PNG HERE BECAUSE ITS BETTER THEN JPEG IN RESULT
                        imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                        MultipartFormDataContent formData = new MultipartFormDataContent();
                        //NEEDS TO BE PNG HERE BECAUSE ITS BETTER THEN JPEG IN RESULT
                        formData.Add(imageContent, "File", "image.jpeg");
                        var res = await client.PostAsync(main_url, formData);
                        if (res.StatusCode.ToString().ToLower().Contains(Constants.status_code401) || res.StatusCode.ToString().ToLower().Contains("401"))
                            return new AttachmentsUploadModel { logo_id = Constants.image_upload_status_code401 }; //means that we got 401
                        var content_response = await res.Content.ReadAsStringAsync();
                        result = content_response;
                        var attachment_id = JsonConvert.DeserializeObject<CompanyLogoModel>(result).id;
                        ids_temp.Add(attachment_id);
                        i++;
                    }
                    result_obj.attachments_ids = ids_temp;
                }
                //catch { }
                #endregion personal photos upload
                return result_obj;
            }
        }
    }
}
