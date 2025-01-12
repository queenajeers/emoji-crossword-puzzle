using AssetKits.ParticleImage;
using UnityEngine;

public class CoinsGenerator : MonoBehaviour
{
    public static CoinsGenerator Instance { get; private set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform puzzleParent;
    public Transform target;
    public GameObject coinPrefab;

    void Awake()
    {
        Instance = this;
    }

    public void SpawnACoin(Vector2 localPos)
    {
        var coin = Instantiate(coinPrefab, puzzleParent);
        coin.GetComponent<RectTransform>().localPosition = localPos;
        coin.GetComponent<ParticleImage>().attractorTarget = target;
    }

}
