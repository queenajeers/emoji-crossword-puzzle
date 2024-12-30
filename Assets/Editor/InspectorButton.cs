using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PuzzleCreator))]
public class InspectorButton : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default Inspector
        DrawDefaultInspector();

        // Add a button
        PuzzleCreator puzzleCreator = (PuzzleCreator)target;
        if (GUILayout.Button("Save Puzzle"))
        {
            puzzleCreator.SavePuzzle();
        }
    }
}
