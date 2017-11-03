using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace sudoku.editor
{
    [CustomEditor(typeof(asset.Puzzle))]
    public class PuzzleAssetInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var puzzle = (asset.Puzzle)target;

            puzzle.Size = EditorGUILayout.IntField("Size", puzzle.Size);
            puzzle.PuzzleString = GUILayout.TextArea(puzzle.PuzzleString, GUILayout.Width(300));

            if (GUILayout.Button("Save")) {
                EditorUtility.SetDirty(puzzle);
            }
        }
    }
}