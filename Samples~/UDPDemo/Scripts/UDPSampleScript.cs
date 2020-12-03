using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UDP;

/// <summary>
/// Scripts for the SampleScene.
/// </summary>
public class UDPSampleScript : MonoBehaviour
{
    // Two products shown in the dropdown of sample scene.
    private static bool _mConsumeOnPurchase;
    private static bool _mConsumeOnQuery;

    private static Dropdown _mDropdown;
    private static Text _mTextField;
    private static bool _mInitialized;

    private static readonly PurchaseListener MPurchaseListener = new PurchaseListener();
    private static readonly InitListener MInitListener = new InitListener();
    private static readonly LicenseCheckListener MLicenseCheckListener = new LicenseCheckListener();

    private static GameObject _infoObj;
    private static GameObject _dropObj;

    private static List<string> _productIds = new List<string>();

    protected virtual void Start()
    {
        _infoObj = GameObject.Find("Information");
        _dropObj = GameObject.Find("Dropdown");

        #region Text Field Initialization

        _mTextField = _infoObj.GetComponent<Text>();
        _mTextField.text = "Please Click Init to Start";

        #endregion

        #region DropDown Initialization

        _mDropdown = _dropObj.GetComponent<Dropdown>();
        _mDropdown.ClearOptions();
        _mDropdown.RefreshShownValue();

        #endregion

        InitUi();

        LicenseCheck();
    }

    protected virtual void LicenseCheck()
    {
        Show("Start license check");
        StoreService.LicenseCheck(MLicenseCheckListener);
    }

    private static void Show(string message, bool append = false)
    {
        _mTextField.text = append ? string.Format("{0}\n{1}", _mTextField.text, message) : message;
    }

    protected virtual void InitUi()
    {
        #region Button Initialization

        /*
         * Initialzie the SDK. StoreService.Initialize() function will read GameSettings.asset
         * file to initialize the SDK. However, GameSettings.asset only supports Unity whose
         * version is higher than 5.6.1 (inlcuded). If developers are using older Unity, they
         * should get these information from the UDP portal and fill the AppInfo manually.
         *
         * AppInfo appInfo = new AppInfo();
         * appInfo.AppSlug = "game id from the portal";
         * appInfo.ClientId = "client id from the portal";
         * appInfo.ClientKey = "client key from the portal";
         * appInfo.RSAPublicKey = "rsa public key from the portal";
         * StoreService.Initialize(initListener, appInfo);
         *
         * In addition, developers using lower version Unity have to save the clientId to
         * Assets/Plugins/Android/assets/GameSettings.prop manually.
         *
         * GameSettings.prop only contains a single line of clientID, for example:
         * DXVCZFVPxp8S1xkliHwYww
         */
        GetButton("InitButton").onClick.AddListener(() =>
        {
            _mInitialized = false;
            Debug.Log("Init button is clicked.");
            Show("Initializing");
            StoreService.Initialize(MInitListener);
        });

        /*
         * Purchase a product.
         *
         * The purchase flow of UDP is:
         * 1) For consumable products, e.g. 100 coins: purchase -> consume -> deliver
         * 2) For un-consumable products, e.g. removing ads: purchase -> deliver
         *
         * Through this way, if a purchase is paid but for some reason (e.g. crash of
         * the game) the player doesn't get the product, the product can be restored
         * by StoreService.QueryInventory(). Thus, make sure StoreService.QueryInventory()
         * is called after the Initialization is successful.
         */
        GetButton("BuyButton").onClick.AddListener(() =>
        {
            if (!_mInitialized)
            {
                Show("Please Initialize first");
                return;
            }

            var productId = _mDropdown.options[_mDropdown.value].text;
            Debug.Log("Buy button is clicked.");
            Show("Buying Product: " + productId);
            _mConsumeOnPurchase = false;
            Debug.Log(_mDropdown.options[_mDropdown.value].text + " will be bought");
            StoreService.Purchase(productId, "payload", MPurchaseListener);
        });

        /*
         * Purchase a product and consume it (for consumable product).
         * The API is the same but m_ConsumeOnPurchase will be set to true
         * and the product will be consumed in OnPurchase().
         */
        GetButton("BuyConsumeButton").onClick.AddListener(() =>
        {
            if (!_mInitialized)
            {
                Show("Please Initialize first");
                return;
            }

            var productId = _mDropdown.options[_mDropdown.value].text;
            Show("Buying Product: " + productId);
            Debug.Log("Buy&Consume button is clicked.");
            _mConsumeOnPurchase = true;

            StoreService.Purchase(productId, "payload2", MPurchaseListener);
        });

        /*
         * Query the Inventory. This function should be called right after the
         * initialization succeeds.
         *
         * StoreService.QueryInventory() will return two things:
         * 1) Un-consumed purchase (unconsumed consumable products & non-consumable products).
         * 2) Queried products information.
         */
        GetButton("QueryButton").onClick.AddListener(() =>
        {
            if (!_mInitialized)
            {
                Show("Please Initialize first");
                return;
            }

            _mConsumeOnQuery = false;
            Debug.Log("Query button is clicked.");
            Show("Querying Inventory");
            StoreService.QueryInventory(_productIds, MPurchaseListener);
        });

        /*
         * Query the inventory and consume the unconsumed purchase.
         */
        GetButton("QueryConsumeButton").onClick.AddListener(() =>
        {
            if (!_mInitialized)
            {
                Show("Please Initialize first");
                return;
            }

            _mConsumeOnQuery = true;
            Show("Querying Inventory");
            Debug.Log("QueryConsume button is clicked.");
            StoreService.QueryInventory(_productIds, MPurchaseListener);
        });

        #endregion
    }

    protected virtual Button GetButton(string buttonName)
    {
        var obj = GameObject.Find(buttonName);
        return obj != null ? obj.GetComponent<Button>() : null;
    }

    /// <summary>
    /// Init Listener
    /// </summary>
    private class InitListener : IInitListener
    {
        public void OnInitialized(UserInfo userInfo)
        {
            Debug.Log("[Game]On Initialized succeeded");
            Show("Initialize succeeded. Start query inventory.");
            StoreService.QueryInventory(MPurchaseListener);
            _mInitialized = true;
        }

        public void OnInitializeFailed(string message)
        {
            Debug.Log("[Game]OnInitializeFailed: " + message);
            Show("Initialize Failed: " + message);
        }
    }

    private class LicenseCheckListener : ILicensingListener
    {
        public void allow(LicensingCode code, string message)
        {
            //LicensingCode enum:
            //RETRY, LICENSED, NOT_LICENSED, STORE_NOT_SUPPORT
            Debug.Log("license check passed, code: " + code + " message: " + message);
            Show("license check passed, code: " + code + " message: " + message); //some meaningful message
        }

        public void dontAllow(LicensingCode code, string message)
        {
            //LicensingCode enum:
            //RETRY, LICENSED, NOT_LICENSED, STORE_NOT_SUPPORT
            Debug.Log("license check failed: code: " + code + " message: " + message);
            Show("license check failed: code: " + code + " message: " + message); //some meaningful message
        }

        public void applicationError(LicensingErrorCode code, string message)
        {
            //LicensingErrorCode enum:
            //ERROR_INVALID_PACKAGE_NAME, ERROR_NON_MATCHING_UID, ERROR_NOT_MARKET_MANAGED, ERROR_CHECK_IN_PROGRESS, ERROR_INVALID_PUBLIC_KEY, ERROR_MISSING_PERMISSION
            Debug.Log("license check error: code: " + code + " message: " + message);
            Show("license check error: code: " + code + " message: " + message); //some meaningful message
        }
    }

    /// <summary>
    /// Purchase Listener.
    /// </summary>
    private class PurchaseListener : IPurchaseListener
    {
        public void OnPurchase(PurchaseInfo purchaseInfo)
        {
            string message = string.Format(
                "[Game] Purchase Succeeded, productId: {0}, cpOrderId: {1}, developerPayload: {2}, storeJson: {3}, orderQueryToken: {4}",
                purchaseInfo.ProductId, purchaseInfo.GameOrderId, purchaseInfo.DeveloperPayload,
                purchaseInfo.StorePurchaseJsonString, purchaseInfo.OrderQueryToken);

            Debug.Log(message);
            Show(message);

            /*
             * If the product is consumable, consume it and deliver the product in OnPurchaseConsume().
             * Otherwise, deliver the product here.
             */

            if (!_mConsumeOnPurchase) return;
            Debug.Log("Consuming");
            StoreService.ConsumePurchase(purchaseInfo, this);
        }

        public void OnPurchaseFailed(string message, PurchaseInfo purchaseInfo)
        {
            Debug.Log("Purchase Failed: " + message);
            Show("Purchase Failed: " + message);
        }

        public void OnPurchaseRepeated(string productCode)
        {
            throw new System.NotImplementedException();
        }

        public void OnPurchaseConsume(PurchaseInfo purchaseInfo)
        {
            Show("Consume success for " + purchaseInfo.ProductId, true);
            Debug.Log("Consume success: " + purchaseInfo.ProductId);
        }

        public void OnPurchaseConsumeFailed(string message, PurchaseInfo purchaseInfo)
        {
            Debug.Log("Consume Failed: " + message);
            Show("Consume Failed: " + message);
        }

        public void OnQueryInventory(Inventory inventory)
        {
            Debug.Log("OnQueryInventory");
            Debug.Log("[Game] Product List: ");
            var message = "Product List: \n";

            _productIds = new List<string>();
            foreach (var productInfo in inventory.GetProductDictionary())
            {
                Debug.Log("[Game] Returned product: " + productInfo.Key + " " + productInfo.Value.ProductId);
                message += string.Format("{0}:\n" +
                                         "\tTitle: {1}\n" +
                                         "\tDescription: {2}\n" +
                                         "\tConsumable: {3}\n" +
                                         "\tPrice: {4}\n" +
                                         "\tCurrency: {5}\n" +
                                         "\tPriceAmountMicros: {6}\n" +
                                         "\tItemType: {7}\n",
                    productInfo.Key,
                    productInfo.Value.Title,
                    productInfo.Value.Description,
                    productInfo.Value.Consumable,
                    productInfo.Value.Price,
                    productInfo.Value.Currency,
                    productInfo.Value.PriceAmountMicros,
                    productInfo.Value.ItemType
                );

                _productIds.Add(productInfo.Value.ProductId);
            }

            message += "\nPurchase List: \n";

            foreach (var purchaseInfo in inventory.GetPurchaseDictionary())
            {
                Debug.Log("[Game] Returned purchase: " + purchaseInfo.Key + " orderQueryToken: " +
                          purchaseInfo.Value.OrderQueryToken + " gameOrderId: " + purchaseInfo.Value.GameOrderId);
                message += string.Format("{0}\n", purchaseInfo.Value.ProductId);
            }

            Show(message);

            // update dropdown list
            _mDropdown = _dropObj.GetComponent<Dropdown>();
            _mDropdown.ClearOptions();
            foreach (var id in _productIds)
            {
                _mDropdown.options.Add(new Dropdown.OptionData(id));
            }

            _mDropdown.RefreshShownValue();

            if (_mConsumeOnQuery)
            {
                StoreService.ConsumePurchase(inventory.GetPurchaseList(), this);
            }
        }

        public void OnQueryInventoryFailed(string message)
        {
            Debug.Log("OnQueryInventory Failed: " + message);
            Show("QueryInventory Failed: " + message);
        }
    }
}