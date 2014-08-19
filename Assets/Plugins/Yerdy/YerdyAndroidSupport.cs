using UnityEngine;
using System.Collections;

public class YerdyAndroidSupport : MonoBehaviour
{
	public System.Action<bool> FocusDelegate = delegate {};

	void Awake()
	{
		gameObject.name = this.GetType().ToString();
		DontDestroyOnLoad(gameObject);
	}

	void OnApplicationFocus(bool focus)
	{
		FocusDelegate(focus);
	}
}

