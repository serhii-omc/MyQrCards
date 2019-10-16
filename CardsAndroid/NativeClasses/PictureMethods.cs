using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using CardsPCL;
using System;
using System.Collections.Generic;

namespace CardsAndroid.NativeClasses
{
    public class PictureMethods
    {
        public static string CameraOrGalleryIndicator;
        public void CreateDirectoryForPictures()
        {
            App.Dir = new Java.IO.File(
                Android.OS.Environment.GetExternalStoragePublicDirectory(
                    Android.OS.Environment.DirectoryPictures), Constants.myqrcards);
            if (!App.Dir.Exists())
            {
                App.Dir.Mkdirs();
            }
        }

        public bool IsThereAnAppToTakePictures(Activity context)
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            IList<ResolveInfo> availableActivities =
                context.PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            return availableActivities != null && availableActivities.Count > 0;
        }

        public void TakeAPicture(Activity context)
        {
            //cameraOrGalleryIndicator = "camera";
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            App.File = new Java.IO.File(App.Dir, String.Format("myPhoto_{0}.jpg", Guid.NewGuid()));
            intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(App.File));
            StrictMode.VmPolicy.Builder builder = new StrictMode.VmPolicy.Builder();
            StrictMode.SetVmPolicy(builder.Build());
            context.StartActivityForResult(intent, 0);
        }
    }
    public static class App
    {
        public static Java.IO.File File;
        public static Java.IO.File Dir;
        public static Bitmap bitmap;
    }
    public static class BitmapHelpers
    {
        public static Bitmap LoadAndResizeBitmap(this string fileName, int width, int height)
        {
            // First we get the the dimensions of the file on disk
            BitmapFactory.Options options = new BitmapFactory.Options { InJustDecodeBounds = true };
            BitmapFactory.DecodeFile(fileName, options);

            // Next we calculate the ratio that we need to resize the image by
            // in order to fit the requested dimensions.
            int outHeight = options.OutHeight;
            int outWidth = options.OutWidth;
            int inSampleSize = 1;

            if (outHeight > height || outWidth > width)
            {
                inSampleSize = outWidth > outHeight
                                   ? outHeight / height
                                   : outWidth / width;
            }

            // Now we will load the image and have BitmapFactory resize it for us.
            options.InSampleSize = inSampleSize;
            options.InJustDecodeBounds = false;
            Bitmap resizedBitmap = BitmapFactory.DecodeFile(fileName, options);

            return resizedBitmap;
        }
    }
}
