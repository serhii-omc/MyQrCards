using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CardsPCL.Database;
using Plugin.InAppBilling;
using Plugin.InAppBilling.Abstractions;

namespace CardsAndroid.NativeClasses
{
    //public interface IInAppBillingService
    //{
    //    string LastExceptionMessage { get; }

    //    Task<bool> PurchaseItemAsync();
    //    Task<bool> WasItemPurchasedAsync();
    //}
    public class InAppBillingService// : IInAppBillingService
    {
        CardsPCL.CommonMethods.Accounts _accounts = new CardsPCL.CommonMethods.Accounts();
        DatabaseMethods _databaseMethods = new DatabaseMethods();
        public string LastExceptionMessage { get; private set; }

        const string kPayload = "ANY_STRING";

        public async Task<bool> WasItemPurchasedAsync(string id)
        {
            var billing = CrossInAppBilling.Current;
            LastExceptionMessage = null;

            try
            {
                var connected = await billing.ConnectAsync();

                if (connected == false)
                    return false;

                var purchases = await billing.GetPurchasesAsync(ItemType.Subscription);

                if (purchases?.Any(p => p.ProductId == id/*_kProductId*/) ?? false)
                    return true;
                else
                    return false;
            }
            catch (InAppBillingPurchaseException ex)
            {
                OnPurchaseException(ex);
            }
            catch (Exception ex)
            {
                //Dlog.Error("Issue connecting: " + ex);
                LastExceptionMessage = ex.Message;
            }
            finally
            {
                await billing.DisconnectAsync();
            }

            return false;
        }
        public async Task<InAppBillingProduct> GetProductDetails(string productId)
        {
            var billing = CrossInAppBilling.Current;
            try
            {

                var productIds = new string[] { productId };
                //You must connect
                var connected = await billing.ConnectAsync(ItemType.Subscription);

                if (!connected)
                {
                    //Couldn't connect
                    return null;
                }

                //check purchases

                var items = await billing.GetProductInfoAsync(ItemType.Subscription, productIds);

                foreach (var item in items)
                {
                    return item;
                }
                return null;
            }
            catch (InAppBillingPurchaseException pEx)
            {
                //Handle IAP Billing Exception
            }
            catch (Exception ex)
            {
                //Something has gone wrong
            }
            finally
            {
                await billing.DisconnectAsync();
            }
            return null;
        }
        public async Task<InAppBillingPurchase> PurchaseSubscription(string productId, string payload)
        {
            var billing = CrossInAppBilling.Current;
            InAppBillingPurchase purchase = new InAppBillingPurchase();
            try
            {
                var connected = await billing.ConnectAsync(ItemType.Subscription);
                if (!connected)
                {
                    //we are offline or can't connect, don't try to purchase
                    return null;
                }

                //check purchases
                purchase = await billing.PurchaseAsync(productId, ItemType.Subscription, payload);
                //possibility that a null came through.
                if (purchase == null)
                {
                    //did not purchase
                }
                else
                {
                    //purchased!

                    return purchase;
                }
            }
            catch (InAppBillingPurchaseException purchaseEx)
            {
                //Billing Exception handle this based on the type
                Debug.WriteLine("Error: " + purchaseEx);
                return null;
            }
            catch (Exception ex)
            {
                //Something else has gone wrong, log it
                Debug.WriteLine("Issue connecting: " + ex);
                return null;
            }
            finally
            {
                await billing.DisconnectAsync();
            }
            return purchase;
        }

        void OnPurchaseException(InAppBillingPurchaseException ex)
        {
            switch (ex.PurchaseError)
            {
                case PurchaseError.AppStoreUnavailable:
                    LastExceptionMessage = "Currently the store seems to be unavailble, please try again later.";
                    break;
                case PurchaseError.BillingUnavailable:
                    LastExceptionMessage = "Billing seems to be unavailable, please try again later.";
                    break;
                case PurchaseError.InvalidProduct:
                case PurchaseError.ItemUnavailable:
                    LastExceptionMessage = "Product is unavailable.";
                    break;
                case PurchaseError.PaymentInvalid:
                    LastExceptionMessage = "Payment seems to be invalid, please try again.";
                    break;
                case PurchaseError.PaymentNotAllowed:
                    LastExceptionMessage = "Payment does not seem to be enabled/allowed, please try again.";
                    break;
                case PurchaseError.ProductRequestFailed:
                    LastExceptionMessage = "Product request failed, please try again.";
                    break;
                case PurchaseError.RestoreFailed:
                    LastExceptionMessage = "Product restore failed, please try again.";
                    break;
                case PurchaseError.ServiceUnavailable:
                    LastExceptionMessage = "Network is unavailable, please try again later.";
                    break;
                case PurchaseError.UserCancelled:
                    LastExceptionMessage = "Purchase was cancelled.";
                    break;
                //case PurchaseError.DeveloperError:
                //case PurchaseError.GeneralError:
                //case PurchaseError.AlreadyOwned:
                //case PurchaseError.NotOwned:
                default:
                    LastExceptionMessage = "Unknown error, sorry.";
                    break;
            }
        }
    }
}
