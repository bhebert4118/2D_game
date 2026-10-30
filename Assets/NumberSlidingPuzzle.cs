using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 3x3 "8 puzzle". Builds its own tiles from one template button,
// so you don't have to place 8 buttons and 9 slots by hand.
public class NumberSlidingPuzzle : MonoBehaviour
{
    [Header("Puzzle Layout")]
    [Tooltip("The area the tiles are laid out in (your PuzzleGrid).")]
    public RectTransform board;

    [Tooltip("One Button with a TMP_Text child. It gets copied for every tile and then hidden.")]
    public Button tileTemplate;

    public int gridSize = 3;
    public float spacing = 10f;

    [Header("Puzzle Settings")]
    public int shuffleMoves = 30;
    public float tileSlideSpeed = 2500f;
    public float delayBeforeClosing = 0.75f;

    [Header("Sound")]
    public AudioClip winSound;
    public float winVolume = 1f;

    [Header("Solution Helper")]
    [Tooltip("Writes the moves that solve the puzzle to PuzzleSolution.txt. Kept up to date as you play.")]
    public bool writeSolutionFile = true;
    [Tooltip("Also opens that text file in a separate window (Notepad etc.) when the puzzle starts.")]
    public bool openSolutionFile = true;

    private int[] boardValues;
    private int blankIndex;
    private Button[] tiles;
    private Vector2[] tileTargets;
    private float cellSize;

    private PartReturn activePartReturn;
    private bool puzzleRunning = false;
    private List<int> moveHistory = new List<int>();

    private void Awake()
    {
    BuildTiles();
    }

    public void BeginPuzzle(PartReturn partReturn)
    {
    activePartReturn = partReturn;
    puzzleRunning = true;

    CreateSolvedBoard();
    ShuffleBoard();
    UpdateVisuals(true);

    if (writeSolutionFile)
    {
    WriteSolution(openSolutionFile);
    }
    }

    private void BuildTiles()
    {
    if (board == null || tileTemplate == null)
    {
    Debug.LogError("NumberSlidingPuzzle needs a Board and a Tile Template assigned.");
    return;
    }

    // A Grid Layout Group on the board would fight the tile positions.
    LayoutGroup layout = board.GetComponent<LayoutGroup>();

    if (layout != null)
    {
    layout.enabled = false;
    }

    int tileCount = gridSize * gridSize - 1;

    boardValues = new int[gridSize * gridSize];
    tiles = new Button[tileCount];
    tileTargets = new Vector2[tileCount];

    for (int i = 0; i < tileCount; i++)
    {
    int tileValue = i + 1;

    Button tile = Instantiate(tileTemplate, board);
    tile.name = "Tile " + tileValue;
    tile.gameObject.SetActive(true);

    RectTransform tileRect = tile.GetComponent<RectTransform>();
    tileRect.anchorMin = new Vector2(0.5f, 0.5f);
    tileRect.anchorMax = new Vector2(0.5f, 0.5f);
    tileRect.pivot = new Vector2(0.5f, 0.5f);

    TMP_Text tileText = tile.GetComponentInChildren<TMP_Text>(true);

    if (tileText != null)
    {
    tileText.text = tileValue.ToString();
    }

    tile.onClick.RemoveAllListeners();
    tile.onClick.AddListener(() => TryMoveTile(tileValue));

    tiles[i] = tile;
    }

    tileTemplate.gameObject.SetActive(false);
    }

    private void Update()
    {
    if (tiles == null)
    {
    return;
    }

    // Slide each tile toward where it belongs. Unscaled so it still works if you pause time.
    for (int i = 0; i < tiles.Length; i++)
    {
    RectTransform tileRect = (RectTransform)tiles[i].transform;

    tileRect.anchoredPosition = Vector2.MoveTowards(
        tileRect.anchoredPosition,
        tileTargets[i],
        tileSlideSpeed * Time.unscaledDeltaTime
    );
    }
    }

    private void CreateSolvedBoard()
    {
    for (int i = 0; i < boardValues.Length - 1; i++)
    {
    boardValues[i] = i + 1;
    }

    blankIndex = boardValues.Length - 1;
    boardValues[blankIndex] = 0;

    moveHistory.Clear();
    }

    // Shuffles by making random legal moves from the solved state,
    // so the puzzle is always solvable.
    private void ShuffleBoard()
    {
    int previousBlankIndex = -1;

    for (int i = 0; i < shuffleMoves; i++)
    {
    int[] possibleMoves = GetAdjacentIndexes(blankIndex);
    int selectedMove;

    do
    {
    selectedMove = possibleMoves[Random.Range(0, possibleMoves.Length)];
    }
    while (selectedMove == previousBlankIndex);

    previousBlankIndex = blankIndex;
    MoveIntoBlank(selectedMove);
    }

    if (IsSolved())
    {
    MoveIntoBlank(GetAdjacentIndexes(blankIndex)[0]);
    }
    }

    private void TryMoveTile(int tileValue)
    {
    if (!puzzleRunning)
    {
    return;
    }

    int tileIndex = System.Array.IndexOf(boardValues, tileValue);

    int tileRow = tileIndex / gridSize;
    int tileColumn = tileIndex % gridSize;
    int blankRow = blankIndex / gridSize;
    int blankColumn = blankIndex % gridSize;

    // Like the real toy: clicking any tile in the blank's row or column
    // slides that whole line of tiles toward the gap.
    if (tileRow == blankRow)
    {
    int step = tileColumn > blankColumn ? 1 : -1;

    while (blankIndex != tileIndex)
    {
    MoveIntoBlank(blankIndex + step);
    }
    }
    else if (tileColumn == blankColumn)
    {
    int step = tileRow > blankRow ? gridSize : -gridSize;

    while (blankIndex != tileIndex)
    {
    MoveIntoBlank(blankIndex + step);
    }
    }
    else
    {
    return;
    }

    UpdateVisuals(false);

    if (writeSolutionFile)
    {
    WriteSolution(false);
    }

    if (IsSolved())
    {
    puzzleRunning = false;
    Sfx.Play(winSound, winVolume);
    StartCoroutine(FinishAfterDelay());
    }
    }

    private IEnumerator FinishAfterDelay()
    {
    yield return new WaitForSecondsRealtime(delayBeforeClosing);

    if (activePartReturn != null)
    {
    activePartReturn.CompletePuzzle();
    }

    activePartReturn = null;
    }

    // Moves the tile at fromIndex into the blank square.
    private void MoveIntoBlank(int fromIndex)
    {
    RecordMove(boardValues[fromIndex]);

    boardValues[blankIndex] = boardValues[fromIndex];
    boardValues[fromIndex] = 0;
    blankIndex = fromIndex;
    }

    private void UpdateVisuals(bool snap)
    {
    Rect area = board.rect;
    float boardSize = Mathf.Min(area.width, area.height);

    cellSize = (boardSize - spacing * (gridSize - 1)) / gridSize;
    float centerOffset = (gridSize - 1) / 2f;

    for (int position = 0; position < boardValues.Length; position++)
    {
    int tileValue = boardValues[position];

    if (tileValue == 0)
    {
    continue;
    }

    int row = position / gridSize;
    int column = position % gridSize;

    Vector2 target = new Vector2(
        (column - centerOffset) * (cellSize + spacing),
        (centerOffset - row) * (cellSize + spacing)
    );

    int tileNumber = tileValue - 1;
    RectTransform tileRect = (RectTransform)tiles[tileNumber].transform;

    tileRect.sizeDelta = new Vector2(cellSize, cellSize);
    tileTargets[tileNumber] = target;

    if (snap)
    {
    tileRect.anchoredPosition = target;
    }
    }
    }

    private bool IsSolved()
    {
    for (int i = 0; i < boardValues.Length - 1; i++)
    {
    if (boardValues[i] != i + 1)
    {
    return false;
    }
    }

    return true;
    }

    private int[] GetAdjacentIndexes(int index)
    {
    int row = index / gridSize;
    int column = index % gridSize;

    List<int> result = new List<int>();

    if (row > 0) result.Add(index - gridSize);
    if (row < gridSize - 1) result.Add(index + gridSize);
    if (column > 0) result.Add(index - 1);
    if (column < gridSize - 1) result.Add(index + 1);

    return result.ToArray();
    }

    // ---------- Solution helper ----------

    // The solution is just the shuffle played backwards. Every move (shuffle or player)
    // goes on this list; moving the same tile twice in a row undoes it, so those cancel out.
    private void RecordMove(int tileValue)
    {
    int last = moveHistory.Count - 1;

    if (last >= 0 && moveHistory[last] == tileValue)
    {
    moveHistory.RemoveAt(last);
    }
    else
    {
    moveHistory.Add(tileValue);
    }
    }

    // Writes the tiles to click (newest move first) to PuzzleSolution.txt,
    // which is next to the Assets folder in the editor.
    private void WriteSolution(bool openFile)
    {
    List<int> moves = new List<int>(moveHistory);
    moves.Reverse();

    string text = "Click these tiles in order (" + moves.Count + " moves):\n\n";

    for (int i = 0; i < moves.Count; i++)
    {
    text += (i + 1) + ". " + moves[i] + "\n";
    }

    string path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PuzzleSolution.txt"));
    File.WriteAllText(path, text);

    if (openFile)
    {
    Debug.Log("Puzzle solution: " + string.Join(", ", moves) + "\nSaved to " + path);

    string url = path.StartsWith("/") ? "file://" + path : "file:///" + path.Replace("\\", "/");
    Application.OpenURL(url);
    }
    }
}
