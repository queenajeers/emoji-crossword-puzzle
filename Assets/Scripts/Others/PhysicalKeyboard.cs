using System;
using UnityEngine;

public class PhysicalKeyboard : MonoBehaviour
{
    public static Action<char> LetterTyped;
    public static Action BackspaceTyped;


    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
        {
            for (KeyCode key = KeyCode.A; key <= KeyCode.Z; key++)
            {
                if (Input.GetKeyDown(key))
                {
                    char letter = key.ToString()[0];
                    LetterTyped?.Invoke(letter);
                    return;
                }

            }

            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                BackspaceTyped?.Invoke();
            }

        }
    }
}
