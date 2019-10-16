using System;
using System.Globalization;

using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Widget;
using CardsAndroid.Adapters;
using CardsAndroid.NativeClasses;
using CardsPCL;
using CardsPCL.Database;
using CardsPCL.Localization;
using CardsPCL.Models;

namespace CardsAndroid.Activities
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class SocialNetworksActivity : Activity
    {
        SocialNetworkAdapter _socialNetworkAdapter;
        TextView _headerTv;
        RelativeLayout _backRl;
        CultureInfo _ci = GetCurrentCulture.GetCurrentCultureInfo();
        RecyclerView _socialNetworksRecyclerView;
        RecyclerView.LayoutManager _socialNetworksLayoutManager;
        DatabaseMethods _databaseMethods = new DatabaseMethods();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.SocialNetworks);
        }

        protected override void OnResume()
        {
            base.OnResume();
            InitElements();
        }

        private void InitElements()
        {
            Typeface tf = Typeface.CreateFromAsset(Assets, "FiraSansRegular.ttf");
            _socialNetworksRecyclerView = FindViewById<RecyclerView>(Resource.Id.socialNetworksRecyclerView);
            _headerTv = FindViewById<TextView>(Resource.Id.headerTV);
            _backRl = FindViewById<RelativeLayout>(Resource.Id.backRL);
            _headerTv.Text = TranslationHelper.GetString("socialNetworks", _ci);
            _headerTv.SetTypeface(tf, TypefaceStyle.Normal);

            _socialNetworksLayoutManager = new LinearLayoutManager(this, LinearLayoutManager.Vertical, false);
            _socialNetworksRecyclerView.SetLayoutManager(_socialNetworksLayoutManager);
            _socialNetworkAdapter = new SocialNetworkAdapter(this, _ci, tf);
            _socialNetworksRecyclerView.SetAdapter(_socialNetworkAdapter);
            _backRl.Click += (s, e) => OnBackPressed();
        }

        protected override void OnPause()
        {
            base.OnPause();
            _databaseMethods.CleanPersonalNetworksTable();
            foreach (var item/*index*/ in SocialNetworkAdapter.SocialNetworks)//.selectedIndexes)
            {
                int socialnetworkId = 0;
                if (item.SocialNetworkName == Constants.facebook)
                    socialnetworkId = 1;
                else if (item.SocialNetworkName == Constants.instagram)
                    socialnetworkId = 4;
                else if (item.SocialNetworkName == Constants.linkedin)
                    socialnetworkId = 3;
                else if (item.SocialNetworkName == Constants.twitter)
                    socialnetworkId = 5;
                else if (item.SocialNetworkName == Constants.vkontakte)
                    socialnetworkId = 2;
                if (!String.IsNullOrEmpty(item.UsersUrl))
                    _databaseMethods.InsertPersonalNetwork(new SocialNetworkModel { SocialNetworkID = socialnetworkId, ContactUrl = item.UsersUrl });
                //databaseMethods.InsertPersonalNetwork(new SocialNetworkModel { SocialNetworkID = datalist[index].Id, ContactUrl = datalist[index].ContactUrl });
            }
        }
    }
}
