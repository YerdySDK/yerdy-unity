#import "Yerdy.h"
#import "YerdyCallbacks.h"
#import "YerdyTransactionObserver.h"

// Bad form to declare this here, but Unity keeps moving which header file it
// is between different Unity versions
UIWindow*           UnityGetMainWindow();


#define EXTERN_C extern "C"

static Yerdy *yerdy;
static YerdyCallbacks *yerdyCallbacks;
static YerdyTransactionObserver *yerdyTransactionObserver;

static NSString *CStringToNSString(const char *str)
{
	if (str) {
		return [NSString stringWithUTF8String:str];
	} else {
		return nil;
	}
}

static NSArray *DeserializeList(const char *str, id(^itemConverter)(NSString*)=NULL)
{
	static NSString *RS = @"\x1e";
	
	if (!itemConverter)
		itemConverter = ^id(NSString *input) { return input; };
	
	NSScanner *scanner = [NSScanner scannerWithString:CStringToNSString(str)];
	scanner.charactersToBeSkipped = nil;
	NSMutableArray *array = [NSMutableArray array];
	
	while (![scanner isAtEnd]) {
		NSString *item;
		BOOL readItem = [scanner scanUpToString:RS intoString:&item];
		[scanner scanString:RS intoString:NULL];
		
		if (readItem) {
			id converted = itemConverter(item);
			[array addObject:converted];
		}
	}
	
	return array;
}

// Deserializes a dictionary created no the C#-side by YerdyPlatformiOS.SerializeDictionary()
// Format:  <key1>[US]<value1>[RS]<key2>[US]<value2>[US]...  ([RS]/[US] are the ASCII record/unit separator characters)
// By default simply converts to NSString key/value, can be overriden with the keyConverter/valueConverter methods
static NSDictionary *DeserializeDictionary(const char *str, id(^keyConverter)(NSString*)=NULL, id(^valueConverter)(NSString*)=NULL)
{
	static NSString *RS = @"\x1e", *US = @"\x1f";
	
	if (!keyConverter)
		keyConverter = ^id(NSString *input) { return input; };
	if (!valueConverter)
		valueConverter = ^id(NSString *input) { return input; };
	
	NSScanner *scanner = [NSScanner scannerWithString:CStringToNSString(str)];
	scanner.charactersToBeSkipped = nil;
	NSMutableDictionary *dictionary = [NSMutableDictionary dictionary];
	
	while (![scanner isAtEnd]) {
		NSString *key;
		BOOL readKey = [scanner scanUpToString:US intoString:&key];
		[scanner scanString:US intoString:NULL];
		
		NSString *value;
		BOOL readValue = [scanner scanUpToString:RS intoString:&value];
		[scanner scanString:RS intoString:NULL];
		
		if (readKey && readValue) {
			id convertedKey = keyConverter(key);
			id convertedValue = valueConverter(value);
			dictionary[convertedKey] = convertedValue;
		}
	}
	
	return dictionary;
}

static Yerdy *YerdyInstance()
{
	if (yerdy == nil) {
		NSLog(@"ERROR: Yerdy.Init() must be called first");
	}
	return yerdy;
}


EXTERN_C void _Yerdy_Init(const char *publisherKey)
{
	if (yerdy)
		return;
	
	yerdy = [Yerdy startWithPublisherKey:CStringToNSString(publisherKey)];
	yerdyCallbacks = [[YerdyCallbacks alloc] init];
	yerdyTransactionObserver = [[YerdyTransactionObserver alloc] init];
	
	yerdy.delegate = yerdyCallbacks;
	yerdy.messageDelegate = yerdyCallbacks;
}

EXTERN_C void _Yerdy_SetLogLevel(int level)
{
	[Yerdy setLogLevel:(YRDLogLevel)level];
}

EXTERN_C void _Yerdy_SetiOSPushToken(unsigned char *bytes, int length)
{
	if (bytes != NULL) {
		NSData *data = [NSData dataWithBytes:bytes length:length];
		yerdy.pushToken = data;
	} else {
		yerdy.pushToken = nil;
	}
}

EXTERN_C bool _Yerdy_IsPremiumUser()
{
	return YerdyInstance().isPremiumUser;
}

EXTERN_C bool _Yerdy_IsMessageAvailable(const char *placement)
{
	return [YerdyInstance() isMessageAvailable:CStringToNSString(placement)];
}

EXTERN_C bool _Yerdy_ShowMessage(const char *placement)
{
	return [YerdyInstance() showMessage:CStringToNSString(placement) inWindow:UnityGetMainWindow()];
}

EXTERN_C void _Yerdy_DismissMessage()
{
	[YerdyInstance() dismissMessage];
}

EXTERN_C bool _Yerdy_ShouldShowAnotherMessageAfterUserCancel_get()
{
	return yerdyCallbacks.shouldShowAnotherMessageAfterUserCancel;
}

EXTERN_C void _Yerdy_ShouldShowAnotherMessageAfterUserCancel_set(bool value)
{
	if (!yerdyCallbacks)
		NSLog(@"Please call Yerdy.Init(...) before attemptint to set ShouldShowAnotherMessageAfterUserCancel");
	yerdyCallbacks.shouldShowAnotherMessageAfterUserCancel = value;
}

EXTERN_C void _Yerdy_ConfigureCurrencies(const char *currency0, const char *currency1, const char *currency2,
										 const char *currency3, const char *currency4, const char *currency5)
{
	const char *cStrNames[] = { currency0, currency1, currency2, currency3, currency4, currency5 };
	NSMutableArray *names = [NSMutableArray array];
	
	// convert to NSStrings
	for (size_t i = 0; i < sizeof(cStrNames)/sizeof(const char *); i++) {
		NSString *str = CStringToNSString(cStrNames[i]);
		[names addObject:(str != nil ? str : [NSNull null])];
	}
	
	// trim trailing nulls
	while (names.lastObject == [NSNull null]) {
		[names removeLastObject];
	}
	
	[YerdyInstance() configureCurrencies:names];
}

EXTERN_C void _Yerdy_EarnedCurrencies(const char *currencies)
{
	NSDictionary *currenciesDict = DeserializeDictionary(currencies, NULL, ^id(NSString *str) { return @(str.intValue); } );
	[YerdyInstance() earnedCurrencies:currenciesDict];
}

EXTERN_C void _Yerdy_PurchasedItem(const char *itemName, const char *currencies, bool onSale)
{
	NSDictionary *currenciesDict = DeserializeDictionary(currencies, NULL, ^id(NSString *str) { return @(str.intValue); });
	
	[YerdyInstance() purchasedItem:CStringToNSString(itemName) withCurrencies:currenciesDict onSale:onSale];
}

EXTERN_C void _Yerdy_PurchasedInApp(const char *productIdentifier, const char *transactionIdentifer, const char *currencies, bool onSale)
{
	NSDictionary *currenciesDict = DeserializeDictionary(currencies, NULL, ^id(NSString *str) { return @(str.intValue); });
	
	[yerdyTransactionObserver withProductIdentifier:CStringToNSString(productIdentifier)
							  transactionIdentifier:CStringToNSString(transactionIdentifer)
												 do:^(SKPaymentTransaction *transaction) {
													 YRDPurchase *purchase = [YRDPurchase purchaseWithTransaction:transaction];
													 if (onSale) purchase.onSale = YES;
													 [YerdyInstance() purchasedInApp:purchase currencies:currenciesDict];
												 }];
}

EXTERN_C void _Yerdy_SetUserAsPreYerdy(const char *currencies)
{
	NSDictionary *currenciesDict = DeserializeDictionary(currencies, NULL, ^id(NSString *str) { return @(str.intValue); });
	
	if (currenciesDict.count == 0) {
		YerdyInstance().preYerdyUser = YES;
	} else {
		[YerdyInstance() setExistingCurrenciesForPreYerdyUser:currenciesDict];
	}
}

EXTERN_C void _Yerdy_SetShouldTrackPreYerdyUserProgression(bool shouldTrackPreYerdyUserProgression)
{
	YerdyInstance().shouldTrackPreYerdyUsersProgression = shouldTrackPreYerdyUserProgression;
}

EXTERN_C void _Yerdy_StartPlayerProgression(const char *category, const char *milestone)
{
	[YerdyInstance() startPlayerProgression:CStringToNSString(category)
						   initialMilestone:CStringToNSString(milestone)];
}

EXTERN_C void _Yerdy_LogPlayerProgression(const char *category, const char *milestone)
{
	[YerdyInstance() logPlayerProgression:CStringToNSString(category)
								milestone:CStringToNSString(milestone)];
}

EXTERN_C void _Yerdy_LogFeatureUse(const char *feature)
{
	[YerdyInstance() logFeatureUse:CStringToNSString(feature)];
}

EXTERN_C void _Yerdy_SetFeatureUseLevels(int usesForNovice, int usesForAmateur, int usesForMaster)
{
	[YerdyInstance() setFeatureUsesForNovice:usesForNovice amateur:usesForAmateur master:usesForMaster];
}

EXTERN_C void _Yerdy_SetFeatureUseLevelsForFeature(const char *feature, int usesForNovice, int usesForAmateur, int usesForMaster)
{
	[YerdyInstance() setFeatureUsesForNovice:usesForNovice amateur:usesForAmateur
									  master:usesForMaster forFeature:CStringToNSString(feature)];
}

EXTERN_C void _Yerdy_LogEvent(const char *eventName, const char *parameters)
{
	[YerdyInstance() logEvent:CStringToNSString(eventName)
				   parameters:DeserializeDictionary(parameters)];
}

EXTERN_C void _Yerdy_LogAdRequest(const char *adNetworkName)
{
	[YerdyInstance() logAdRequest:CStringToNSString(adNetworkName)];
}

EXTERN_C void _Yerdy_LogAdFill(const char *adNetworkName)
{
	[YerdyInstance() logAdFill:CStringToNSString(adNetworkName)];
}

