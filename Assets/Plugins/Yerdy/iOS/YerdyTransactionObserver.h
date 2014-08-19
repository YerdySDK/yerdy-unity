#import <StoreKit/StoreKit.h>

// Watches SKPaymentQueue for completed transactions so that the user doesn't need
// to pass in all the transaction properties from the Unity side (since their IAP
// plugin may not provide those parameters)

@interface YerdyTransactionObserver : NSObject <SKPaymentTransactionObserver>

// Runs block with the corresponding productIdentifier/transactionIdentifer when the
// transaction comes in (or immediately if it already has)
- (void)withProductIdentifier:(NSString *)productIdentifier
		transactionIdentifier:(NSString *)transactionIdentifier // optional
						   do:(void(^)(SKPaymentTransaction *))block;

@end
