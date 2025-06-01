using UnityEngine;

namespace Player.Candle.Stats
{
    
    [CreateAssetMenu(fileName = "NewStats", menuName = "Player/Stats/CandleStats")]
    public class CandleStats : ScriptableObject
    {
        public float WaxAmount = 120;
        public float StartingWax = 120;
        public float ChangeFlameMulti = 1;
        public FlameType StartingFlame = FlameType.Off;
        public float ConsumingWaxOnNormal = 1f;
        public float ConsumingWaxOnIncrease = 2f;
        // public float SuffocationTime = 10;
        // public float ChangeSuffocationMulti = 1;
    }
}
