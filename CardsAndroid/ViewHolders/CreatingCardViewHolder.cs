using System;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace CardsAndroid.ViewHolders
{
    public class CreatingCardViewHolder : RecyclerView.ViewHolder
    {
        public TextView CardNameTv { get; set; }
        public CreatingCardViewHolder(View itemView, Action<int>itemClick):base(itemView)
        {
            CardNameTv = itemView.FindViewById<TextView>(Resource.Id.cardNameTV);
            itemView.Click += (s, e) => itemClick(Position);
        }
    }
}
