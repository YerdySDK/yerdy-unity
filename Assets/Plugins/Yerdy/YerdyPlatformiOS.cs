using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Debug = UnityEngine.Debug;

#if UNITY_IPHONE
public class YerdyPlatformiOS : IYerdyPlatformBinding
{
	[DllImport("__Internal")]
	private static extern void _Yerdy_Init(string publisherKey);
	[DllImport("__Internal")]
	private static extern void _Yerdy_SetLogLevel(int level);
	[DllImport("__Internal")]
	private static extern void _Yerdy_SetiOSPushToken(byte[] pushToken, int length);
	[DllImport("__Internal")]
	private static extern bool _Yerdy_IsPremiumUser();
	[DllImport("__Internal")]
	private static extern bool _Yerdy_ShouldShowAnotherMessageAfterUserCancel_get();
	[DllImport("__Internal")]
	private static extern void _Yerdy_ShouldShowAnotherMessageAfterUserCancel_set(bool value);
	[DllImport("__Internal")]
	private static extern bool _Yerdy_IsMessageAvailable(string placement);
	[DllImport("__Internal")]
	private static extern bool _Yerdy_ShowMessage(string placement);
	[DllImport("__Internal")]
	private static extern void _Yerdy_DismissMessage();
	[DllImport("__Internal")]
	private static extern void _Yerdy_ConfigureCurrencies(string currency0, string currency1, string currency2, 
		string currency3, string currency4, string currency5);
	[DllImport("__Internal")]
	private static extern void _Yerdy_EarnedCurrencies(string currencies);
	[DllImport("__Internal")]
	private static extern void _Yerdy_PurchasedItem(string itemName, string currencies, bool onSale);
	[DllImport("__Internal")]
	private static extern void _Yerdy_PurchasedInApp(string productIdentifier, string transactionIdentifer, string currencies, bool onSale);
	[DllImport("__Internal")]
	private static extern void _Yerdy_SetUserAsPreYerdy(string currencies);
	[DllImport("__Internal")]
	private static extern void _Yerdy_SetShouldTrackPreYerdyUserProgression(bool shouldTrackPreYerdyUserProgression);
	[DllImport("__Internal")]
	private static extern void _Yerdy_StartPlayerProgression(string category, string milestone);
	[DllImport("__Internal")]
	private static extern void _Yerdy_LogPlayerProgression(string category, string milestone);
	[DllImport("__Internal")]
	private static extern void _Yerdy_LogScreenVisit(string screenVisit);
	[DllImport("__Internal")]
	private static extern void _Yerdy_LogEvent(string eventName, string parameters);
	[DllImport("__Internal")]
	private static extern void _Yerdy_LogAdRequest(string adNetworkName);
	[DllImport("__Internal")]
	private static extern void _Yerdy_LogAdFill(string adNetworkName);

	// Converts a List to a string that can be deserialized on the iOS end.
	// Format:  <item1>[RS]<item2>[RS]  ([RS] is the ASCII record separator)
	private static string SerializeList<T>(List<T> list)
	{
		char RS = (char)30;
		
		StringBuilder sb = new StringBuilder();
		foreach (var item in list)
		{
			sb.Append(item.ToString());
			sb.Append(RS);
		}
		return sb.ToString();
	}
	
	// Converts a Dictionary to a string that can be deserialized on the iOS end.
	// Format:  <key1>[US]<value1>[RS]<key2>[US]<value2>[US]...  ([RS]/[US] are the ASCII record/unit separator characters)
	private static string SerializeDictionary<TKey, TValue>(Dictionary<TKey, TValue> dict)
	{		
		char RS = (char)30, US = (char)31;
		
		StringBuilder sb = new StringBuilder();
		foreach (var kvp in dict) {
			sb.Append(kvp.Key.ToString());
			sb.Append(US);
			sb.Append(kvp.Value.ToString());
			sb.Append(RS);
		}
		return sb.ToString();
	}
	
	
	public void Init (string publisherKey, string currency0, string currency1, string currency2, string currency3, string currency4, string currency5)
	{
		_Yerdy_Init(publisherKey);
		_Yerdy_ConfigureCurrencies(currency0, currency1, currency2, currency3, currency4, currency5);
	}
	
	public void SetLogLevel(Yerdy.LogLevel level)
	{
		// iOS constants
		const int Silent = 0, Error = 1, Warn = 2, Info = 3, Debug = 4;
		
		int enumValue = Warn;
		
		switch (level) {
		case Yerdy.LogLevel.Silent:
			enumValue = Silent; break;
		case Yerdy.LogLevel.Error:
			enumValue = Error; break;
		case Yerdy.LogLevel.Warn:
			enumValue = Warn; break;
		case Yerdy.LogLevel.Info:
			enumValue = Info; break;
		case Yerdy.LogLevel.Debug:
			enumValue = Debug; break;
		case Yerdy.LogLevel.Verbose: // no "Verbose" equivalent on iOS, use the closest level
			enumValue = Debug; break;
		}
		
		_Yerdy_SetLogLevel(enumValue);
	}
	
	public void SetiOSPushToken (byte[] pushToken)
	{
		_Yerdy_SetiOSPushToken(pushToken, pushToken.Length);
	}
	
	public void ConfigureGoogleLVL(string lvlKey)
	{
		//Only applicable for Android applications
	}

	public void SetAndroidPlatform(Yerdy.AndroidPlatform platform)
	{
		//Only applicable for Android applications
	}
	
	public bool IsPremiumUser ()
	{
		return _Yerdy_IsPremiumUser();
	}

	public bool ShouldShowAnotherMessageAfterUserCancel
	{
		get
		{
			return _Yerdy_ShouldShowAnotherMessageAfterUserCancel_get();
		}
		set
		{
			_Yerdy_ShouldShowAnotherMessageAfterUserCancel_set(value);
		}
	}

	public bool IsMessageAvailable (string placement)
	{
		return _Yerdy_IsMessageAvailable(placement);
	}
	
	public bool ShowMessage (string placement)
	{
		return _Yerdy_ShowMessage(placement);
	}
	
	public void DismissMessage ()
	{
		_Yerdy_DismissMessage();
	}
	
	public void EarnedCurrencies (Dictionary<string, int> currencies)
	{
		var currenciesString = currencies != null ? SerializeDictionary(currencies) : null;
		_Yerdy_EarnedCurrencies(currenciesString);
	}
	
	public void PurchasedItem (string itemName, Dictionary<string, int> currencies, bool onSale)
	{
		var currenciesString = currencies != null ? SerializeDictionary(currencies) : null;
		_Yerdy_PurchasedItem(itemName, currenciesString, onSale);
	}
	
	public void PurchasedInApp (YerdyPurchase purchase, Dictionary<string, int> currencies)
	{
		var iosPurchase = purchase as YerdyPurchaseiOS;
		if (iosPurchase == null) {
			Debug.LogError("Failed to report iOS purchase, 'purchase' was not a YerdyPurchaseiOS instance");
			return;
		}
		
		var currenciesString = currencies != null ? SerializeDictionary(currencies) : null;
		_Yerdy_PurchasedInApp(iosPurchase.ProductIdentifier, iosPurchase.TransactionIdentifier, currenciesString, iosPurchase.OnSale);
	}
	
	public void SetUserAsPreYerdy (Dictionary<string, int> existingCurrencies)
	{
		var currenciesString = existingCurrencies != null ? SerializeDictionary(existingCurrencies) : null;
		_Yerdy_SetUserAsPreYerdy(currenciesString);
	}
	
	public void SetShouldTrackPreYerdyUserProgression (bool shouldTrackPreYerdyUserProgression)
	{
		_Yerdy_SetShouldTrackPreYerdyUserProgression(shouldTrackPreYerdyUserProgression);
	}

	public void StartPlayerProgression (string category, string milestone)
	{
		_Yerdy_StartPlayerProgression(category, milestone);
	}

	public void LogPlayerProgression (string category, string milestone)
	{
		_Yerdy_LogPlayerProgression(category, milestone);
	}
	
	public void LogScreenVisit (string screenName)
	{
		_Yerdy_LogScreenVisit(screenName);
	}
	
	public void LogEvent (string eventName, Dictionary<string, string> parameters)
	{
		_Yerdy_LogEvent(eventName, SerializeDictionary(parameters));
	}

	public void LogAdRequest (string adNetworkName)
	{
		_Yerdy_LogAdRequest(adNetworkName);
	}
	
	public void LogAdFill (string adNetworkName)
	{
		_Yerdy_LogAdFill(adNetworkName);
	}
}
#endif