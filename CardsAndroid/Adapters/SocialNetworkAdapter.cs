using System;
using System.Collections.Generic;
using System.Globalization;
using Android.App;
using Android.Support.V7.Widget;
using Android.Views;
using CardsAndroid.ViewHolders;
using CardsAndroid.Models;
using CardsPCL;
using CardsAndroid.Activities;
using Android.Graphics;

namespace CardsAndroid.Adapters
{
    public class SocialNetworkAdapter : RecyclerView.Adapter
    {
        private Activity _context;
        Typeface _tf;
        public static List<SocialNetworkModel> SocialNetworks = new List<SocialNetworkModel> { new SocialNetworkModel { Id =1, SocialNetworkName = Constants.facebook, UsersUrl = null },
                                                                                                new SocialNetworkModel {Id = 4, SocialNetworkName = Constants.instagram, UsersUrl = null },
                                                                                                new SocialNetworkModel { Id = 3, SocialNetworkName = Constants.linkedin, UsersUrl = null },
                                                                                                new SocialNetworkModel {Id=5, SocialNetworkName = Constants.twitter, UsersUrl = null },
                                                                                                new SocialNetworkModel {Id = 2, SocialNetworkName = Constants.vkontakte, UsersUrl = null } };
        CultureInfo _ci;
        SocialNetworkViewHolder _socialNetworkViewHolder;

        public SocialNetworkAdapter(Activity context, CultureInfo ci, Typeface tf)
        {
            this._context = context;
            this._ci = ci;
            this._tf = tf;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            _socialNetworkViewHolder = (SocialNetworkViewHolder)holder;
            _socialNetworkViewHolder.SocialNetwNameTv.Text = SocialNetworks[position].SocialNetworkName;
            if (position == 0)
                _socialNetworkViewHolder.SocialNetwIv.SetBackgroundResource(Resource.Drawable.facebook);
            if (position == 1)
                _socialNetworkViewHolder.SocialNetwIv.SetBackgroundResource(Resource.Drawable.instagram);
            if (position == 2)
                _socialNetworkViewHolder.SocialNetwIv.SetBackgroundResource(Resource.Drawable.linkedin);
            if (position == 3)
                _socialNetworkViewHolder.SocialNetwIv.SetBackgroundResource(Resource.Drawable.twitter);
            if (position == 4)
                _socialNetworkViewHolder.SocialNetwIv.SetBackgroundResource(Resource.Drawable.vk);
            if (String.IsNullOrEmpty(SocialNetworks[position].UsersUrl))
                _socialNetworkViewHolder.CheckIv.Visibility = ViewStates.Gone;
            else
                _socialNetworkViewHolder.CheckIv.Visibility = ViewStates.Visible;
            _socialNetworkViewHolder.SocialNetwNameTv.SetTypeface(_tf, TypefaceStyle.Normal);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var layout = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.SocialNetworkRow, parent, false);
            return new SocialNetworkViewHolder(layout, OnClick);
        }

        public override int ItemCount
        {
            get { return SocialNetworks.Count; }
        }

        void OnClick(int position)
        {
            if (position == 0)
            {
                WebViewSocialToChooseActivity.UrlRoot = Constants.facebookUrl;
                WebViewSocialToChooseActivity.HeaderValue = Constants.facebook;
            }
            else if (position == 1)
            {
                WebViewSocialToChooseActivity.UrlRoot = Constants.instagramUrl;
                WebViewSocialToChooseActivity.HeaderValue = Constants.instagram;
            }
            else if (position == 2)
            {
                WebViewSocialToChooseActivity.UrlRoot = Constants.linkedinUrl;
                WebViewSocialToChooseActivity.HeaderValue = Constants.linkedin;
            }
            else if (position == 3)
            {
                WebViewSocialToChooseActivity.UrlRoot = Constants.twitterUrl;
                WebViewSocialToChooseActivity.HeaderValue = Constants.twitter;
            }
            else if (position == 4)
            {
                WebViewSocialToChooseActivity.UrlRoot = Constants.vkontakteUrl;
                WebViewSocialToChooseActivity.HeaderValue = Constants.vkontakte;
            }
            //Reload(position);
            WebViewSocialToChooseActivity.UrlString = SocialNetworks[position].UsersUrl;
            _context.StartActivity(typeof(WebViewSocialToChooseActivity));
        }

        private void Reload(int position)
        {
            //var oldValue = socialNetworks[position].chosen;
            //bool newValue;
            //if (oldValue)
            //    newValue = false;
            //else
            //    newValue = true;
            //var socialNetwName = socialNetworks[position].socialNetworkName;
            //socialNetworks.RemoveAt(position);
            //socialNetworks.Insert(position, new SocialNetworkModel { socialNetworkName = socialNetwName, chosen = newValue });
            //NotifyDataSetChanged();
        }
    }
}
