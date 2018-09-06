using UnityEngine;

public class TournamentManager : MonoBehaviour {
    public TickEvent OnTick;
    public static TournamentManager _instance;

    [Header("Player Setup")]
    public AI.LogicBase P1, P2;

    [Header("Prefabs")]
    public GameObject unicornPrefab;
    public GameObject bunnyPrefab;


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
    }

    private void Update () {
        IBoardState data = new LanesNodes ();
        OnTick.Invoke (data);
    }
    public void OnResponse (IResponse[] ResponseChain) {

    }
}