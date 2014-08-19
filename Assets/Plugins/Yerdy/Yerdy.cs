using System.Collections.Generic;
using GameObject = UnityEngine.GameObject;


public static class Yerdy
{
	public enum LogLevel
	{
		Silent,
		Error,
		Warn,
		Info,
		Debug,
		Verbose,
	}

	/// <summary>
	/// Auto: Auto detects store downloaded from, if not downloaded from store defaults to Google
	/// Google: Force application to report as Google
	/// Amazon: Force application to report as Amazon
	/// </summary>
	public enum AndroidPlatform
	{
		Auto,
		Google,
		Amazon
	}
	
	private static readonly IYerdyPlatformBinding binding;
	
	static Yerdy()
	{
#if UNITY_EDITOR
		binding = new YerdyPlatformDefault();
#elif UNITY_IPHONE
		binding = new YerdyPlatformiOS();
#elif UNITY_ANDROID
		binding = new YerdyPlatformAndroid();
#else
		binding = new YerdyPlatformDefault();
#endif
	}

	/// <summary>
	/// Starts Yerdy, and registers up to 6 currencies used in the app
	/// </summary>
	/// <param name='publisherKey'>
	/// Your publisher key
	/// </param>
	/// <param name='currency0'>
	/// First currency name
	/// </param>
	/// <param name='currency1'>
	/// Second currency name
	/// </param>
	/// <param name='currency2'>
	/// Third currency name
	/// </param>
	/// <param name='currency3'>
	/// Fourth currency name
	/// </param>
	/// <param name='currency4'>
	/// Fifth currency name
	/// </param>
	/// <param name='currency5'>
	/// Sixth currency name
	/// </param>
	public static void Init(string publisherKey, string currency0=null, string currency1=null, string currency2=null, string currency3=null, string currency4=null, string currency5=null)
	{
		// Spawn YerdyCallbacks if it isn't already in the game
		var yerdyCallbacks = GameObject.Find(typeof(YerdyCallbacks).ToString());
		if (yerdyCallbacks == null) {
			new GameObject().AddComponent<YerdyCallbacks>();
		}
		
		binding.Init(publisherKey, currency0, currency1, currency2, currency3, currency4, currency5);
	}
	
	/// <summary>
	/// Sets the log level
	/// </summary>
	/// <param name='level'>
	/// The log level
	/// </param>
	public static void SetLogLevel(LogLevel level)
	{
		binding.SetLogLevel(level);
	}

	/// <summary>
	/// Allows yerdy to verify the application download aginst the Google License Validation services
	/// </summary>
	/// <param name='platform'>
	/// The android platform
	/// </param>
	public static void ConfigureGoogleLVL(string lvlKey)
	{
		binding.ConfigureGoogleLVL(lvlKey);
	}

	/// <summary>
	/// Sets overrides the Android platform auto detection
	/// </summary>
	/// <param name='platform'>
	/// The android platform
	/// </param>
	public static void SetAndroidPlatform(AndroidPlatform platform)
	{
		binding.SetAndroidPlatform(platform);
	}

	/// <summary>
	/// Sets the user’s push token (iOS token)
	/// </summary>
	/// <param name='pushToken'>
	/// The value from NotificationServices.deviceToken
	/// (see http://docs.unity3d.com/Documentation/ScriptReference/NotificationServices.RegisterForRemoteNotificationTypes.html)
	/// </param>
	public static void SetiOSPushToken(byte[] pushToken)
	{
		binding.SetiOSPushToken(pushToken);
	}
	
	/// <summary>
	/// Is the user a premium user? (Do they have any validated IAP purchases?)
	/// </summary>
	/// <returns>
	/// <c>true</c> if user has a validated IAP; otherwise, <c>false</c>.
	/// </returns>
	public static bool IsPremiumUser()
	{
		return binding.IsPremiumUser();
	}
	
	/// <summary>
	/// Checks if a message is available for the given placement
	/// </summary>
	/// <returns>
	/// <c>true</c> if a message is available for the given placement; otherwise, <c>false</c>.
	/// </returns>
	/// <param name='placement'>
	/// The placement (for example, you could have “launch”, “gameover”, and “store”). Pass in <c>null</c> for any placement.
	/// </param>
	public static bool IsMessageAvailable(string placement)
	{
		return binding.IsMessageAvailable(placement);
	}
	
	/// <summary>
	/// Shows a message (if available)
	/// </summary>
	/// <returns>
	/// <c>true</c> if a message is was shown; otherwise, <c>false</c>.
	/// </returns>
	/// <param name='placement'>
	/// The placement (for example, you could have “launch”, “gameover”, and “store”). Pass in <c>null</c> for any placement.
	/// </param>
	public static bool ShowMessage(string placement)
	{
		return binding.ShowMessage(placement);
	}
	
	/// <summary>
	/// Dismisses any open messages
	/// </summary>
	public static void DismissMessage()
	{
		binding.DismissMessage();
	}
	
	/// <summary>
	/// Sets a limit to the number of “failover” messages that can be shown
	/// 
	/// If the user clicks “cancel” (or “ok” on a non actionable message), we try and show another message for that placement
	/// (until we run out of messages). You can set a limit here. (for example, if you wanted to only show 1 message no matter
	/// what, you can call:
	/// 
	/// <code>
	/// 	Yerdy.SetMaxFailoverCount("myPlacement", 0);
	/// </code>
	/// 
	/// If you would like to apply it to all placements in your app, pass in nil for placement:
	/// 
	/// <code>
	/// 	Yerdy.SetMaxFailoverCount(null, 0);
	/// </code>
	/// </summary>
	/// <param name='placement'>
	/// The placement this should apply to. To apply it to all placements, pass in <c>null</c>
	/// </param>
	/// <param name='count'>
	/// The maximun number failover messages to show
	/// </param>
	public static void SetMaxFailoverCount(string placement, int count)
	{
		binding.SetMaxFailoverCount(placement, count);
	}
	
	/// <summary>
	/// Tracks currency earned by the user.
	/// </summary>
	/// <param name='currency'>
	/// Currency name
	/// </param>
	/// <param name='amount'>
	/// The amount of currency earned
	/// </param>
	public static void EarnedCurrency(string currency, uint amount)
	{
		var currencies = new Dictionary<string, uint>()
		{
			{ currency, amount }
		};
		binding.EarnedCurrencies(currencies);
	}
	
	/// <summary>
	/// Tracks currency earned by the user.
	/// </summary>
	/// <param name='currencies'>
	/// A dictionary mapping currency names to amounts
	/// </param>
	public static void EarnedCurrency(Dictionary<string, uint> currencies)
	{
		binding.EarnedCurrencies(currencies);
	}
	
	/// <summary>
	/// Tracks in-game item purchase.
	/// </summary>
	/// <param name='item'>
	/// The name of the item.
	/// </param>
	/// <param name='currency'>
	/// The name of the currency used to purchase the item.
	/// </param>
	/// <param name='currencyAmount'>
	/// The amount of currency used to purchase the item.
	/// </param>
	/// <param name='onSale'>
	/// Whether or not the item is on sale.
	/// </param>
	public static void PurchasedItem(string item, string currency, uint currencyAmount, bool onSale=false)
	{
		var currencies = new Dictionary<string, uint>()
		{
			{ currency, currencyAmount }
		};
		binding.PurchasedItem(item, currencies, onSale);
	}
	
	/// <summary>
	/// Tracks in-game item purchase.
	/// </summary>
	/// <param name='item'>
	/// The name of the item.
	/// </param>
	/// <param name='currencies'>
	/// A dictionary mapping currency names to amounts.
	/// </param>
	/// <param name='onSale'>
	/// Whether or not the item is on sale.
	/// </param>
	public static void PurchasedItem(string item, Dictionary<string, uint> currencies, bool onSale=false)
	{
		binding.PurchasedItem(item, currencies, onSale);
	}
	
	/// <summary>
	/// Tracks in-app purchases (IAP).
	/// </summary>
	/// <param name='purchase'>
	/// A YerdyPurchase instance describing the purchase.
	/// </param>
	public static void PurchasedInApp(YerdyPurchase purchase)
	{
		binding.PurchasedInApp(purchase, null);
	}
	
	/// <summary>
	/// Tracks in-app purchases (IAP).
	/// </summary>
	/// <param name='purchase'>
	/// A YerdyPurchase instance describing the purchase.
	/// </param>
	/// <param name='currency'>
	/// The name of the currency received from the IAP.
	/// </param>
	/// <param name='amount'>
	/// The amount of the currency received from the IAP.
	/// </param>
	public static void PurchasedInApp(YerdyPurchase purchase, string currency, uint amount)
	{
		var currencies = new Dictionary<string, uint>()
		{
			{ currency, amount },
		};
		binding.PurchasedInApp(purchase, currencies);
	}
	
	/// <summary>
	/// Tracks in-app purchases (IAP).
	/// </summary>
	/// <param name='purchase'>
	/// A YerdyPurchase instance describing the purchase.
	/// </param>
	/// <param name='currencies'>
	/// A dictionary mapping currency names to amounts of the currency received from the IAP
	/// </param>
	public static void PurchasedInApp(YerdyPurchase purchase, Dictionary<string, uint> currencies)
	{
		binding.PurchasedInApp(purchase, currencies);
	}
	
	/// <summary>
	/// Marks a user as pre-Yerdy integration and sets their existing currency
	/// </summary>
	/// <param name='existingCurrencies'>
	/// A dictionary mapping currency names to amounts of the user's current currency balance
	/// </param>
	public static void SetUserAsPreYerdy(Dictionary<string, uint> existingCurrencies)
	{
		binding.SetUserAsPreYerdy(existingCurrencies);
	}
	
	/// <summary>
	/// Overrides the default behaviour of not tracking certain metrics for pre-Yerdy users
	/// </summary>
	/// <param name='shouldTrackPreYerdyUserProgression'>
	/// Should track pre yerdy user progression.
	/// </param>
	public static void SetShouldTrackPreYerdyUserProgression(bool shouldTrackPreYerdyUserProgression)
	{
		binding.SetShouldTrackPreYerdyUserProgression(shouldTrackPreYerdyUserProgression);
	}
	
	/// <summary>
	/// Logs a player progression event.
	/// 
	/// Milestones are grouped by category. For example, you may have a ‘map’ category and
	/// your milestones could be ‘unlocked world 1’, ‘unlocked world 2’, ‘unlocked world 3’, etc…
	/// </summary>
	/// <param name='category'>
	/// The category for this progression event
	/// </param>
	/// <param name='milestone'>
	/// The milestone the user reached
	/// </param>
	public static void LogPlayerProgression(string category, string milestone)
	{
		binding.LogPlayerProgression(category, milestone);
	}
	
	/// <summary>
	/// Tracks a screen visit
	/// </summary>
	/// <param name='screenName'>
	/// The name of the screen (for example: “settings”, “store”, etc…)
	/// </param>
	public static void LogScreenVisit(string screenName)
	{
		binding.LogScreenVisit(screenName);
	}
	
	/// <summary>
	/// Tracks a user-defined event
	/// </summary>
	/// <param name='eventName'>
	/// The name of the event
	/// </param>
	/// <param name='parameters'>
	/// Any parameters for the event
	/// </param>
	public static void LogEvent(string eventName, Dictionary<string, string> parameters)
	{
		binding.LogEvent(eventName, parameters);
	}

	/// <summary>
	/// Tracks an ad request
	/// </summary>
	/// <param name="adNetworkName">The ad network name</param>
	public static void LogAdRequest(string adNetworkName)
	{
		binding.LogAdRequest(adNetworkName);
	}

	/// <summary>
	/// Tracks an ad fill
	/// </summary>
	/// <param name="adNetworkName">The ad network name</param>
	public static void LogAdFill(string adNetworkName)
	{
		binding.LogAdFill(adNetworkName);
	}
}
