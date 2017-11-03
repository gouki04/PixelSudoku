using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace sudoku.view
{
    public enum EPuzzleFillMode
    {
        Invalid = 0,
        Normal = 1,
        Candidate = 2,
    }

    public class Puzzle : MonoBehaviour
    {
        public data.Puzzle PuzzleData;

        protected List<data.Puzzle> mSavedPuzzleDataList = new List<data.Puzzle>();

        public asset.Puzzle PuzzleAsset;
        public asset.Skin Skin;

        public GameObject CellPrefab;
        public Cell SelectedCell;

        public GameObject NumberBtnPrefab;
        public GameObject NumberBtnRoot;

        public Toggle FillModeToggle;

        public int BorderW = 3;
        public int MarginW = 1;
        public int OuterBorderW
        {
            get {
                return BorderW + 2;
            }
        }

        protected Cell[,] mCells = null;
        protected NumberBtn[] mNumberBtns = null;

        protected Dictionary<KeyCode, int> mKeyCode2NumberDict = new Dictionary<KeyCode, int>()
        {
            { KeyCode.Alpha1, 1 },
            { KeyCode.Alpha2, 2 },
            { KeyCode.Alpha3, 3 },
            { KeyCode.Alpha4, 4 },
            { KeyCode.Alpha5, 5 },
            { KeyCode.Alpha6, 6 },
            { KeyCode.Alpha7, 7 },
            { KeyCode.Alpha8, 8 },
            { KeyCode.Alpha9, 9 },
            { KeyCode.Keypad1, 1 },
            { KeyCode.Keypad2, 2 },
            { KeyCode.Keypad3, 3 },
            { KeyCode.Keypad4, 4 },
            { KeyCode.Keypad5, 5 },
            { KeyCode.Keypad6, 6 },
            { KeyCode.Keypad7, 7 },
            { KeyCode.Keypad8, 8 },
            { KeyCode.Keypad9, 9 },
        };

        public EPuzzleFillMode mFillMode = EPuzzleFillMode.Normal;

        public Vector2 CellSize
        {
            get {
                var rect_transform = CellPrefab.transform as RectTransform;
                return rect_transform.sizeDelta;
            }
        }

        protected void Start()
        {
            PuzzleData = new data.Puzzle();

            if (PuzzleAsset != null && PuzzleAsset.GivenCells != null) {
                PuzzleData.Init(PuzzleAsset);
                PuzzleData.OnCellChanged += OnCellDataChanged;
                PuzzleData.OnError += OnPuzzleError;
                PuzzleData.OnCandidateChanged += OnCandidateChanged;
            }

            GenerateAllCellView();
            GenerateAllNumberBtn();

            FillModeToggle.onValueChanged.AddListener(delegate(bool v) {
                SetFillMode(v ? EPuzzleFillMode.Candidate : EPuzzleFillMode.Normal);
            });
        }

        protected void Update()
        {
            if (SelectedCell != null) {
                if (Input.GetKeyDown(KeyCode.LeftShift)) {
                    mFillMode = EPuzzleFillMode.Candidate;
                }
                else if (Input.GetKeyUp(KeyCode.LeftShift)) {
                    mFillMode = EPuzzleFillMode.Normal;
                }

                if (Input.GetKeyUp(KeyCode.F)) {
                    PuzzleData.FillAllCandidates();
                }

                if (Input.GetKeyUp(KeyCode.Q)) {
                    SavePuzzle();
                }

                if (Input.GetKeyUp(KeyCode.W)) {
                    LoadPuzzle(0);
                }

                if (Input.GetKeyUp(KeyCode.A)) {
                    //var solver = new data.PuzzleAutoSolver(PuzzleData);
                    //StartCoroutine(solver.Run());

                    var solver = new data.solve.Solver(PuzzleData);
                    StartCoroutine(solver.Run());
                }

                foreach (var kvp in mKeyCode2NumberDict) {
                    if (Input.GetKeyUp(kvp.Key)) {
                        if (mFillMode == EPuzzleFillMode.Candidate) {
                            PuzzleData.TrySetCandidateAt(SelectedCell.Row, SelectedCell.Col, kvp.Value);
                        } else {
                            PuzzleData.TrySetCellAt(SelectedCell.Row, SelectedCell.Col, kvp.Value);
                        }
                    }
                }
            }
        }

        protected void GenerateAllCellView()
        {
            var cell_size = CellSize;
            var box_row_cnt = PuzzleData.BoxRowCnt;
            var box_col_cnt = PuzzleData.BoxColCnt;

            mCells = new Cell[PuzzleData.RowCnt, PuzzleData.ColCnt];

            for (var r = 0; r < PuzzleData.RowCnt; ++r) {
                for (var c = 0; c < PuzzleData.ColCnt; ++c) {
                    var cell_go = GameObject.Instantiate(CellPrefab);
                    cell_go.name = string.Format("Cell {0}-{1}", r, c);

                    var rect_transform = cell_go.transform as RectTransform;
                    rect_transform.SetParent(this.transform);
                    rect_transform.anchoredPosition = new Vector2(
                        OuterBorderW + (MarginW * 2) * c + MarginW + Mathf.FloorToInt(c / box_col_cnt) * BorderW + cell_size.x * c + cell_size.x / 2,
                        -(OuterBorderW + (MarginW * 2) * r + MarginW + Mathf.FloorToInt(r / box_row_cnt) * BorderW + cell_size.y * r + cell_size.y / 2));
                    rect_transform.localRotation = Quaternion.identity;
                    rect_transform.localScale = Vector3.one;

                    var cell_view = cell_go.GetComponent<Cell>();
                    cell_view.Skin = Skin;
                    cell_view.BgImg.sprite = Skin.CellBorder;

                    var cell_number = PuzzleData.GivenCells[r, c];
                    if (cell_number == 0) {
                        cell_view.GivenBgImg.sprite = Skin.CellBg;
                        cell_view.NumberImg.gameObject.SetActive(false);
                    } else {
                        cell_view.GivenBgImg.sprite = Skin.GivenCellBg;
                        cell_view.NumberImg.gameObject.SetActive(true);
                        cell_view.NumberImg.sprite = Skin.CellNumberList[cell_number - 1];
                    }

                    cell_view.Row = r;
                    cell_view.Col = c;
                    cell_view.Button.onClick.AddListener(delegate () { OnCellClick(cell_view); });

                    mCells[r, c] = cell_view;
                }
            }
        }

        protected void GenerateAllNumberBtn()
        {
            var cell_size = (NumberBtnPrefab.transform as RectTransform).sizeDelta;
            var border = 3;
            var margin = 3;

            mNumberBtns = new NumberBtn[PuzzleData.Size];

            for (var i = 0; i < PuzzleData.Size; ++i) {
                var number_btn_go = GameObject.Instantiate(NumberBtnPrefab);
                number_btn_go.name = string.Format("NumberBtn {0}", i);

                var rect_transform = number_btn_go.transform as RectTransform;
                rect_transform.SetParent(NumberBtnRoot.transform);
                rect_transform.anchoredPosition = new Vector2(
                    border + (i > 0 ? margin * i : 0) + cell_size.x * i + cell_size.x / 2,
                    -(border + cell_size.y / 2));
                rect_transform.localRotation = Quaternion.identity;
                rect_transform.localScale = Vector3.one;

                var number_btn_view = number_btn_go.GetComponent<NumberBtn>();
                number_btn_view.Skin = Skin;

                number_btn_view.NumberImg.gameObject.SetActive(true);
                number_btn_view.NumberImg.sprite = Skin.CellNumberList[i];

                number_btn_view.Number = i + 1;
                number_btn_view.Button.onClick.AddListener(delegate () {
                    if (mFillMode == EPuzzleFillMode.Candidate) {
                        PuzzleData.TrySetCandidateAt(SelectedCell.Row, SelectedCell.Col, number_btn_view.Number);
                    } else {
                        PuzzleData.TrySetCellAt(SelectedCell.Row, SelectedCell.Col, number_btn_view.Number);
                    }
                });

                mNumberBtns[i] = number_btn_view;
            }
        }

        protected void SetFillMode(EPuzzleFillMode mode)
        {
            mFillMode = mode;
            var color = mFillMode == EPuzzleFillMode.Candidate ? Color.green : Color.white;
            foreach (var number_btn in mNumberBtns) {
                number_btn.BgImg.color = color;
            }
        }

        protected void ClearAllHighlight()
        {
            foreach (var cell in mCells) {
                cell.Highlight(false);
            }
        }

        protected void HighlightRow(int row)
        {
            for (var c = 0; c < PuzzleData.ColCnt; ++c) {
                mCells[row, c].Highlight(true);
            }
        }

        protected void HighlightColumn(int column)
        {
            for (var r = 0; r < PuzzleData.RowCnt; ++r) {
                mCells[r, column].Highlight(true);
            }
        }

        protected void HighlightBox(int box)
        {
            int row, col;
            PuzzleData.Box2RowCol(box, out row, out col);

            var box_row_cnt = PuzzleData.BoxRowCnt;
            var box_col_cnt = PuzzleData.BoxColCnt;
            for (var r = row; r < row + box_row_cnt; ++r) {
                for (var c = col; c < col + box_col_cnt; ++c) {
                    mCells[r, c].Highlight(true);
                }
            }
        }

        protected void HighlightNumber(int number)
        {
            foreach (var cell in mCells) {
                var cell_number = PuzzleData[cell.Row, cell.Col];
                if (cell_number == number) {
                    cell.Highlight(true);

                    //HighlightRow(cell.Row);
                    //HighlightColumn(cell.Col);
                    //HighlightGrid(PuzzleData.RowCol2Grid(cell.Row, cell.Col));
                }
                else if (cell_number == 0) {
                    var candidates = PuzzleData.Candidates[cell.Row, cell.Col];
                    if (candidates.HasBit(number)) {
                        cell.Highlight(true);
                    }
                }
            }
        }

        protected void SelectCell(Cell cell)
        {
            ClearAllHighlight();

            if (cell != null) {
                var number = PuzzleData[cell.Row, cell.Col];
                if (number == 0) {
                    HighlightRow(cell.Row);
                    HighlightColumn(cell.Col);
                    HighlightBox(PuzzleData.RowCol2Box(cell.Row, cell.Col));
                }
                else {
                    HighlightNumber(number);
                }
            }
            
            SelectedCell = cell;
        }

        protected void OnCellClick(Cell cell)
        {
            if (cell == SelectedCell) {
                SelectCell(null);
            }
            else {
                SelectCell(cell);
            }
        }

        protected void OnCellDataChanged(int row, int col, int old_number, int number)
        {
            var cell_view = mCells[row, col];

            if (number != 0) {
                cell_view.NumberImg.gameObject.SetActive(true);
                cell_view.NumberImg.sprite = Skin.CellNumberList[number - 1];
            }
            else {
                cell_view.NumberImg.gameObject.SetActive(false);
            }

            SelectCell(SelectedCell);
        }

        protected void OnCandidateChanged(int row, int col, data.BitSet32 old_candidates, data.BitSet32 candidates)
        {
            var cell_view = mCells[row, col];

            if (!candidates.IsEmpty) {
                cell_view.CandidatesRoot.SetActive(true);

                for (var digit = 1; digit <= PuzzleData.Size; ++digit) {
                    var candidate_img = cell_view.CandidateImgList[digit - 1];
                    if (candidates.HasBit(digit)) {
                        candidate_img.gameObject.SetActive(true);
                        candidate_img.sprite = Skin.CellNumberList[digit - 1];
                    }
                    else {
                        candidate_img.gameObject.SetActive(false);
                    }
                }
            } else {
                cell_view.CandidatesRoot.SetActive(false);
            }

            SelectCell(SelectedCell);
        }

        protected void OnPuzzleError(int[,] error_cells)
        {
            foreach (var cell in mCells) {
                cell.Error(error_cells != null && error_cells[cell.Row, cell.Col] != 0);
            }
        }

        public void SavePuzzle()
        {
            var clone_puzzle = PuzzleData.Clone() as data.Puzzle;
            if (mSavedPuzzleDataList.Count > 0) {
                mSavedPuzzleDataList[0] = clone_puzzle;
            }
            else {
                mSavedPuzzleDataList.Add(clone_puzzle);
            }
        }

        public void LoadPuzzle(int index)
        {
            if (index >= 0 && index < mSavedPuzzleDataList.Count) {
                PuzzleData = mSavedPuzzleDataList[index].Clone() as data.Puzzle;

                for (var r = 0; r < mCells.GetLength(0); ++r) {
                    for (var c = 0; c < mCells.GetLength(1); ++c) {
                        var cell_given_number = PuzzleData.GivenCells[r, c];
                        var cell_number = PuzzleData[r, c];
                        var cell_view = mCells[r, c];

                        if (cell_given_number == 0) {
                            cell_view.GivenBgImg.sprite = Skin.CellBg;
                        } else {
                            cell_view.GivenBgImg.sprite = Skin.GivenCellBg;
                        }

                        if (cell_number != 0) {
                            cell_view.NumberImg.gameObject.SetActive(true);
                            cell_view.NumberImg.sprite = Skin.CellNumberList[cell_number - 1];
                        }
                        else {
                            cell_view.NumberImg.gameObject.SetActive(false);

                            var candidates = PuzzleData.Candidates[r, c];
                            if (!candidates.IsEmpty) {
                                cell_view.CandidatesRoot.SetActive(true);

                                for (var digit = 1; digit <= PuzzleData.Size; ++digit) {
                                    var candidate_img = cell_view.CandidateImgList[digit - 1];
                                    if (candidates.HasBit(digit)) {
                                        candidate_img.gameObject.SetActive(true);
                                        candidate_img.sprite = Skin.CellNumberList[digit - 1];
                                    } else {
                                        candidate_img.gameObject.SetActive(false);
                                    }
                                }
                            } else {
                                cell_view.CandidatesRoot.SetActive(false);
                            }
                        }

                        SelectCell(null);
                    }
                }
            }
        }
    }
}
