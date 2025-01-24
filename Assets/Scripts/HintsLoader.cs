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

    public void LoadHintImages(string levelName)
    {
        if (!spritesLoaded)
        {
            spritesLoaded = true;
            foreach (var sprite in Resources.LoadAll<Sprite>($"{levelName}"))
            {
                var hintSelectorComp = Instantiate(hintSelectorPrefab, hintsParent).GetComponent<HintSelector>();
                string spritePath = $"{levelName}/{sprite.name}";
                hintSelectorComp.LoadImage(spritePath);
            }
        }
    }

    bool spritesLoaded = false;



    public void OpenHintsLoader()
    {
        hintsSelectorPage.SetActive(true);
    }
    public void CloseHintsLoader()
    {
        hintsSelectorPage.SetActive(false);
    }
}
