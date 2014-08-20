#import "YerdyCallbacks.h"

static const char *GameObjectName = "YerdyCallbacks";

static const char *NSStringToCString(NSString *input)
{
	return input.UTF8String != NULL ? input.UTF8String : "";
}


@implementation YerdyCallbacks

- (void)yerdyConnected
{
	UnitySendMessage(GameObjectName, "_YerdyConnected", "");
}


- (void)yerdy:(Yerdy *)yerdy willPresentMessageForPlacement:(NSString *)placement
{
	UnitySendMessage(GameObjectName, "_WillPresentMessage", NSStringToCString(placement));
}

- (void)yerdy:(Yerdy *)yerdy didPresentMessageForPlacement:(NSString *)placement
{
	UnitySendMessage(GameObjectName, "_DidPresentMessage", NSStringToCString(placement));
}

- (void)yerdy:(Yerdy *)yerdy willDismissMessageForPlacement:(NSString *)placement
{
	UnitySendMessage(GameObjectName, "_WillDismissMessage", NSStringToCString(placement));
}

- (void)yerdy:(Yerdy *)yerdy didDismissMessageForPlacement:(NSString *)placement
{
	UnitySendMessage(GameObjectName, "_DidDismissMessage", NSStringToCString(placement));
}

- (BOOL)yerdy:(Yerdy *)yerdy shouldShowAnotherMessageAfterUserCancelForPlacement:(NSString *)placement
{
	return _shouldShowAnotherMessageAfterUserCancel;
}

- (void)yerdy:(Yerdy *)yerdy handleInAppPurchase:(YRDInAppPurchase *)purchase
{
	UnitySendMessage(GameObjectName, "_HandleInAppPurchase", NSStringToCString(purchase.productIdentifier));
}

- (void)yerdy:(Yerdy *)yerdy handleItemPurchase:(YRDItemPurchase *)purchase
{
	UnitySendMessage(GameObjectName, "_HandleItemPurchase", NSStringToCString(purchase.item));
}

- (void)yerdy:(Yerdy *)yerdy handleReward:(YRDReward *)reward
{
	NSMutableString *rewardsAsString = [NSMutableString string];
	
	for (NSString *name in reward.rewards) {
		NSNumber *amount = reward.rewards[name];
		[rewardsAsString appendFormat:@"%@,%@;", name, amount];
	}
	
	UnitySendMessage(GameObjectName, "_HandleRewards", NSStringToCString(rewardsAsString));
}

- (void)yerdy:(Yerdy *)yerdy handleNavigation:(NSString *)screenName
{
	UnitySendMessage(GameObjectName, "_HandleNavigation", NSStringToCString(screenName));
}

@end