using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour {
	// Transform of the camera to shake. Grabs the gameObject's transform
	// if null.
	public Transform camTransform;

	// How long the object should shake for.
	public float shakeDuration = 0f;
	float _duration;

	// Amplitude of the shake. A larger value shakes the camera harder.
	public float shakeAmount = 0.7f;
	float _shake;
	public float decreaseFactor = 1.0f;

	Vector3 originalPos;

	public static CameraShake _CameraShake;

	void Awake () {
		_CameraShake = this;
		if (camTransform == null) {
			camTransform = transform;
		}
		originalPos = camTransform.localPosition;
	}

	public void DoCameraShake (float duration, float multiplier) {
		_shake = multiplier * shakeAmount;
		if (duration == 0) duration = shakeDuration;
		_duration = duration;
	}

	void Update () {
		if (_duration > 0) {
			Vector3 newShake = (new Vector3 (0, Random.Range (-1f, 1f) * shakeAmount, Random.Range (2f, 5f) * _shake) /* Random.insideUnitSphere */ );
			// Debug.Log ("SHAKE: " + newShake);
			camTransform.localPosition = originalPos + newShake;

			_duration -= Time.deltaTime * decreaseFactor;
		} else {
			_duration = 0f;
			camTransform.localPosition = originalPos;
		}
	}
}