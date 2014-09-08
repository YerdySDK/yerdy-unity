using System.Collections.Generic;
using System.Linq;
using Logger = YerdyEditorLogger;
using GameObject = UnityEngine.GameObject;

/// <summary>
/// The "default" platform binding.  Logs some calls to console & emulates some of the
/// callbacks
/// </summary>
public class YerdyPlatformDefault : IYerdyPlatformBinding
{
	private HashSet<string> validCurrencies = new HashSet<string>();


	public void Init (string publisherKey, string currency0, string currency1, string currency2, string currency3, string currency4, string currency5)
	{
		Logger.Info("Starting with publisher key: {0}", publisherKey);
		Logger.Info("Configure Currencies: {0}, {1}, {2}, {3}, {4}, {5}", currency0, currency1, currency2, currency3, currency4, currency5);

		// add all non-null currencies to validCurrencies
		IEnumerable<string> allCurrencies = new string[] { currency0, currency1, currency2, currency3, currency4, currency5 };
		validCurrencies.UnionWith(allCurrencies.Where(x => x != null));

		GameObject callbacks = GameObject.Find(typeof(YerdyCallbacks).ToString());
		callbacks.SendMessage("_YerdyConnected", string.Empty);
	}
	
	public void SetLogLevel (Yerdy.LogLevel level)
	{
		Logger.LogLevel = level;
	}
	
	public void ConfigureGoogleLVL(string lvlKey)
	{
		//Only applicable for Android applications
	}
	
	public void SetAndroidPlatform(Yerdy.AndroidPlatform platform)
	{
		//Only applicable for Android applications
	}
	
	public void SetiOSPushToken (byte[] pushToken)
	{
		//Only applicable for iOS applications
	}
	
	public bool IsPremiumUser ()
	{
		return false;
	}

	public bool ShouldShowAnotherMessageAfterUserCancel { get; set; }
	
	public bool IsMessageAvailable (string placement)
	{
		return false;
	}
	
	public bool ShowMessage (string placement)
	{
		return false;
	}
	
	public void DismissMessage ()
	{
	}

	public void EarnedCurrencies (Dictionary<string, int> currencies)
	{
		currencies = CheckAndClampCurrencyValues(currencies, "EarnedCurrencies");
		Logger.Info("Earned currency: {0}", FormatDictionary(currencies));
	}
	
	public void PurchasedItem (string itemName, Dictionary<string, int> currencies, bool onSale)
	{
		currencies = CheckAndClampCurrencyValues(currencies, "PurchasedItem");
		Logger.Info("Purchased item {0} for {1}", itemName, FormatDictionary(currencies));
	}
	
	public void PurchasedInApp (YerdyPurchase purchase, Dictionary<string, int> currencies)
	{
		currencies = CheckAndClampCurrencyValues(currencies, "PurchasedInApp");
		Logger.Info("Purchased in app: {0}, received currencies: {1}", purchase, FormatDictionary(currencies));
	}
		
	public void SetUserAsPreYerdy (Dictionary<string, int> existingCurrencies)
	{
		existingCurrencies = CheckAndClampCurrencyValues(existingCurrencies, "SetUserAsPreYerdy");
		Logger.Info("Set user as pre Yerdy user. Currencies: {0}", FormatDictionary(existingCurrencies));
	}
	
	public void SetShouldTrackPreYerdyUserProgression(bool shouldTrackPreYerdyUserProgression)
	{
		Logger.Info("Should track pre Yerdy user progression: " + shouldTrackPreYerdyUserProgression);
	}

	public void StartPlayerProgression (string category, string milestone)
	{
		Logger.Info("Start player progression: {0}  milestone: {1}", category, milestone);
	}

	public void LogPlayerProgression (string category, string milestone)
	{
		Logger.Info("Player progression: {0}  milestone: {1}", category, milestone);
	}
	
	public void LogFeatureUse (string feature)
	{
		Logger.Info("Feature use: {0}", feature);
	}

	public void SetFeatureUseLevels(int usesForNovice, int usesForAmateur, int usesForMaster)
	{
	}
	
	public void SetFeatureUseLevels(string feature, int usesForNovice, int usesForAmateur, int usesForMaster)
	{
	}
	
	public void LogEvent (string eventName, Dictionary<string, string> parameters)
	{
		Logger.Info("Log event: {0}, {1}", eventName, FormatDictionary(parameters));
	}

	public void LogAdRequest(string adNetworkName)
	{
		Logger.Info("Log ad request: " + adNetworkName);
	}

	public void LogAdFill(string adNetworkName)
	{
		Logger.Info("Log ad fill: " + adNetworkName);
	}


	private static string FormatDictionary<TKey,TValue>(Dictionary<TKey, TValue> currencies)
	{
		List<string> components = new List<string>();
		foreach (var kvp in currencies) {
			components.Add(kvp.Key.ToString() + "=" + kvp.Value.ToString());
		}
		return string.Join(",", components.ToArray());
	}

	private Dictionary<string, int> CheckAndClampCurrencyValues(Dictionary<string, int> currencies, string callingMethod) {
		Dictionary<string, int> clamped = new Dictionary<string, int>();
		foreach (var kvp in currencies) {
			if (validCurrencies.Contains(kvp.Key) == false) {
				Logger.Error("{0} - Invalid currency name '{1}'.  Make sure you register it in Yerdy.Init(...)", callingMethod, kvp.Key);
			} else if (kvp.Value < 0) {
				Logger.Error("{0} - Invalid value for currency '{1}': '{2}'.  All currencies must be >= 0.  Clamping value to 0...", callingMethod, kvp.Key, kvp.Value);
				clamped[kvp.Key] = 0;
			} else {
				clamped[kvp.Key] = kvp.Value;
			}
		}
		return clamped;
	}
}
