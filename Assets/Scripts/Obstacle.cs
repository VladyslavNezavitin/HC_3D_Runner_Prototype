using UnityEngine;



public class Obstacle : MonoBehaviour
{
    public enum ObstacleID
    {
        Car,
        BarrierLow,
        BarrierHigh,
        Container,
        Slope,
        DoubleSlope,
        Spikes,
        IlyshaSpikes
    }

    [SerializeField] private ObstacleID id;
    public int Id => (int)id;

    public void Hit()
    {
        
    }
}
