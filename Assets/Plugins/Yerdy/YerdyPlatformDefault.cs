using System.Collections.Generic;
using GameObject = UnityEngine.GameObject;

/// <summary>
/// The "default" platform binding.  Logs some calls to console & emulates some of the
/// callbacks
/// </summary>
public class YerdyPlatformDefault : IYerdyPlatformBinding
{
	/// <summary>
	/// Toggle this to 'true' to enable debug logging while running in the Unity editor
	/// </summary>
	private static readonly bool EnableLogging = true;
	
	
	private static void Log(string fmt, params object[] args)
	{
		if (EnableLogging) {
			string formatted = string.Format(fmt, args);
			UnityEngine.Debug.Log("[Yerdy] " + formatted);
		}
	}
	
	private static string FormatDictionary<TKey,TValue>(Dictionary<TKey, TValue> currencies)
	{
		List<string> components = new List<string>();
		foreach (var kvp in currencies) {
			components.Add(kvp.Key.ToString() + "=" + kvp.Value.ToString());
		}
		return string.Join(",", components.ToArray());
	}
	
	
	public void Init (string publisherKey, string currency0, string currency1, string currency2, string currency3, string currency4, string currency5)
	{
		Log("Starting with publisher key: {0}", publisherKey);
		Log ("Configure Currencies: {0}, {1}, {2}, {3}, {4}, {5}", currency0, currency1, currency2, currency3, currency4, currency5);
		
		GameObject callbacks = GameObject.Find(typeof(YerdyCallbacks).ToString());
		callbacks.SendMessage("_YerdyConnected", string.Empty);
	}
	
	public void SetLogLevel (Yerdy.LogLevel level)
	{
		Log ("Log level: " + level.ToString());
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
	
	public void SetMaxFailoverCount (string placement, int count)
	{
	}
	
	public void EarnedCurrencies (Dictionary<string, uint> currencies)
	{
		Log("Earned currency: {0}", FormatDictionary(currencies));
	}
	
	public void PurchasedItem (string itemName, Dictionary<string, uint> currencies, bool onSale)
	{
		Log("Purchased item {0} for {1}", itemName, FormatDictionary(currencies));
	}
	
	public void PurchasedInApp (YerdyPurchase purchase, Dictionary<string, uint> currencies)
	{
		Log("Purchased in app: {0}, received currencies: {1}", purchase, FormatDictionary(currencies));
	}
		
	public void SetUserAsPreYerdy (Dictionary<string, uint> existingCurrencies)
	{
		Log ("Set user as pre Yerdy user. Currencies: {0}", FormatDictionary(existingCurrencies));
	}
	
	public void SetShouldTrackPreYerdyUserProgression(bool shouldTrackPreYerdyUserProgression)
	{
		Log ("Should track pre Yerdy user progression: " + shouldTrackPreYerdyUserProgression);
	}
	
	public void LogPlayerProgression (string category, string milestone)
	{
		Log ("Player progression: {0}  milestone: {1}", category, milestone);
	}
	
	public void LogScreenVisit (string screenName)
	{
		Log	("Screen visit: {0}", screenName);
	}
	
	public void LogEvent (string eventName, Dictionary<string, string> parameters)
	{
		Log ("Log event: {0}, {1}", eventName, FormatDictionary(parameters));
	}

	public void LogAdRequest(string adNetworkName)
	{
		Log ("Log ad request: " + adNetworkName);
	}

	public void LogAdFill(string adNetworkName)
	{
		Log ("Log ad fill: " + adNetworkName);
	}
}
