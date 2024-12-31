using UnityEngine;

public class HintsLoader : MonoBehaviour
{
    public GameObject hintSelector;
    public Transform hintsParent;

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
}
