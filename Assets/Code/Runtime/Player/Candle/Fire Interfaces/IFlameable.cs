using Player.Candle;
using UnityEngine;

public interface IFlameable
{
    //public void Interact(PlayerController sender);
    public void Interact(float flameMulti,FlameType flameState);
}