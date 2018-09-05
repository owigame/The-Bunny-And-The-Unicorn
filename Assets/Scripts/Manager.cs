using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Manager : MonoBehaviour {
	public static Tick OnTick;
	public static Manager _Instance;
	public AI_Base P1,P2;
	private void Awake() {
		_Instance = this;
	}
	private void Start() {
		OnTick = new Tick();
		OnTick.AddListener(P1.OnTick);
		OnTick.AddListener(P2.OnTick);
		P1.init();
		P2.init();
	}
	private void Update() {
		IData data = new Data();
		OnTick.Invoke(data);
	}
	public void OnResponse(IResponse[] ResponseChain)
	{

	}
}
public class Data : IData
{
    public Data()
    {
    }
}
public class Tick : UnityEvent<IData>{}

