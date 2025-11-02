public interface IPilotAction
{
    EAction PilotAction { get; set; }
    IActionParam ActionParam { get; set; }
}