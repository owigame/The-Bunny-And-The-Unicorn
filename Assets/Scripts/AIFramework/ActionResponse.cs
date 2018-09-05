public class ActionResponse : IResponse
{
    private int _lane;
    private Spawnable _spawnable;

    public ActionResponse(Spawnable Spawnable,int Lane)
    {
        _lane = Lane;
        _spawnable = Spawnable;
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
}