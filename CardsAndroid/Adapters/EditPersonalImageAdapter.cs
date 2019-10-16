using System;
using System.Collections.Generic;
using System.Globalization;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using CardsAndroid.Activities;
using CardsAndroid.NativeClasses;
using CardsAndroid.ViewHolders;
using CardsPCL;
using CardsPCL.Localization;

namespace CardsAndroid.Adapters
{
    public class EditPersonalImageAdapter : RecyclerView.Adapter
    {
        Activity _context;
        public static List<Bitmap> Photos { get; set; } = new List<Bitmap>();
        CultureInfo _ci;
        NativeMethods _nativeMethods = new NativeMethods();
        PictureMethods _pictureMethods = new PictureMethods();
        EditPersonalDataActivity _editPersonalDataActivity = new EditPersonalDataActivity();
        EditPersonalImageViewHolder _editPersonalImageViewHolder;
        public EditPersonalImageAdapter(List<Bitmap> photos, Activity context, CultureInfo ci)
        {
            this._ci = ci;
            Photos = photos;
            _context = context;
        }
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            _editPersonalImageViewHolder = (EditPersonalImageViewHolder)holder;
            if (Photos[position] != null)
            {
                _editPersonalImageViewHolder.ImageIv.SetImageBitmap(Photos[position]);
            }
            if (position == 0)
            {
                _editPersonalImageViewHolder.ImageIv.SetImageBitmap(null);
                _editPersonalImageViewHolder.ImageIv.SetBackgroundResource(Resource.Drawable.add_photoBn);
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var layout = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.ImageRow, parent, false);
            return new EditPersonalImageViewHolder(layout, OnItemClick, /*OnItemLongClick,*/ _context);
        }

        public override int ItemCount
        {
            get { try { return Photos.Count; } catch { return 0; } }
        }

        //void OnItemLongClick(int position, View view)
        //{
        //    if (position > 0)
        //    {
        //        ShowRemovePopup(view, position);
        //    }
        //}
        void OnItemClick(int position, View view)
        {
            var editPersonalDataActivity = _context as EditPersonalDataActivity;
            if (position == 0)
            {
                editPersonalDataActivity.PhotosPermissionAllowed += delegate
                {
                    ShowPopup(view);
                };
                if (_nativeMethods.AreStorageAndCamPermissionsGranted(_context))
                    if (Photos.Count <= 10)
                        ShowPopup(view);
                    else
                        Toast.MakeText(_context, TranslationHelper.GetString("tenPhotosLimit", _ci), ToastLength.Short).Show();
                else
                    _nativeMethods.CheckStoragePermissions(_context);
                return;
            }

            //editPersonalDataActivity.PhotosPermissionAllowed += delegate
            //{
            //    ShowAlternativePopup(view, position);
            //};

            ShowAlternativePopup(view, position);
        }
        void ShowAlternativePopup(View view, int position)
        {
            Android.Support.V7.Widget.PopupMenu popupMenu = new Android.Support.V7.Widget.PopupMenu(_context, view);
            var menuOpts = popupMenu.Menu;
            popupMenu.Inflate(Resource.Layout.photo_option);

            popupMenu.MenuItemClick += async (s1, arg1) =>
            {
                //if (_nativeMethods.(_context))
                //{
                if (arg1.Item.TitleFormatted.ToString() == _context.GetString(Resource.String.edit))
                {
                    _editPersonalImageViewHolder.ActivityIndicator.Visibility = ViewStates.Visible;
                    _editPersonalImageViewHolder.ImageIv.Visibility = ViewStates.Gone;

                    var uri = await _nativeMethods.ExportBitmapAsJpegAndGetUri(Photos[position]);
                    _editPersonalImageViewHolder.ActivityIndicator.Visibility = ViewStates.Gone;
                    _editPersonalImageViewHolder.ImageIv.Visibility = ViewStates.Visible;
                    try
                    {
                        _editPersonalDataActivity.StartCropActivity(uri, _context, position);
                    }
                    catch (Exception ex)
                    {

                    }
                }
                if (arg1.Item.TitleFormatted.ToString() == _context.GetString(Resource.String.removePhoto))
                {
                    Photos.RemoveAt(position);
                    this.NotifyDataSetChanged();
                }
                //}
                //else
                //_nativeMethods.CheckStoragePermissions(_context);
            };
            popupMenu.Show();
        }
        void ShowPopup(View view)
        {
            Android.Support.V7.Widget.PopupMenu popupMenu = new Android.Support.V7.Widget.PopupMenu(_context, view);
            var menuOpts = popupMenu.Menu;
            popupMenu.Inflate(Resource.Layout.camOrGalleryMenu);

            popupMenu.MenuItemClick += (s1, arg1) =>
            {
                if (_nativeMethods.AreStorageAndCamPermissionsGranted(_context))
                {
                    if (arg1.Item.TitleFormatted.ToString() == _context.GetString(Resource.String.takePhoto))
                    {
                        try
                        {
                            if (_pictureMethods.IsThereAnAppToTakePictures(_context))
                            {
                                PictureMethods.CameraOrGalleryIndicator = Constants.camera;
                                _pictureMethods.CreateDirectoryForPictures();
                                _pictureMethods.TakeAPicture(_context);
                            }
                        }
                        catch
                        {
                            Toast.MakeText(_context, TranslationHelper.GetString("failedToOpenTheCamera", _ci), ToastLength.Long).Show();
                        }
                    }
                    if (arg1.Item.TitleFormatted.ToString() == _context.GetString(Resource.String.uploadFromGallery))
                    {
                        PictureMethods.CameraOrGalleryIndicator = "gallery";
                        var imageIntent = new Intent();
                        imageIntent.SetType("image/*");
                        //imageIntent.SetType("file/*");
                        imageIntent.SetAction(Intent.ActionGetContent);
                        _context.StartActivityForResult(
                            Intent.CreateChooser(imageIntent, "Select photo"), 0);
                    }
                }
                else
                    _nativeMethods.CheckStoragePermissions(_context);
            };
            popupMenu.Show();
        }
        void ShowRemovePopup(View view, int position)
        {
            Android.Support.V7.Widget.PopupMenu popupMenu = new Android.Support.V7.Widget.PopupMenu(_context, view);
            var menuOpts = popupMenu.Menu;
            popupMenu.Inflate(Resource.Layout.delete_photo_popup);

            popupMenu.MenuItemClick += (s1, arg1) =>
            {
                Photos.RemoveAt(position);
                this.NotifyDataSetChanged();
            };
            popupMenu.Show();
        }
    }
}
