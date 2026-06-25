using System;
using System.Collections;
using UnityEngine;

namespace NXO.Utilities;

public class CoroutineHelper : MonoBehaviour
{
	public static CoroutineHelper Instance { get; private set; }

	public static void InvokeAfterDelay(float time, Action afterDelay)
	{
		if (afterDelay == null)
		{
			UnityEngine.Debug.LogError((object)"Delay called with null action!");
		}
		else
		{
			((MonoBehaviour)Instance).StartCoroutine(InvokeAfterDelayCoroutine(time, afterDelay));
		}
	}

	public static IEnumerator DestroyAfter(GameObject obj, float delay)
	{
		yield return new WaitForSeconds(delay);
		if ((UnityEngine.Object)(object)obj != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)obj);
		}
	}

	private void Awake()
	{
		if ((UnityEngine.Object)(object)Instance != (UnityEngine.Object)null && (UnityEngine.Object)(object)Instance != (UnityEngine.Object)(object)this)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)this);
			return;
		}
		Instance = this;
		UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)(object)((Component)this).gameObject);
	}

	private static IEnumerator InvokeAfterDelayCoroutine(float time, Action afterDelay)
	{
		yield return new WaitForSeconds(time);
		afterDelay();
	}
}
