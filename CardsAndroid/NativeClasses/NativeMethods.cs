using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Widget;
using CardsAndroid.Activities;
using CardsAndroid.Adapters;
using CardsAndroid.Models;
using CardsPCL;
using CardsPCL.Database;
using CardsPCL.Localization;
using Com.Yalantis.Ucrop;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

namespace CardsAndroid.NativeClasses
{
    public class NativeMethods : AppCompatActivity
    {
        Activity context;
        CultureInfo ci = GetCurrentCulture.GetCurrentCultureInfo();
        DatabaseMethods databaseMethods = new DatabaseMethods();

        public static string GetDeviceId()
        {
            return Settings.Secure.GetString(Application.Context.ContentResolver, Settings.Secure.AndroidId);
        }
        public static Bitmap GetBitmapFromUrl(string url)
        {
            using (var webClient = new WebClient())
            {
                try
                {
                    var imageBytes = webClient.DownloadData(url);
                    if (imageBytes != null && imageBytes.Length > 0)
                        return BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                }
                catch
                {
                }
            }
            return null;
        }

        public async Task<Android.Net.Uri> ExportBitmapAsJpegAndGetUri(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));
            var sdCardPath = $"{Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + Constants.myQrCardsPath}";
            try
            {
                if (!Directory.Exists(sdCardPath))
                    Directory.CreateDirectory(sdCardPath).Refresh();
                var filePath = System.IO.Path.Combine(sdCardPath, Constants.destinationFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    var res = await bitmap.CompressAsync(Bitmap.CompressFormat.Png, 80, stream);
                    stream.Close();
                    return Android.Net.Uri.FromFile(new Java.IO.File(filePath));
                }
            }
            catch (IOException ex)
            {
                // This is used for cache images in violation on path situations.
                return await ExportBitmapAsJpegAndGetUri(bitmap);
            }
        }

        public async Task<bool> CacheQrOffline(Bitmap qrBitmap, Bitmap logoBitmap, int indexName)
        {
            if (qrBitmap == null)
                throw new ArgumentNullException(nameof(qrBitmap));
            var sdCardPath = $"{Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + Constants.myQrCardsOfflineCache}";
            try
            {
                if (!Directory.Exists(sdCardPath))
                    Directory.CreateDirectory(sdCardPath).Refresh();
                var filePath = System.IO.Path.Combine(sdCardPath, indexName + ".jpeg");
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    var res = await qrBitmap.CompressAsync(Bitmap.CompressFormat.Png, 80, stream);
                    stream.Close();
                    await CacheQROfflineLogo(logoBitmap, indexName);
                    return true;
                }
            }
            catch (IOException ex)
            {
                //return false;
                // This is used for cache images in violation on path situations.
                return await CacheQrOffline(qrBitmap, logoBitmap, indexName);
            }
        }

        async Task<bool> CacheQROfflineLogo(Bitmap logoBitmap, int indexName)
        {
            if (logoBitmap == null)
                throw new ArgumentNullException(nameof(logoBitmap));
            var sdCardPath = $"{Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + Constants.myQrCardsOfflineLogoCache}";
            try
            {
                if (!Directory.Exists(sdCardPath))
                    Directory.CreateDirectory(sdCardPath).Refresh();
                var filePath = System.IO.Path.Combine(sdCardPath, indexName + ".jpeg");
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    var res = await logoBitmap.CompressAsync(Bitmap.CompressFormat.Png, 80, stream);
                    stream.Close();
                    return true;
                }
            }
            catch (IOException ex)
            {
                //return false;
                // This is used for cache images in violation on path situations.
                return await CacheQROfflineLogo(logoBitmap, indexName);
            }

            return true;
        }

        public async Task<bool> RemoveOfflineCache()
        {
            var sdQrPath = $"{Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + Constants.myQrCardsOfflineCache}";
            try
            {
                // Delete all files before clear directory.
                var list = Directory.GetFiles(sdQrPath, "*");
                if (list.Length > 0)
                {
                    for (int i = 0; i < list.Length; i++)
                    {
                        System.IO.File.Delete(list[i]);
                    }
                }
                //Directory.Delete(sdCardPath);
                //return true;
            }
            catch (Exception ex)
            {
                //return false;
            }
            var sdLogoPath = $"{Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + Constants.myQrCardsOfflineLogoCache}";
            try
            {
                // Delete all files before clear directory.
                var list = Directory.GetFiles(sdLogoPath, "*");
                if (list.Length > 0)
                {
                    for (int i = 0; i < list.Length; i++)
                    {
                        System.IO.File.Delete(list[i]);
                    }
                }
                //Directory.Delete(sdCardPath);
                //return true;
            }
            catch (Exception ex)
            {
                //return false;
            }
            return true;
        }

        public async Task<List<QrListModel>> GetCachedQrsAndLogos()
        {
            var retievedList = new List<QrListModel>();
            var sdQrPath = $"{Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + Constants.myQrCardsOfflineCache}";
            var sdLogoPath = $"{Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + Constants.myQrCardsOfflineLogoCache}";
            if (!Directory.Exists(sdQrPath) || !Directory.Exists(sdLogoPath))
                return retievedList;
            var listQRs = Directory.GetFiles(sdQrPath, "*");
            var listLogos = Directory.GetFiles(sdLogoPath, "*");
            try
            {
                //var gfjhg = listQRs.Where(x => x == (sdQrPath + 0 + ".jpeg"));
                //var kfgjfgk = listQRs.Single(item => item.Contains(0 + ".jpeg"));
                var listCardObj = databaseMethods.GetCardNamesWithUrlAndIdAndPersonData();
                Bitmap tempQrBitmap, tempLogoBitmap;
                if (listQRs.Length > 0)
                {
                    for (int i = 0; i < listQRs.Length; i++)
                    {
                        tempQrBitmap = BitmapFactory.DecodeFile(sdQrPath + "/" + i + ".jpeg");
                        tempLogoBitmap = BitmapFactory.DecodeFile(sdLogoPath + "/" + i + ".jpeg");
                        retievedList.Add(new QrListModel
                        {
                            QrImage = tempQrBitmap,
                            LogoImage = tempLogoBitmap,
                            Id = listCardObj[i].card_id,
                            Url = listCardObj[i].card_url,
                            Person = new CardsPCL.Models.Person
                            {
                                firstName = listCardObj[i].PersonName,
                                lastName = listCardObj[i].PersonSurname
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return retievedList;
            }
            return retievedList;
        }

        public async Task<bool> CachePersonalImage(Bitmap bitmap, int indexName)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));
            var sdCardPath = $"{Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + Constants.myQrCardsPathPersonalCache}";
            try
            {
                bitmap = FixedResolutionBitmap(bitmap);
                if (!Directory.Exists(sdCardPath))
                    Directory.CreateDirectory(sdCardPath).Refresh();
                var filePath = System.IO.Path.Combine(sdCardPath, "personal" + indexName + ".jpeg");
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    var res = await bitmap.CompressAsync(Bitmap.CompressFormat.Png, 80, stream);
                    stream.Close();
                    return true;
                }
            }
            catch (IOException ex)
            {
                //return false;
                // This is used for cache images in violation on path situations.
                return await CachePersonalImage(bitmap, indexName);
            }
        }

        private Bitmap FixedResolutionBitmap(Bitmap bitmap)
        {
            double maxWidth = Constants.BitmapSide;
            double maxHeight = Constants.BitmapSide;
            double widthOriginal = bitmap.Width;
            double heightOriginal = bitmap.Height;

            SqueezeImage(ref maxWidth, ref maxHeight, ref widthOriginal, ref heightOriginal);
            var resizedBitmap = Bitmap.CreateScaledBitmap(bitmap, (int)maxWidth, (int)maxHeight, true);
            return resizedBitmap;
        }

        public async Task<bool> RemovePersonalImages()
        {
            var sdCardPath = $"{Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + Constants.myQrCardsPathPersonalCache}";
            try
            {
                // Delete all files before clear directory.
                var list = Directory.GetFiles(sdCardPath, "*");
                if (list.Length > 0)
                {
                    for (int i = 0; i < list.Length; i++)
                    {
                        System.IO.File.Delete(list[i]);
                    }
                }
                //Directory.Delete(sdCardPath);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> CacheLogo(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));
            var sdCardPath = $"{Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + Constants.myQrCardsPathCompanyCache}";
            //System.IO.d
            //File.Create().Dispose();
            try
            {
                bitmap = FixedResolutionBitmap(bitmap);
                if (!Directory.Exists(sdCardPath))
                    Directory.CreateDirectory(sdCardPath).Refresh();
                var filePath = System.IO.Path.Combine(sdCardPath, "logo.jpeg");
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    var res = await bitmap.CompressAsync(Bitmap.CompressFormat.Png, 80, stream);
                    stream.Close();
                    return true;
                }
            }
            catch (IOException ex)
            {
                // This is used for cache images in violation on path situations.
                return await CacheLogo(bitmap);
            }
            return false;
        }

        internal int GetCountCachedQrs()
        {
            var sdCardPath = $"{Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + Constants.myQrCardsOfflineCache}";
            try
            {
                var list = Directory.GetFiles(sdCardPath, "*");
                return list.Length;
            }
            catch (Exception e)
            { }
            return 0;
        }

        internal int GetCountCachedLogos()
        {
            var sdCardPath = $"{Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + Constants.myQrCardsOfflineLogoCache}";
            try
            {
                var list = Directory.GetFiles(sdCardPath, "*");
                return list.Length;
            }
            catch (Exception e)
            { }
            return 0;
        }

        public async Task<bool> RemoveLogo()
        {
            var sdCardPath = $"{Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + Constants.myQrCardsPathCompanyCache}";
            try
            {
                // Delete all files before clear directory.
                var list = Directory.GetFiles(sdCardPath, "*");
                if (list.Length > 0)
                {
                    for (int i = 0; i < list.Length; i++)
                    {
                        System.IO.File.Delete(list[i]);
                    }
                }
                //try
                //{
                //    //Directory.Delete(sdCardPath);
                //}
                //catch (Exception ex)
                //{

                //}
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<List<byte[]>> GetPersonalImages()
        {
            var sdCardPath = $"{Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + Constants.myQrCardsPathPersonalCache}";
            try
            {
                var byteArrayList = new List<byte[]>();
                var list = Directory.GetFiles(sdCardPath, "*");
                if (list.Length > 0)
                    for (int i = 0; i < list.Length; i++)
                        byteArrayList.Add(File.ReadAllBytes(list[i]));
                else
                    return null;
                return byteArrayList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<byte[]> GetDocumentsLogo()
        {
            var sdCardPath = $"{Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + Constants.myQrCardsPathCompanyCache}";
            try
            {
                // Delete all files before clear directory.
                var list = Directory.GetFiles(sdCardPath, "*");
                if (list.Length > 0)
                {
                    for (int i = 0; i < list.Length; i++)
                    {
                        return File.ReadAllBytes(list[i]);
                    }
                    return null;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        //public Uri GetUriFromBitmap(Activity context_, Bitmap inImage)
        //{

        //    try
        //    {

        //        using (System.IO.Stream bytes = null)
        //        {
        //            inImage.Compress(Bitmap.CompressFormat.Jpeg, 100, bytes);
        //            String path = MediaStore.Images.Media.InsertImage(context_.ContentResolver, inImage, "Title", null);
        //            return new UriBuilder(path).Uri;
        //        }
        //    }
        //    catch { }
        //    return null;
        //    //ByteArrayOutputStream bytes = new ByteArrayOutputStream();
        //    //inImage.Compress(Bitmap.CompressFormat.Jpeg, 100, bytes);
        //    //String path = Images.Media.insertImage(inContext.getContentResolver(), inImage, "Title", null);
        //    //return Uri.parse(path);
        //}


        public bool AreStorageAndCamPermissionsGranted(Activity context)
        {
            if (Build.VERSION.SdkInt >= Build.VERSION_CODES.M)
                if (
                             context.CheckSelfPermission(Manifest.Permission.Camera) != Android.Content.PM.Permission.Granted
                          || context.CheckSelfPermission(Manifest.Permission.ReadExternalStorage) != Android.Content.PM.Permission.Granted
                          || context.CheckSelfPermission(Manifest.Permission.WriteExternalStorage) != Android.Content.PM.Permission.Granted)
                {
                    ActivityCompat.RequestPermissions(context, new String[]
                   {
                                Manifest.Permission.Camera,
                                Manifest.Permission.ReadExternalStorage,
                                Manifest.Permission.WriteExternalStorage,
                   }, REQUEST_PERMISSION_CODE);
                    return false;
                }
            return true;
        }
        public async Task<bool> CheckStoragePermissions(Activity context)
        {
            this.context = context;
            PermissionStatus permissionStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);

            if (permissionStatus != PermissionStatus.Granted)
            {
                var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Location });
                permissionStatus = results[Permission.Location];
                //RequestRuntimePermissions();
                RequestStoragePermissions();
                return false;
            }
            else
            {
                //Toast.MakeText(this, TranslationHelper.GetString("permissionsNeeded", ci), ToastLength.Long).Show();
                return true;
            }
        }

        private const int REQUEST_PERMISSION_CODE = 1000;
        public void RequestCameraAndStoragePermissions()
        {
            if (Build.VERSION.SdkInt >= Build.VERSION_CODES.M)
                if (
                             context.CheckSelfPermission(Manifest.Permission.Camera) != Android.Content.PM.Permission.Granted
                          || context.CheckSelfPermission(Manifest.Permission.ReadExternalStorage) != Android.Content.PM.Permission.Granted
                          || context.CheckSelfPermission(Manifest.Permission.WriteExternalStorage) != Android.Content.PM.Permission.Granted)
                {
                    ActivityCompat.RequestPermissions(context, new String[]
                    {
                                Manifest.Permission.Camera,
                                Manifest.Permission.ReadExternalStorage,
                                Manifest.Permission.WriteExternalStorage,
                    }, REQUEST_PERMISSION_CODE);
                }
                else
                {
                    ActivityCompat.RequestPermissions(context, new String[]
                    {
                                Manifest.Permission.Camera,
                                Manifest.Permission.ReadExternalStorage,
                                Manifest.Permission.WriteExternalStorage,
                    }, REQUEST_PERMISSION_CODE);
                }
        }

        public void RequestStoragePermissions()
        {
            if (Build.VERSION.SdkInt >= Build.VERSION_CODES.M)
                if (context.CheckSelfPermission(Manifest.Permission.ReadExternalStorage) != Android.Content.PM.Permission.Granted
                          || context.CheckSelfPermission(Manifest.Permission.WriteExternalStorage) != Android.Content.PM.Permission.Granted)
                {
                    ActivityCompat.RequestPermissions(context, new String[]
                    {
                                Manifest.Permission.ReadExternalStorage,
                                Manifest.Permission.WriteExternalStorage,
                    }, REQUEST_PERMISSION_CODE);
                }
                else
                {
                    ActivityCompat.RequestPermissions(context, new String[]
                    {
                                Manifest.Permission.ReadExternalStorage,
                                Manifest.Permission.WriteExternalStorage,
                    }, REQUEST_PERMISSION_CODE);
                }
        }
        //bool shown = false;
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            //base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            switch (requestCode)
            {
                case REQUEST_PERMISSION_CODE:
                    {
                        if (grantResults.Length > 0 && grantResults[0] == Android.Content.PM.Permission.Granted)
                        {
                            //StartActivity(typeof(MainActivity));
                        }
                        else
                        {
                            //if (!shown)
                            //    Toast.MakeText(this, TranslationHelper.GetString("permissionsNeeded", ci), ToastLength.Long).Show();
                            //shown = true;
                            //request_runtime_permissions();
                        }
                        break;
                    }
            }
        }
        public static void ResetSocialNetworkList()
        {
            SocialNetworkAdapter.SocialNetworks = null;
            SocialNetworkAdapter.SocialNetworks = new List<SocialNetworkModel> { new SocialNetworkModel { Id = 1, SocialNetworkName = Constants.facebook, UsersUrl = null },
                                                                                                new SocialNetworkModel { Id = 4, SocialNetworkName = Constants.instagram, UsersUrl = null },
                                                                                                new SocialNetworkModel { Id= 3, SocialNetworkName = Constants.linkedin, UsersUrl = null },
                                                                                                new SocialNetworkModel { Id = 5, SocialNetworkName = Constants.twitter, UsersUrl = null },
                                                                                                new SocialNetworkModel { Id = 2, SocialNetworkName = Constants.vkontakte, UsersUrl = null } };
        }


        public static void ResetQrsList()
        {
            QrAdapter.QrsList = null;
            QrAdapter.QrsList = new List<QrListModel>();
        }

        public static void ResetHomeAddress()
        {
            HomeAddressActivity.FullAddressStatic = null;
            HomeAddressActivity.MyCountry = null;
            HomeAddressActivity.MyRegion = null;
            HomeAddressActivity.MyCity = null;
            HomeAddressActivity.MyIndex = null;
            HomeAddressActivity.MyNotation = null;

            HomeAddressActivity.FullAddressTemp = null;
            HomeAddressActivity.MyCountryTemp = null;
            HomeAddressActivity.MyRegionTemp = null;
            HomeAddressActivity.MyCityTemp = null;
            HomeAddressActivity.MyIndexTemp = null;
            HomeAddressActivity.MyNotationTemp = null;

            NewCardAddressMapActivity.Lat = null;
            NewCardAddressMapActivity.Lng = null;
        }

        public void ClearAll()
        {
            #region clearing tables, variables and photos
            var TaskA = new Task(async () =>
            {
                await RemovePersonalImages();
                await RemoveLogo();
                await RemoveOfflineCache();
                //var QRs_cache_dir = Path.Combine(docs, Constants.QRs_cache_dir);
            });
            TaskA.Start();
            databaseMethods.CleanPersonalNetworksTable();
            databaseMethods.ClearCompanyCardTable();
            databaseMethods.ClearUsersCardTable();
            //CropCompanyLogoViewController.currentImage = null;
            CompanyDataActivity.CroppedResult = null;
            //CompanyAddressMapViewController.lat = null;
            //CompanyAddressMapViewController.lng = null;
            CompanyAddressMapActivity.CompanyLat = null;
            CompanyAddressMapActivity.CompanyLng = null;
            CompanyAddressActivity.FullCompanyAddressStatic = null;
            CompanyAddressActivity.Country = null;
            CompanyAddressActivity.Region = null;
            CompanyAddressActivity.City = null;
            CompanyAddressActivity.Index = null;
            CompanyAddressActivity.Notation = null;
            CompanyDataActivity.CompanyName = null;
            CompanyDataActivity.LinesOfBusiness = null;
            CompanyDataActivity.Position = null;
            CompanyDataActivity.FoundationYear = null;
            CompanyDataActivity.Clients = null;
            CompanyDataActivity.CompanyPhone = null;
            CompanyDataActivity.CorporativePhone = null;
            CompanyDataActivity.Fax = null;
            CompanyDataActivity.CompanyEmail = null;
            CompanyDataActivity.CorporativeSite = null;
            PersonalDataActivity.MySurname = null;
            PersonalDataActivity.MyName = null;
            PersonalDataActivity.MyMiddlename = null;
            PersonalDataActivity.MyPhone = null;
            PersonalDataActivity.MyEmail = null;
            PersonalDataActivity.MyHomePhone = null;
            PersonalDataActivity.MySite = null;
            PersonalDataActivity.MyDegree = null;
            PersonalDataActivity.MyCardName = null;
            try { PersonalImageAdapter.Photos?.Clear(); } catch { }
            PersonalDataActivity.MyBirthdate = null;
            NativeMethods.ResetHomeAddress();
            NativeMethods.ResetCompanyAddress();
            EditCompanyDataActivity.Position = null;
            EditCompanyDataActivity.LogoId = null;
            #endregion clearing tables, variables and photos
        }

        public static void ResetCompanyAddress()
        {
            CompanyAddressActivity.FullCompanyAddressStatic = null;
            CompanyAddressActivity.Country = null;
            CompanyAddressActivity.Region = null;
            CompanyAddressActivity.City = null;
            CompanyAddressActivity.Index = null;
            CompanyAddressActivity.Notation = null;

            CompanyAddressActivity.FullCompanyAddressTemp = null;
            CompanyAddressActivity.CountryTemp = null;
            CompanyAddressActivity.RegionTemp = null;
            CompanyAddressActivity.CityTemp = null;
            CompanyAddressActivity.IndexTemp = null;
            CompanyAddressActivity.NotationTemp = null;

            CompanyAddressMapActivity.CompanyLat = null;
            CompanyAddressMapActivity.CompanyLng = null;
        }

        public Android.App.AlertDialog CallPremiumOptionMenu(Activity context)
        {
            Android.App.AlertDialog.Builder builder = new Android.App.AlertDialog.Builder(context);
            //builder.SetTitle(TranslationHelper.GetString("fillingInThisFieldPremiumOnly", ci));
            if (!QrActivity.IsPremium)
            {
                builder.SetMessage(TranslationHelper.GetString("fillingInThisFieldPremiumOnly", ci));
                builder.SetNeutralButton(TranslationHelper.GetString("premium", ci), (object sender1, DialogClickEventArgs e1) =>
                {
                    context.StartActivity(typeof(PremiumActivity));
                });
            }
            builder.SetNegativeButton("OK", (object sender1, DialogClickEventArgs e1) => { });
            builder.SetCancelable(false);
            if (!databaseMethods.UserExists())
                builder.SetPositiveButton(TranslationHelper.GetString("login", ci), (object sender1, DialogClickEventArgs e1) =>
                {
                    context.StartActivity(typeof(EmailActivity));
                });
            Android.App.AlertDialog dialog = builder.Create();
            return dialog;
        }

        public void DisableEditText(Activity context, EditText editText)
        {
            editText.Enabled = false;
            editText.SetTextColor(context.Resources.GetColor(Resource.Color.editTextLineColorDisabled));
        }

        public UCrop.Options UCropOptions()
        {
            UCrop.Options options = new UCrop.Options();
            options.SetHideBottomControls(true);
            return options;
        }

        public void SqueezeImage(ref double maxWidth,
                                 ref double maxHeight,
                                 ref double widthOriginal,
                                 ref double heightOriginal)
        {
            if (widthOriginal < heightOriginal)
            {
                var aspectRatio = widthOriginal  / heightOriginal;
                if (aspectRatio == 0)
                    return;
                maxWidth = (int)(maxHeight * aspectRatio);
            }
            else
            {
                var aspectRatio = heightOriginal  / widthOriginal;
                if (aspectRatio == 0)
                    return;
                maxHeight = (int)(maxWidth * aspectRatio);
            }
        }
    }
}