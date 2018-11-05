[System.Serializable]
public class ActionResponse : IResponse {
    private int _lane;
    private CreatureBase _creatureBase;
    private AI.LogicBase _logicBase;
    private ResponseActionType _responseActionType;
    private LaneNode _laneNode;

    public ActionResponse (CreatureBase Creature, int Lane, AI.LogicBase LogicBase, ResponseActionType ResponseActionType, LaneNode LaneNode) {
        _lane = Lane;
        _creatureBase = Creature;
        _logicBase = LogicBase;
        _responseActionType = ResponseActionType;
        _laneNode = LaneNode;
    }

    int IResponse.Lane {
        get {
            return _lane;
        }
        set {
            _lane = value;
        }
    }

    AI.LogicBase IResponse.player {
        get {
            return _logicBase;
        }
        set {
            _logicBase = value;
        }
    }

    CreatureBase IResponse.creature {
        get {
            return _creatureBase;
        }

        set {
            _creatureBase = value;
        }
    }

    ResponseActionType IResponse.responseActionType {
        get {
            return _responseActionType;
        }
        set {
            _responseActionType = value;
        }
    }

    LaneNode IResponse.laneNode {
        get {
            return _laneNode;
        }
        set {
            _laneNode = value;
        }
    }
}