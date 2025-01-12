using UnityEngine;

public class Coin : MonoBehaviour
{
    public void OnCoinReached()
    {
        UIManager.Instance.OnCoinReached();
    }
}
