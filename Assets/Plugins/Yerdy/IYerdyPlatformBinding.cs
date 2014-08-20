using System.Collections.Generic;

public interface IYerdyPlatformBinding
{
	void Init(string publisherKey, string currency0, string currency1, string currency2, string currency3, string currency4, string currency5);

	void SetAndroidPlatform(Yerdy.AndroidPlatform platform);
	void ConfigureGoogleLVL(string lvlKey);
	void SetLogLevel(Yerdy.LogLevel level);
	
	void SetiOSPushToken(byte[] pushToken);
	bool IsPremiumUser();

	bool ShouldShowAnotherMessageAfterUserCancel { get; set; }
	bool IsMessageAvailable(string placement);
	bool ShowMessage(string placement);
	void DismissMessage();

	void EarnedCurrencies(Dictionary<string, int> currencies);
	void PurchasedItem(string itemName, Dictionary<string, int> currencies, bool onSale);
	void PurchasedInApp(YerdyPurchase purchase, Dictionary<string, int> currencies);
	
	void SetUserAsPreYerdy(Dictionary<string, int> existingCurrencies);
	void SetShouldTrackPreYerdyUserProgression(bool shouldTrackPreYerdyUserProgression);

	void StartPlayerProgression(string category, string milestone);
	void LogPlayerProgression(string category, string milestone);
	void LogScreenVisit(string screenName);
	void LogEvent(string eventName, Dictionary<string, string> parameters);

	void LogAdRequest(string adNetworkname);
	void LogAdFill(string adNetworkname);
}
