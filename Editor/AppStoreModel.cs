using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UnityEngine.UDP.Editor
{
    public class TestAccount
    {
        public string email;
        public string password;
        public string playerId;
        public bool deleted = false;
        public bool isUpdate = true;

        public TestAccount()
        {
            this.isUpdate = false;
            this.email = "";
            this.password = "";
            this.playerId = "";
            this.deleted = false;
        }

        public string Validate()
        {
            if (deleted)
            {
                return "";
            }

            if (string.IsNullOrEmpty(email) || email == "Email")
            {
                return "Email cannot be null";
            }
            else if (string.IsNullOrEmpty(password) || password == "Password")
            {
                return "Password cannot be null";
            }
            else if (password.Length < 6)
            {
                return "The min length of password is 6";
            }
            else if (!CheckEmailAddr())
            {
                return "Email is not valid";
            }

            return "";
        }

        private bool CheckEmailAddr()
        {
            string pattern =
                @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$";
            return new Regex(pattern, RegexOptions.IgnoreCase).IsMatch(email);
        }
    }

    [Serializable]
    public class UnityClientResponse : GeneralResponse
    {
        public string client_id;
        public string client_secret;
        public UnityChannelResponse channel;
    }

    public class UnityIapItemUpdateResponse : GeneralResponse
    {
    }

    public class UnityIapItemCreateResponse : GeneralResponse
    {
    }

    [Serializable]
    public class UnityChannelResponse
    {
        public string projectGuid;
        public string callbackUrl;
        public string publicRSAKey;
        public string channelSecret;
    }

    [Serializable]
    public class TokenRequest
    {
        public string token;
        public string projectId;
    }

    [Serializable]
    public class TokenInfo : GeneralResponse
    {
        public string token;
        public string orgId;
        public bool canManage;
    }

    [Serializable]
    public class GeneralResponse
    {
        public string message;
    }

    [Serializable]
    public class IapItem
    {
        public string id;
        public string slug;
        public string name;
        public string masterItemSlug;
        public bool consumable = true;
        public string type = "IAP";
        public string status = "STAGE";
        public string ownerId;
        public string ownerType = "ORGANIZATION";

        public PriceSets priceSets = new PriceSets();
        //public Locales locales;

        public Properties properties = new Properties();
        //		public string refresh_token;

        private int CheckValidation()
        {
            if (string.IsNullOrEmpty(slug))
            {
                return -1;
            }

            if (string.IsNullOrEmpty(name))
            {
                return -2;
            }

            if (string.IsNullOrEmpty(masterItemSlug))
            {
                return -3;
            }

            if (priceSets == null || priceSets.PurchaseFee == null
                                  || priceSets.PurchaseFee.priceMap == null
                                  || priceSets.PurchaseFee.priceMap.DEFAULT.Count.Equals(0)
                                  || string.IsNullOrEmpty(priceSets.PurchaseFee.priceMap.DEFAULT[0].price))
            {
                return -4;
            }

            if (properties == null || string.IsNullOrEmpty(properties.description))
            {
                return -6;
            }

            var price = priceSets.PurchaseFee.priceMap.DEFAULT[0].price;
            if (!Regex.IsMatch(price, @"(^[1-9]\d*(\.\d{1,2})?$)|(^0(\.\d{1,2})?$)"))
            {
                return -7;
            }

            if (price.Length > 60)
            {
                return -8;
            }

            if (slug.Length > 128)
            {
                return -9;
            }

            if (name.Length > 128)
            {
                return -10;
            }

            if (properties.description.Length > 600)
            {
                return -11;
            }

            return 0;
        }

        public string Validate()
        {
            int result = CheckValidation();

            switch (result)
            {
                case 0:
                    return "";
                case -1:
                    return "Product ID cannot be null";
                case -2:
                    return "Name cannot be null";
                case -4:
                    return "Price cannot be null";
                case -6:
                    return "Description cannot be null";
                case -7:
                    return "Invalid price format";
                case -8:
                    return "Price max length:60";
                case -9:
                    return "Slug max length:128";
                case -10:
                    return "Name max length:128";
                case -11:
                    return "Description max length:600";

                default:
                    return "Unknown error, please retry";
            }
        }

        public string SlugValidate(HashSet<string> slugs)
        {
            if (slugs.Contains(slug))
            {
                return "Slug occupied by another item";
            }

            slugs.Add(slug);
            return "";
        }
    }

    [Serializable]
    public class PriceSets
    {
        public PurchaseFee PurchaseFee = new PurchaseFee();
    }

    [Serializable]
    public class PurchaseFee
    {
        public string priceType = "CUSTOM";
        public PriceMap priceMap = new PriceMap();
    }

    [Serializable]
    public class PriceMap
    {
        public List<PriceDetail> DEFAULT = new List<PriceDetail>();
    }

    [Serializable]
    public class PriceDetail
    {
        public string price;
        public string currency;
    }

    [Serializable]
    public class PlayerResponse : GeneralResponse
    {
        public string nickName;
        public string id;
    }

    [Serializable]
    public class PlayerResponseWrapper : GeneralResponse
    {
        public int total;
        public PlayerResponse[] results;
    }

    [Serializable]
    public class AppItem
    {
        public string id;
        public string type;
        public string slug;
        public string name;
        public string status;
        public string ownerId;
        public string ownerType;
        public string clientId;
        public string packageName;
        public string revision;
    }

    [Serializable]
    public class Properties
    {
        public string description;
    }

    [Serializable]
    public class AppItemResponse : GeneralResponse
    {
        public string slug;
        public string name;
        public string id;
        public string status;
        public string type;
        public string ownerId;
        public string ownerType;
        public string clientId;
        public string packageName;
        public string revision;
    }

    [Serializable]
    public class PlayerDeleteResponse : GeneralResponse
    {
    }

    [Serializable]
    public class IapItemDeleteResponse : GeneralResponse
    {
    }

    [Serializable]
    public class ErrorResponse : GeneralResponse
    {
        public string errorCode;
        public string target;
    }

    [Serializable]
    public class AllDataResponse : GeneralResponse
    {
        public AppItemResponse appItem;
        public IapItem[] iapItems;
        public PlayerResponse[] players;
        public UnityClientResponse client;
    }

    [Serializable]
    public class SimpleIapPayload
    {
        public string id;
        public string slug;
        public string name;
        public bool consumable;
        public string price;
        public string description;
    }

    [Serializable]
    public class SimplePlayerPayload
    {
        public string id;
        public string email;
        public string password;
    }

    [Serializable]
    public class FullUpdatePayload
    {
        public string clientId;
        public string gameTitle;
        public string callbackUrl;
        public SimpleIapPayload[] iapItems;
        public SimplePlayerPayload[] testAccounts;
    }

    public class FullUpdateResponse : GeneralResponse
    {
        public AllDataResponse allDataResponse;
        public string[] iapSuccess;
        public string[] testAccountSuccess;
        public string callbackUrlErrorMsg;
        public string gameTitleErrorMsg;
    }

    public class LinkProjectPayload
    {
        public string clientId;
    }
}