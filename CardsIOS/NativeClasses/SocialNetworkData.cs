using System;
using System.Collections.Generic;
using CardsPCL;
using UIKit;

namespace CardsIOS.NativeClasses
{
    public class SocialNetworkData
    {
        public SocialNetworkData()
        {
        }

        public static List<SocialNetworkData> SampleData()
        {
            var newDataList = new List<SocialNetworkData>();
            newDataList.Add(new SocialNetworkData(1, UIImage.FromBundle("facebook.png"), Constants.facebook, Constants.facebookUrl));
            newDataList.Add(new SocialNetworkData(4, UIImage.FromBundle("instagram.png"), Constants.instagram, Constants.instagramUrl));
            newDataList.Add(new SocialNetworkData(3, UIImage.FromBundle("linkedin.png"), Constants.linkedin, Constants.linkedinUrl));
            newDataList.Add(new SocialNetworkData(5, UIImage.FromBundle("twitter.png"), Constants.twitter, Constants.twitterUrl));
            newDataList.Add(new SocialNetworkData(2, UIImage.FromBundle("vk.png"), Constants.vkontakte, Constants.vkontakteUrl));

            return newDataList;
        }

        public SocialNetworkData(int newId, UIImage logo, string nameNetworkLabel, string contactUrl)
        {
            Id = newId;
            Logo = logo;
            NameNetworkLabel = nameNetworkLabel;
            ContactUrl = contactUrl;
        }

        public int Id { get; set; }
        public UIImage Logo { get; set; }
        public string NameNetworkLabel { get; set; }
        public string ContactUrl { get; set; }
    }
}
