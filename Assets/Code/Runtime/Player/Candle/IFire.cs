namespace Player.Candle
{
    public interface IFire
    {
        public void TurnFireOn();
        public void TurnFireOff();
        public bool IsFireOn { get; }
    }
}
