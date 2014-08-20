#import "Yerdy.h"


@interface YerdyCallbacks : NSObject <YerdyDelegate, YerdyMessageDelegate>
@property (nonatomic, assign) BOOL shouldShowAnotherMessageAfterUserCancel;
@end
