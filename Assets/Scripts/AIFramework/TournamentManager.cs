using UnityEngine;

public class TournamentManager : MonoBehaviour {
    public TickEvent OnTick;
    public static TournamentManager _instance;

    [Header ("Player Setup")]
    public AI.LogicBase P1, P2;

    [Header ("Prefabs")]
    public GameObject unicornPrefab;
    public GameObject bunnyPrefab;

    int playersReady = 0;

    //Singleton management
    private void Awake () {
        if (_instance == null) {
            _instance = this;
        } else {
            Destroy (this);
        }
    }
    private void OnDestroy () {
        _instance = null;
    }

    // Setup the two Ai players 
    private void Start () {
        OnTick = new TickEvent ();
        OnTick.AddListener (P1.OnTick);
        OnTick.AddListener (P2.OnTick);
        P1.init ();
        P2.init ();

        IBoardState data = new LanesNodes ();
        OnTick.Invoke (data);
    }

    private void Update () {
        if (playersReady == 2) { //Max ready players 2
            playersReady = 0;
            
            IBoardState data = new LanesNodes ();
            OnTick.Invoke (data);
        }
    }
    public void OnResponse (IResponse[] ResponseChain) {
        Debug.Log ("--- OnResponse");
        playersReady++;
        foreach (var item in ResponseChain) {
            Debug.Log (item.spawnable + " spawned in lane " + item.Lane);
        }
    }
}