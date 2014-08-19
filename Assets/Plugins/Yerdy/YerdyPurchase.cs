using UnityEngine;
using System.Collections;

/// <summary>
/// Use the platform specific subclasses below (YerdyPurchaseiOS, YerdyPurchaseGoogle, 
/// and YerdyPurchaseAmazon) when calling the IAP related methods in Yerdy
/// </summary>
public abstract class YerdyPurchase
{
	/// <summary>
	/// Specifies whether or not this IAP was on sale
	/// </summary>
	public bool OnSale { get; set; }
}

/// <summary>
/// iOS specific purchase details
/// </summary>
public class YerdyPurchaseiOS : YerdyPurchase
{
	public string ProductIdentifier { get; private set; }
	public string TransactionIdentifier { get; private set; }
	
	public YerdyPurchaseiOS(string productIdentifier)
	{
		ProductIdentifier = productIdentifier;
	}
	
	public YerdyPurchaseiOS(string productIdentifier, string transactionIdentifier)
	{
		ProductIdentifier = productIdentifier;
		TransactionIdentifier = transactionIdentifier;
	}
}


public class YerdyPurchaseGoogle : YerdyPurchase
{
	public string ProductIdentifier { get; private set; }
	public string Receipt { get; private set; }
	public string ProductValue { get; private set; }
	public string Signature { get; private set; }
	public bool IsSandbox { get; private set; }

	public YerdyPurchaseGoogle(string productIdentifier, string productValue, string receipt, string signature, bool sandbox) {
		ProductIdentifier = productIdentifier;
		ProductValue = productValue;
		Receipt = receipt;
		Signature = signature;
		IsSandbox = sandbox;
	}
}

public class YerdyPurchaseAmazon : YerdyPurchase
{
	public string ProductIdentifier { get; private set; }
	public string Receipt { get; private set; }
	public string ProductValue { get; private set; }
	public string User { get; private set; }
	public bool IsSandbox { get; private set; }

	public YerdyPurchaseAmazon(string productIdentifier, string productValue, string receipt, string user, bool sandbox) {
		ProductIdentifier = productIdentifier;
		ProductValue = productValue;
		Receipt = receipt;
		User = user;
		IsSandbox = sandbox;
	}
}