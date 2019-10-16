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
    public class EditPersonalImageViewHolder : RecyclerView.ViewHolder
    {
        public ImageView ImageIv { get; set; }
        public ProgressBar ActivityIndicator { get; set; }
        NativeMethods _nativeMethods = new NativeMethods();
        Activity _context;
        public EditPersonalImageViewHolder(View itemView, Action<int, View> listener, /*Action<int, View> longListener,*/ Activity context) : base(itemView)
        {
            _context = context;
            ImageIv = itemView.FindViewById<ImageView>(Resource.Id.imageIV);
            ActivityIndicator = itemView.FindViewById<ProgressBar>(Resource.Id.activityIndicator);
            ActivityIndicator.IndeterminateDrawable.SetColorFilter(Color.Rgb(255, 99, 62), PorterDuff.Mode.Multiply);
            itemView.Click += (s, e) => listener(Position, itemView);
            //itemView.LongClick += (s, e) => longListener(Position, itemView);
        }
    }
}

