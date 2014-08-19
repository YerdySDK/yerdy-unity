using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Callbacks receieved from the Yerdy plugin
/// 
/// Add delegates to the static Actions to receive callbacks when certain events happen
/// </summary>
public class YerdyCallbacks : MonoBehaviour
{
	/// <summary>
	/// When Yerdy has contacted the servers
	/// </summary>
	public static Action YerdyConnected;
	
	/// <summary>
	/// Called right before a message is presented
	/// The string parameter is the placement passed into Yerdy.ShowMessage()
	/// </summary>
	public static Action<string> WillPresentMessage;
	
	/// <summary>
	/// Called right after a message is presented
	/// The string parameter is the placement passed into Yerdy.ShowMessage()
	/// </summary>
	public static Action<string> DidPresentMessage;
	
	/// <summary>
	/// Called right before a message is dismissed
	/// The string parameter is the placement passed into Yerdy.ShowMessage()
	/// </summary>
	public static Action<string> WillDismissMessage;
	
	/// <summary>
	/// Called right after a message is dismissed
	/// The string parameter is the placement passed into Yerdy.ShowMessage()
	/// </summary>
	public static Action<string> DidDismissMessage;
	
	/// <summary>
	/// Called when your app should handle an in-app purchase
	/// The string parameter is the product identifier
	/// </summary>
	public static Action<string> HandleInAppPurchase;
	
	/// <summary>
	/// Called when your app should handle an in-game item purchase
	/// The string parameter is the in-game item to purchase
	/// </summary>
	public static Action<string> HandleItemPurchase;
	
	/// <summary>
	/// Called when you apps should handle rewards
	/// The parameter is a dictionary with reward names as the keys and amounts as the values
	/// </summary>
	public static Action<Dictionary<string, int>> HandleRewards;

	/// <summary>
	/// Called when you apps should navigate to a specific screen
	/// The string parameter is the name of the screen
	/// </summary>
	public static Action<string> HandleNavigation;

	/// <summary>
	/// Called if you provide Yerdy with the aplciation's Google LVL key
	/// The parameter is validation result
	/// </summary>
	public static Action<string> GoogleLVLStatusChanged;
	
	void Awake()
	{
		gameObject.name = this.GetType().ToString();
		DontDestroyOnLoad(gameObject);
	}
	
	
	void _YerdyConnected(string arg)
	{
		if (YerdyConnected != null)
			YerdyConnected();
	}
	
	void _WillPresentMessage(string placement)
	{
		if (WillPresentMessage != null)
			WillPresentMessage(placement);
	}
	
	void _DidPresentMessage(string placement)
	{
		if (DidPresentMessage != null)
			DidPresentMessage(placement);
	}
	
	void _WillDismissMessage(string placement)
	{
		if (WillDismissMessage != null)
			WillDismissMessage(placement);
	}
	
	void _DidDismissMessage(string placement)
	{
		if (DidDismissMessage != null)
			DidDismissMessage(placement);
	}
	
	void _HandleInAppPurchase(string productIdentifier)
	{
		if (HandleInAppPurchase != null) {
			HandleInAppPurchase(productIdentifier);
		} else {
			Debug.LogError("ERROR: No delegate set for YerdyCallbacks.HandleInAppPurchase.  " +
				"Yerdy in app purchase handling will not work");
		}
	}
	
	void _HandleItemPurchase(string itemName)
	{
		if (HandleItemPurchase != null) {
			HandleItemPurchase(itemName);
		} else {
			Debug.LogError("ERROR: No delegate set for YerdyCallbacks.HandleItemPurchase.  " +
				"Yerdy in-game item handling will not work");
		}
	}
	
	void _HandleRewards(string rewards)
	{
		// expects 'rewards' to be a string in the format:
		// <name 1>,<amount 1>;<name 2>,<amount 2>;<...>
		
		var parsedRewards = new Dictionary<string, int>();
		var split = rewards.Split(';');
		foreach (string nameAmountPair in split) {
			var parts = nameAmountPair.Split(',');
			int amount;
			if (parts.Length == 2 && int.TryParse(parts[1], NumberStyles.Integer, 
					CultureInfo.InvariantCulture, out amount)) {
				parsedRewards.Add(parts[0], amount);
			}
		}
		
		if (HandleRewards != null) {
			HandleRewards(parsedRewards);
		} else {
			Debug.LogError("ERROR: No delegate set for YerdyCallbacks.HandleRewards.  " +
				"Yerdy rewards handling will not work");
		}
	}

	void _HandleNavigation(string screenName)
	{
		if (HandleNavigation != null) {
			HandleNavigation(screenName);
		} else {
			Debug.LogError("ERROR: No delegate set for YerdyCallbacks.HandleNavigation.  " +
			               "Yerdy screen navigation handling will not work");
		}
	}

	void _GoogleLVLStatusChanged(string status)
	{
		if (GoogleLVLStatusChanged != null)
			GoogleLVLStatusChanged(status);
	}
}
