using UnityEngine;

public class KeyboardButtonsSpawner : MonoBehaviour
{
    public GameObject keyboardButton;
    public string availableLetters;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnButtons();
    }

    void SpawnButtons()
    {
        for (int i = 0; i < availableLetters.Length; i++)
        {
            var keyboard = Instantiate(keyboardButton, transform);
            keyboard.GetComponent<KeyboardButton>().LoadChar(availableLetters[i]);
        }
    }
}
