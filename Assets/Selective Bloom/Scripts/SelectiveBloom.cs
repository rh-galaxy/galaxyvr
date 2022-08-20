using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script designed to be attached to the objects that the user wants to see with glow.
/// </summary>
public class SelectiveBloom : MonoBehaviour
{
	public Color Color = Color.red;
	[Range(0f, 1f)]
	public float Strength = 1.0f;
	private Color _lastColor;
	private float _lastStrength;

	void OnStart()
	{
		OnEnable();
	}

	void Update()
	{
		/* Check if any parameter changed since the last update and force a "re-add" to the bloom engine so the change can be visualized */
		if(Mathf.Abs(_lastStrength - Strength) > 0.005f || _lastColor != Color)
		{
			OnDisable();
			OnEnable();
			_lastStrength = Strength;
			_lastColor = Color;
		}
	}

	void OnEnable()
	{
		BloomEngine.Add(this);
	}

	void OnDisable()
	{
		BloomEngine.Remove(this);
	}

	void OnDestroy()
	{
		BloomEngine.Remove(this);
	}
}
