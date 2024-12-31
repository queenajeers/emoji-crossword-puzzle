using UnityEngine;

public class HintsLoader : MonoBehaviour
{
    public static HintsLoader Instance { get; private set; }
    public GameObject hintSelector;
    public Transform hintsParent;

    void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        LoadHintImages();
    }

    public void LoadHintImages()
    {
        foreach (var sprite in Resources.LoadAll<Sprite>("Emojis"))
        {
            var hintSelectorComp = Instantiate(hintSelector, hintsParent).GetComponent<HintSelector>();
            string spritePath = $"Emojis/{sprite.name}";
            hintSelectorComp.LoadImage(spritePath);
        }
    }

    public void OpenHintsLoader()
    {

    }
}
