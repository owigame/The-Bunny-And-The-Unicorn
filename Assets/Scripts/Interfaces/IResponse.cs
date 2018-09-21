public interface IResponse
{
    CreatureBase creature { get; set; }
    int Lane { get; set; }
    AI.LogicBase player { get; set;}
    ResponseActionType responseActionType { get; set; }
    LaneNode laneNode { get; set; }
}
