using System;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace CardsAndroid.ViewHolders
{
    public class SocialNetworkViewHolder : RecyclerView.ViewHolder
    {
        public ImageView SocialNetwIv { get; set; }
        public TextView SocialNetwNameTv { get; set; }
        public ImageView CheckIv { get; set; }

        public SocialNetworkViewHolder(View itemView, Action<int> onClick):base(itemView)
        {
            SocialNetwIv = itemView.FindViewById<ImageView>(Resource.Id.SocialNetwIV);
            SocialNetwNameTv = itemView.FindViewById<TextView>(Resource.Id.socialNetwNameTV);
            CheckIv = itemView.FindViewById<ImageView>(Resource.Id.checkIV);
            itemView.Click += (s, e) => onClick(Position);
        }
    }
}
