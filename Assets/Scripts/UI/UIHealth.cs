using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHealth : MonoBehaviour {

	public Image healthBarImage;

	public void SetHealth (float health) {
		healthBarImage.fillAmount = health / 3;
	}
}