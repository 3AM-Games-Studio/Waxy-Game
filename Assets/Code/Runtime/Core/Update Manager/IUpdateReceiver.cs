namespace Core
{
    public interface IUpdateReceiver
    {
        void OnUpdate();
        void OnFixedUpdate();
        void OnLateUpdate();
    }
}