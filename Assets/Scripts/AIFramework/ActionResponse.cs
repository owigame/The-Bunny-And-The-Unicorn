public class ActionResponse : IResponse
{
    private int _lane;
    private Spawnable _spawnable;
    private AI.LogicBase _logicBase;

    public ActionResponse(Spawnable Spawnable,int Lane, AI.LogicBase LogicBase)
    {
        _lane = Lane;
        _spawnable = Spawnable;
        _logicBase = LogicBase;
    }

    Spawnable IResponse.spawnable
    {
        get
        {
            return _spawnable;
        }
        set
        {
            _spawnable = value;
        }
    }

    int IResponse.Lane
    {
        get
        {
            return _lane;
        }
        set
        {
            _lane = value;
        }
    }

    AI.LogicBase IResponse.player
    {
        get
        {
            return _logicBase;
        }
        set
        {
            _logicBase = value;
        }
    }
}