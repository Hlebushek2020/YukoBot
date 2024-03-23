namespace YukoClientBase.Interfaces
{
    public delegate void FullscreenEventHandler();

    public interface IFullscreenEvent
    {
        event FullscreenEventHandler FullscreenEvent;
    }
}