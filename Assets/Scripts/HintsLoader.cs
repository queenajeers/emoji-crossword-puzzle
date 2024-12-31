using UnityEngine;

public class HintsLoader : MonoBehaviour
{
    public static HintsLoader Instance { get; private set; }
    public GameObject hintsSelectorPage;
    public GameObject hintSelectorPrefab;
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
            var hintSelectorComp = Instantiate(hintSelectorPrefab, hintsParent).GetComponent<HintSelector>();
            string spritePath = $"Emojis/{sprite.name}";
            hintSelectorComp.LoadImage(spritePath);
        }
    }

    public void OpenHintsLoader()
    {
        hintsSelectorPage.SetActive(true);
    }
    public void CloseHintsLoader()
    {
        hintsSelectorPage.SetActive(false);
    }
}
