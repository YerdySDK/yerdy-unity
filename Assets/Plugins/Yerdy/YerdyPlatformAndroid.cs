using UnityEngine;
using System.Collections.Generic;
using System;

#if UNITY_ANDROID
public class YerdyPlatformAndroid : IYerdyPlatformBinding
{
	public void Init (string publisherKey, string currency0, string currency1, string currency2, string currency3, string currency4, string currency5)
	{
		using(AndroidJavaObject binding = new AndroidJavaObject("com.yerdy.services.YerdyUnity")) {
			binding.Call("configureCurrencies", currency0, currency1, currency2, currency3, currency4, currency5);
			binding.Call("startWithPublisherKey", publisherKey);
		}

		// Spawn YerdyCallbacks if it isn't already in the game
		var androidSupport = GameObject.Find(typeof(YerdyAndroidSupport).ToString());
		if (androidSupport == null) {
			var support = new GameObject().AddComponent<YerdyAndroidSupport>();
			support.FocusDelegate += (focus) => {
				using(AndroidJavaObject binding = new AndroidJavaObject("com.yerdy.services.YerdyUnity")) {
					binding.Call("_onWindowFocusChanged", focus);
				}
			};
		}
	}
	
	public void SetLogLevel (Yerdy.LogLevel level)
	{
		using(AndroidJavaObject logLevel = new AndroidJavaClass("com.yerdy.services.logging.YRDLogLevel").GetStatic<AndroidJavaObject>(getLogKey(level))) {
			using(AndroidJavaObject binding = new AndroidJavaObject("com.yerdy.services.YerdyUnity")) {
				binding.Call("setLogLevel", logLevel);
			}
		}
	}

	public void ConfigureGoogleLVL(string lvlKey)
	{
		using(AndroidJavaObject binding = new AndroidJavaObject("com.yerdy.services.YerdyUnity")) {
			binding.Call("configureGoogleLVLKey", lvlKey);
		}
	}
	
	public void SetAndroidPlatform(Yerdy.AndroidPlatform platform)
	{
		using(AndroidJavaObject androidPlatform = new AndroidJavaClass("com.yerdy.services.util.YRDPlatform").GetStatic<AndroidJavaObject>(getPlatformKey(platform))) {
			using(AndroidJavaObject binding = new AndroidJavaObject("com.yerdy.services.YerdyUnity")) {
				binding.Call("configureAppPlatform", androidPlatform);
			}
		}
	}
	
	public void SetiOSPushToken (byte[] pushToken)
	{
		// not applicable on Android, but needs to be here since it's  in IYerdyPlatformBinding
	}
	
	public bool IsPremiumUser ()
	{
		bool response = false;
		using(AndroidJavaObject binding = new AndroidJavaObject("com.yerdy.services.YerdyUnity")) {
			response = binding.Call<bool>("getIsPremiumUser");
		}
		return response;
	}

	public bool ShouldShowAnotherMessageAfterUserCancel
	{ 
		get
		{
			using (AndroidJavaObject binding = new AndroidJavaObject("com.yerdy.services.YerdyUnity"))
				return binding.Call<bool>("getShouldShowAnotherMessageAfterUserCancel");
		}
		set
		{
			using (AndroidJavaObject binding = new AndroidJavaObject("com.yerdy.services.YerdyUnity"))
				binding.Call("setShouldShowAnotherMessageAfterUserCancel", value);
		}
	}
	
	public bool IsMessageAvailable (string placement)
	{
		bool response = false;
		using(AndroidJavaObject binding = new AndroidJavaObject("com.yerdy.services.YerdyUnity")) {
			response = binding.Call<bool>("isMessageAvailable", placement);
		}
		return response;
	}
	
	public bool ShowMessage (string placement)
	{
		bool response = false;
		using(AndroidJavaObject binding = new AndroidJavaObject("com.yerdy.services.YerdyUnity")) {
			response = binding.Call<bool>("showMessage", placement);
		}
		return response;
	}
	
	public void DismissMessage ()
	{
		using(AndroidJavaObject binding = new AndroidJavaObject("com.yerdy.services.YerdyUnity")) {
			binding.Call("dismissMessage");
		}
	}
	
	public void EarnedCurrencies (Dictionary<string, int> currencies)
	{
		using(AndroidJavaObject hashMap = new AndroidJavaObject("java.util.HashMap")) {
			buildHashmap(hashMap, currencies);
			using(AndroidJavaObject binding = new AndroidJavaObject("com.yerdy.services.YerdyUnity")) {
				binding.Call("earnedCurrency", hashMap);
			}
		}
	}
	
	public void PurchasedItem (string itemName, Dictionary<string, int> currencies, bool onSale)
	{
		using(AndroidJavaObject hashMap = new AndroidJavaObject("java.util.HashMap")) {
			buildHashmap(hashMap, currencies);
			using(AndroidJavaObject binding = new AndroidJavaObject("com.yerdy.services.YerdyUnity")) {
				binding.Call("purchasedItem", itemName, hashMap, onSale);
			}
		}
	}
	
	public void PurchasedInApp (YerdyPurchase purchase, Dictionary<string, int> currencies)
	{
		using(AndroidJavaObject hashMap = new AndroidJavaObject("java.util.HashMap")) {
			buildHashmap(hashMap, currencies);

			if(purchase is YerdyPurchaseGoogle) {
				YerdyPurchaseGoogle google = purchase as YerdyPurchaseGoogle;
				using(AndroidJavaObject nativePurchase = new AndroidJavaObject("com.yerdy.services.purchases.YRDPurchaseGoogle", google.ProductIdentifier, google.ProductValue, google.Receipt, google.Signature, google.IsSandbox)) {
					using(AndroidJavaObject binding = new AndroidJavaObject("com.yerdy.services.YerdyUnity")) {
						binding.Call("purchasedInApp", nativePurchase, hashMap);
					}
				}
			}
			else if(purchase is YerdyPurchaseAmazon)
			{
				YerdyPurchaseAmazon amazon = purchase as YerdyPurchaseAmazon;
				using(AndroidJavaObject nativePurchase = new AndroidJavaObject("com.yerdy.services.purchases.YRDPurchaseAmazon", amazon.ProductIdentifier, amazon.ProductValue, amazon.Receipt, amazon.User, amazon.IsSandbox)) {
					using(AndroidJavaObject binding = new AndroidJavaObject("com.yerdy.services.YerdyUnity")) {
						binding.Call("purchasedInApp", nativePurchase, hashMap);
					}
				}
			}
		}
	}
	
	public void SetUserAsPreYerdy (Dictionary<string, int> existingCurrencies)
	{
		using(AndroidJavaObject hashMap = new AndroidJavaObject("java.util.HashMap")) {
			buildHashmap(hashMap, existingCurrencies);
			using(AndroidJavaObject binding = new AndroidJavaObject("com.yerdy.services.YerdyUnity")) {
				binding.Call("setExistingCurrenciesForPreYerdyUser", hashMap);
			}
		}
	}
	
	public void SetShouldTrackPreYerdyUserProgression (bool shouldTrackPreYerdyUserProgression)
	{
		using(AndroidJavaObject binding = new AndroidJavaObject("com.yerdy.services.YerdyUnity")) {
			binding.Call("setShouldTrackPreYerdyUserProgression", shouldTrackPreYerdyUserProgression);
		}
	}

	public void StartPlayerProgression (string category, string milestone)
	{
		using(AndroidJavaObject binding = new AndroidJavaObject("com.yerdy.services.YerdyUnity")) {
			binding.Call("startPlayerProgression", category, milestone);
		}
	}
	
	public void LogPlayerProgression (string category, string milestone)
	{
		using(AndroidJavaObject binding = new AndroidJavaObject("com.yerdy.services.YerdyUnity")) {
			binding.Call("logPlayerProgression", category, milestone);
		}
	}
	
	public void LogFeatureUse (string feature)
	{
		using(AndroidJavaObject binding = new AndroidJavaObject("com.yerdy.services.YerdyUnity")) {
			binding.Call("logFeatureUse", feature);
		}
	}

	public void SetFeatureUseLevels(int usesForNovice, int usesForAmateur, int usesForMaster)
	{
		using (AndroidJavaObject binding = new AndroidJavaObject("com.yerdy.services.YerdyUnity"))
			binding.Call("setFeatureUseLevels", usesForNovice, usesForAmateur, usesForMaster);
	}
	
	public void SetFeatureUseLevels(string feature, int usesForNovice, int usesForAmateur, int usesForMaster)
	{
		using (AndroidJavaObject binding = new AndroidJavaObject("com.yerdy.services.YerdyUnity"))
			binding.Call("setFeatureUseLevels", feature, usesForNovice, usesForAmateur, usesForMaster);
	}

	public void LogEvent (string eventName, Dictionary<string, string> parameters)
	{
		using(AndroidJavaObject hashMap = new AndroidJavaObject("java.util.HashMap")) {
			buildHashmap(hashMap, parameters);
			using(AndroidJavaObject binding = new AndroidJavaObject("com.yerdy.services.YerdyUnity")) {
				binding.Call("logEvent", eventName, hashMap);
			}
		}
	}

	public void LogAdRequest (string adNetworkname)
	{
		using(AndroidJavaObject binding = new AndroidJavaObject("com.yerdy.services.YerdyUnity")) {
			binding.Call("logAdRequest", adNetworkname);
		}
	}
	
	public void LogAdFill (string adNetworkname)
	{
		using(AndroidJavaObject binding = new AndroidJavaObject("com.yerdy.services.YerdyUnity")) {
			binding.Call("logAdFill", adNetworkname);
		}
	}

	private void buildHashmap(AndroidJavaObject hashMap, Dictionary<string, int> dictionary)
	{
		IntPtr method_Put = AndroidJNIHelper.GetMethodID(hashMap.GetRawClass(), "put", "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;");
		object[] args = new object[2];
		
		if(dictionary != null) {
			foreach(KeyValuePair<string, int> kvp in dictionary) {
				using(AndroidJavaObject k = new AndroidJavaObject("java.lang.String", kvp.Key)) {
					using(AndroidJavaObject v = new AndroidJavaObject("java.lang.Integer", (int)kvp.Value)) {
						args[0] = k;
						args[1] = v;
						AndroidJNI.CallObjectMethod(hashMap.GetRawObject(),
						                            method_Put, AndroidJNIHelper.CreateJNIArgArray(args));
					}
				}
			}
		}
	}
	
	private void buildHashmap(AndroidJavaObject hashMap, Dictionary<string, string> dictionary)
	{
		IntPtr method_Put = AndroidJNIHelper.GetMethodID(hashMap.GetRawClass(), "put", "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;");
		object[] args = new object[2];
		
		if(dictionary != null) {
			foreach(KeyValuePair<string, string> kvp in dictionary) {
				using(AndroidJavaObject k = new AndroidJavaObject("java.lang.String", kvp.Key)) {
					using(AndroidJavaObject v = new AndroidJavaObject("java.lang.String", kvp.Value)) {
						args[0] = k;
						args[1] = v;
						AndroidJNI.CallObjectMethod(hashMap.GetRawObject(),
						                            method_Put, AndroidJNIHelper.CreateJNIArgArray(args));
					}
				}
			}
		}
	}

	private String getLogKey(Yerdy.LogLevel level)
	{
		switch (level) {
		case Yerdy.LogLevel.Silent:
			return "YRDLogSilent";
		case Yerdy.LogLevel.Warn:
			return "YRDLogWarn";
		case Yerdy.LogLevel.Info:
			return "YRDLogInfo";
		case Yerdy.LogLevel.Debug:
			return "YRDLogDebug";
		case Yerdy.LogLevel.Verbose:
			return "YRDLogVerbose";
		case Yerdy.LogLevel.Error:
		default:
			return "YRDLogError";
		}
	}

	private String getPlatformKey(Yerdy.AndroidPlatform platform)
	{
		switch (platform) {
		case Yerdy.AndroidPlatform.Google:
			return "GOOGLE";
		case Yerdy.AndroidPlatform.Amazon:
			return "AMAZON";
		case Yerdy.AndroidPlatform.Auto:
		default:
			return "AUTO";
		}
	}

}
#endif