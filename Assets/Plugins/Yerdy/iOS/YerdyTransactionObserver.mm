#import "YerdyTransactionObserver.h"

@interface YerdyTransactionObserverWaitingBlock : NSObject

@property (nonatomic, retain) NSString *productIdentifer;
@property (nonatomic, retain) NSString *transactionIdentifier;
@property (nonatomic, copy) void(^block)(SKPaymentTransaction *);

@end


@interface YerdyTransactionObserver ()
{
	NSMutableArray *_completedTransactions;
	NSMutableArray *_waitingBlocks;
}
@end


@implementation YerdyTransactionObserver

- (id)init
{
	self = [super init];
	if (!self)
		return nil;
	
	_completedTransactions = [[NSMutableArray alloc] init];
	_waitingBlocks = [[NSMutableArray alloc] init];
	
	[[SKPaymentQueue defaultQueue] addTransactionObserver:self];
	
	return self;
}

- (void)dealloc
{
	[_completedTransactions release];
	[_waitingBlocks release];
	
	[[SKPaymentQueue defaultQueue] removeTransactionObserver:self];
	[super dealloc];
}

- (void)processBlocks
{
	int i = 0;
	while (i < _waitingBlocks.count) {
		YerdyTransactionObserverWaitingBlock *block = _waitingBlocks[i];
		SKPaymentTransaction *transaction = [self transactionForWaitingBlock:block];
		if (transaction) {
			if (block.block != NULL) {
				block.block(transaction);
			}
			
			[_waitingBlocks removeObject:block];
			[_completedTransactions removeObject:transaction];
			
			// no 'i++' because we removed 1 item from _waitingBlocks, so everything shifted
			// down 1 and the next item will be at the current 'i'
		} else {
			i++;
		}
	}
}

- (SKPaymentTransaction *)transactionForWaitingBlock:(YerdyTransactionObserverWaitingBlock *)block
{
	for (SKPaymentTransaction *transaction in _completedTransactions) {
		if (block.transactionIdentifier) {
			if ([transaction.transactionIdentifier isEqual:block.transactionIdentifier]) {
				return transaction;
			}
		} else {
			if ([block.productIdentifer isEqual:transaction.payment.productIdentifier]) {
				return transaction;
			}
		}
	}
	
	return nil;
}

- (void)withProductIdentifier:(NSString *)productIdentifier
		transactionIdentifier:(NSString *)transactionIdentifier
						   do:(void (^)(SKPaymentTransaction *))block
{
	YerdyTransactionObserverWaitingBlock *waitingBlock = [[YerdyTransactionObserverWaitingBlock alloc] init];
	waitingBlock.productIdentifer = productIdentifier;
	waitingBlock.transactionIdentifier = transactionIdentifier;
	waitingBlock.block = block;
	[_waitingBlocks addObject:waitingBlock];
	[waitingBlock release];
	
	[self processBlocks];
}

- (void)paymentQueue:(SKPaymentQueue *)queue updatedTransactions:(NSArray *)transactions
{
	for (SKPaymentTransaction *transaction in transactions) {
		if (transaction.transactionState == SKPaymentTransactionStatePurchased) {
			if (![_completedTransactions containsObject:transaction])
				[_completedTransactions addObject:transaction];
		}
	}
	
	[self processBlocks];
}

@end


@implementation YerdyTransactionObserverWaitingBlock

- (void)dealloc
{
	[_productIdentifer release];
	[_transactionIdentifier release];
	[(id)_block release];
	[super dealloc];
}

@end