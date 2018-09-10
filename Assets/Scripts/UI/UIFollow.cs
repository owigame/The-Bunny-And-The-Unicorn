using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFollow : MonoBehaviour {
	Transform followTransform;
	RectTransform canvasRect;
	RectTransform rect;
	Camera cam;

	void Start () {
		cam = Camera.main;
	}

	public void Init (Transform _follow, RectTransform _canvas) {
		rect = GetComponent<RectTransform> ();
		followTransform = _follow;
		canvasRect = _canvas;
	}

	void Update () {
		if (followTransform != null) {
			Vector2 viewportPosition = cam.WorldToViewportPoint (followTransform.position);
			rect.anchoredPosition = new Vector2 (((viewportPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f)), ((viewportPosition.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f)));
		}
	}
}