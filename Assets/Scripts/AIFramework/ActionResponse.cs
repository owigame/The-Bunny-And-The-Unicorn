public class ActionResponse : IResponse
{
    private int _lane;
    private CreatureBase _creatureBase;
    private AI.LogicBase _logicBase;
    private ResponseActionType _responseActionType;

    public ActionResponse(CreatureBase Creature,int Lane, AI.LogicBase LogicBase, ResponseActionType ResponseActionType)
    {
        _lane = Lane;
        _creatureBase = Creature;
        _logicBase = LogicBase;
        _responseActionType = ResponseActionType;
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

    CreatureBase IResponse.creature
    {
        get
        {
            return _creatureBase;
        }

        set
        {
            _creatureBase = value;
        }
    }

    ResponseActionType IResponse.responseActionType {
        get
        {
            return _responseActionType;
        }
        set
        {
            _responseActionType = value;
        }
    }
}