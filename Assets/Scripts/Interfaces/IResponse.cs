public interface IResponse
{
    Spawnable spawnable { get; set; }
    int Lane { get; set; }
    AI.LogicBase player { get; set;}
}
