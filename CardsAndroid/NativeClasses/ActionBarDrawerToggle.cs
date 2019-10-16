using Android.App;
using Android.Support.V4.Widget;
using SupportActionBarDrawerToggle = Android.Support.V7.App.ActionBarDrawerToggle;

namespace CardsAndroid.NativeClasses
{
    public class ActionBarDrawerToggle : SupportActionBarDrawerToggle
    {
        private Activity _mHostActivity;
        //private int mOpenedResource;
        //private int mClosedResource;

        public ActionBarDrawerToggle(Activity host, DrawerLayout drawerLayout, int openedResource, int closedResource)
            : base(host, drawerLayout, openedResource, closedResource)
        {
            _mHostActivity = host;
            //mOpenedResource = openedResource;
            //mClosedResource = closedResource;
        }

        public override void OnDrawerOpened(Android.Views.View drawerView)
        {
            base.OnDrawerOpened(drawerView);
        }

        public override void OnDrawerClosed(Android.Views.View drawerView)
        {
            base.OnDrawerClosed(drawerView);
        }

        public override void OnDrawerSlide(Android.Views.View drawerView, float slideOffset)
        {
            base.OnDrawerSlide(drawerView, slideOffset);
        }
    }
}