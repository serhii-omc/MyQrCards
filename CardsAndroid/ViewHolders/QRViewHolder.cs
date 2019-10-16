using System;
using Android.App;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using CardsAndroid.Activities;
using CardsAndroid.Adapters;
using CardsAndroid.NativeClasses;

namespace CardsAndroid.ViewHolders
{
    public class QrViewHolder : RecyclerView.ViewHolder
    {
        //public ImageView imageIV { get; set; }
        //public ProgressBar activityIndicator { get; set; }
        NativeMethods _nativeMethods = new NativeMethods();
        PersonalDataActivity _personalDataActivity = new PersonalDataActivity();
        Activity _context;
        public Button ShareBn, OptionBn;
        public ImageView QrIv, CompanyLogoIv, OrangeBorderIv;
        public QrViewHolder(View itemView, Action<int> demoListener, Action<int> shareListener, Action<int> optionListener, Activity context) : base(itemView)
        {
            _context = context;
            ShareBn = itemView.FindViewById<Button>(Resource.Id.shareBn);
            OptionBn = itemView.FindViewById<Button>(Resource.Id.optionBn);
            QrIv = itemView.FindViewById<ImageView>(Resource.Id.qrIV);
            CompanyLogoIv = itemView.FindViewById<ImageView>(Resource.Id.companyLogoIV);
            OrangeBorderIv = itemView.FindViewById<ImageView>(Resource.Id.orangeBorderIV);
            //imageIV = itemView.FindViewById<ImageView>(Resource.Id.imageIV);
            //activityIndicator = itemView.FindViewById<ProgressBar>(Resource.Id.activityIndicator);
            //activityIndicator.IndeterminateDrawable.SetColorFilter(Color.Rgb(255, 99, 62), PorterDuff.Mode.Multiply);
            //itemView.Click += async (s, e) =>
            //{
            //    //if (Position > 0)
            //    //{
            //    //    activityIndicator.Visibility = ViewStates.Visible;
            //    //    imageIV.Visibility = ViewStates.Gone;

            //    //    var uri = await nativeMethods.ExportBitmapAsJpegAndGetUri(PersonalImageAdapter._photos[Position]);
            //    //    activityIndicator.Visibility = ViewStates.Gone;
            //    //    imageIV.Visibility = ViewStates.Visible;
            //    //    personalDataActivity.StartCropActivity(uri, _context, Position);
            //    //}
            //};
            OrangeBorderIv.Click += (s, e) => demoListener(Position);
            ShareBn.Click += (s, e) => shareListener(Position);
            OptionBn.Click += (s, e) => optionListener(Position);
            //itemView.LongClick += (s, e) => longListener(Position, itemView);
        }
    }
}
