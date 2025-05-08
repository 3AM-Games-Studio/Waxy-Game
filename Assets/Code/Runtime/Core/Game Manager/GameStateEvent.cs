using Event_Bus;

namespace Core
{
    public class GameStateEvent : IEvent
    {
        public EGameState NewState;
        public EGameState PreviousState;

        public GameStateEvent(EGameState from, EGameState to)
        {
            PreviousState = from;
            NewState = to;
        }
    }
}