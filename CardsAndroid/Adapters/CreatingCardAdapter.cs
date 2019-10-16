using System;
using System.Collections.Generic;
using Android.App;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using CardsAndroid.Activities;
using CardsAndroid.Models;
using CardsAndroid.ViewHolders;

namespace CardsAndroid.Adapters
{
    public class CreatingCardAdapter : RecyclerView.Adapter
    {
        Activity _context;
        List<CreatingCardModel> _cardNames;
        CreatingCardViewHolder _creatingCardViewHolder;
        Typeface _tf;
        public CreatingCardAdapter(Activity context, List<CreatingCardModel> cardNames, Typeface tf)
        {
            this._cardNames = cardNames;
            this._context = context;
            this._tf = tf;
        }
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            _creatingCardViewHolder = (CreatingCardViewHolder)holder;
            _creatingCardViewHolder.CardNameTv.Text = _cardNames[position].CardName;
            _creatingCardViewHolder.CardNameTv.SetTypeface(_tf, TypefaceStyle.Normal);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var layout = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.creating_card_row, parent, false);
            return new CreatingCardViewHolder(layout, OnItemClick);
        }

        void OnItemClick(int position)
        {
            CreatingCardActivity.show_loader(_cardNames[position].Id);
        }

        public override int ItemCount
        {
            get { try { return _cardNames.Count; } catch { return 0; } }
        }
    }
}
