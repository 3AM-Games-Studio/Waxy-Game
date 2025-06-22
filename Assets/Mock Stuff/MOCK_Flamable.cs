using System;
using UnityEngine;
using UnityEngine.Events;

public class MOCK_Flamable : MonoBehaviour, IFlameable
{
    private void Awake()
    {
        gameObject.layer = 7;
    }

    public UnityEvent OnFlameOnCollide;
    public void Interact(float flameMulti, FlameType flameState)
    {
        if(flameState == FlameType.Off)return;
        OnFlameOnCollide.Invoke();
    }
}