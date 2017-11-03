using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    //public SudokuBoard CurrentBoard;
    //public GameObject UIBoard;
    //public GameObject CellPrefab;
    //public sudoku.view.Skin Skin;

    //protected List<List<GameObject>> mCells = new List<List<GameObject>>();

    //public Vector2 CellSize
    //{
    //    get {
    //        var rect_transform = CellPrefab.transform as RectTransform;
    //        return rect_transform.sizeDelta;
    //    }
    //}

    //public void Start()
    //{
    //    if (CurrentBoard != null && CurrentBoard.OriginBoard != null) {
    //        var board = CurrentBoard.OriginBoard;
    //        for (var r = 0; r < CurrentBoard.RowCnt; ++r) {
    //            for (var c = 0; c < CurrentBoard.ColCnt; ++c) {
    //                var cell = GameObject.Instantiate(CellPrefab);
    //                var rect_transform = cell.transform as RectTransform;
    //                rect_transform.SetParent(UIBoard.transform);
    //                rect_transform.anchoredPosition = new Vector2(CellSize.x * c + CellSize.x / 2, -(CellSize.y * r + CellSize.y / 2));
    //                rect_transform.localRotation = Quaternion.identity;
    //                rect_transform.localScale = Vector3.one;
    //            }
    //        }
    //    }
    //}
}
