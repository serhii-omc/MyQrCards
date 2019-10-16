using System;
using System.Collections.Generic;
using System.Globalization;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using CardsAndroid.Activities;
using CardsAndroid.Models;
using CardsAndroid.NativeClasses;
using CardsAndroid.ViewHolders;
using CardsPCL.CommonMethods;
using CardsPCL.Localization;

namespace CardsAndroid.Adapters
{
    public class QrAdapter : RecyclerView.Adapter
    {
        Activity _context;
        public static List<QrListModel> QrsList;
        public static bool Offline;
        private List<QrListModel> _qrsList;
        CultureInfo _ci;
        //PersonalDataActivity personalDataActivity = new PersonalDataActivity();
        //NativeMethods nativeMethods = new NativeMethods();
        //PictureMethods pictureMethods = new PictureMethods();
        Methods _methods = new Methods();
        QrViewHolder _qrViewHolder;
        Typeface _tf;
        public QrAdapter(Activity context, Typeface tf, CultureInfo ci)
        {
            this._ci = ci;
            //QRAdapter.qrsList = qrsList;
            _context = context;
            this._tf = tf;
        }
        void OnDemoClick(int position)
        {
            QrActivity.ClickedPosition = position;
            QrActivity.OpenDemoView();
        }

        void OnShareClick(int position)
        {
            QrActivity.CurrentPosition = position;

            // TODO
            //var uri = Android.Net.Uri.Parse(QrsList[position].Url);
            var uri = Android.Net.Uri.Parse(_qrsList[position]?.Url);
            var name = _qrsList[position]?.Person?.firstName;
            var lastname = _qrsList[position]?.Person?.lastName;
            string message = $"{TranslationHelper.GetString("shareWithYouABusinessCard", _ci)} {name} {lastname}\n\n{uri}\n\n{TranslationHelper.GetString("createBusinessCardToYourself", _ci)}{"https://myqrcards.page.link/nM9x"}"; 

             Intent sendIntent = new Intent();
            sendIntent.SetAction(Intent.ActionSend);
            sendIntent.SetType("text/plain");
            sendIntent.PutExtra(Intent.ExtraText, message);
            _context.StartActivity(Intent.CreateChooser(sendIntent, TranslationHelper.GetString("share", _ci)));
        }

        void OnOptionClick(int position)
        {
            QrActivity.CurrentPosition = position;
            // TODO
            //QrActivity.TintClickedCardId = 0;//QrsList[position].Id;
            //EditActivity.CardId = 0;//QrsList[position].Id;
            QrActivity.TintClickedCardUrl = _qrsList[position].Url;
            QrActivity.TintClickedCardId = _qrsList[position].Id;
            EditActivity.CardId = _qrsList[position].Id;

            QrActivity.ShowTint();
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            _qrsList = QrsList ?? _qrsList;
            _qrViewHolder = (QrViewHolder)holder;
            _qrViewHolder.CompanyLogoIv.Background = null;
            if (QrsList[position] != null)
            {
                _qrViewHolder.QrIv.SetImageBitmap(QrsList[position].QrImage);
                if (QrsList[position].LogoImage != null)
                    _qrViewHolder.CompanyLogoIv.SetImageBitmap(QrsList[position].LogoImage);
                else
                    _qrViewHolder.CompanyLogoIv.SetBackgroundResource(Resource.Drawable.company_qr_template);
            }
            //var rowWidth = qrViewHolder.ItemView.Width;
            //if (offline)
            //{
            //    qrViewHolder.show_in_webBn.Visibility = ViewStates.Gone;
            //    qrViewHolder.optionBn.Visibility = ViewStates.Gone;
            //}
            //else
            {
                _qrViewHolder.ShareBn.Visibility = ViewStates.Visible;
                _qrViewHolder.OptionBn.Visibility = ViewStates.Visible;
            }
            _qrViewHolder.ShareBn.SetTypeface(_tf, TypefaceStyle.Normal);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View layout;
            if (QrsList?.Count > 1)
                layout = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.QRRow, parent, false);
            else
                layout = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.QRSingleRow, parent, false);
            return new QrViewHolder(layout, OnDemoClick, OnShareClick, OnOptionClick, _context);
        }

        public override int ItemCount
        {
            get { try { return QrsList.Count; } catch { return 0; } }
        }
    }
}
