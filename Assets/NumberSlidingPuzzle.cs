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

    [Header("Solution Helper")]
    [Tooltip("Writes the moves that solve each shuffle to PuzzleSolution.txt (3x3 only).")]
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
    WriteSolution();
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

    if (IsSolved())
    {
    puzzleRunning = false;
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

    // Finds the shortest list of tile clicks that solves the current board
    // and writes it to PuzzleSolution.txt (next to the Assets folder in the editor).
    private void WriteSolution()
    {
    if (gridSize != 3)
    {
    Debug.Log("Puzzle solution file only works for a 3x3 puzzle.");
    return;
    }

    List<int> moves = SolveBoard();

    string text = "Click these tiles in order (" + moves.Count + " moves):\n\n";

    for (int i = 0; i < moves.Count; i++)
    {
    text += (i + 1) + ". " + moves[i] + "\n";
    }

    //string path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PuzzleSolution.txt"));
    //File.WriteAllText(path, text);

    Debug.Log("Puzzle solution: " + string.Join(", ", moves));

    if (openSolutionFile)
    {
    //string url = path.StartsWith("/") ? "file://" + path : "file:///" + path.Replace("\\", "/");
    //Application.OpenURL(url);
    }
    }

    // Breadth-first search over every board layout, so the answer is always the shortest.
    // A board is stored as a 9 digit number, e.g. 123456780 is solved (0 is the blank).
    private List<int> SolveBoard()
    {
    int start = EncodeBoard(boardValues);
    int goal = 123456780;

    Dictionary<int, int> previous = new Dictionary<int, int>();
    Dictionary<int, int> tileMoved = new Dictionary<int, int>();
    Queue<int> queue = new Queue<int>();

    previous[start] = -1;
    queue.Enqueue(start);

    int[] cells = new int[9];

    while (queue.Count > 0)
    {
    int state = queue.Dequeue();

    if (state == goal)
    {
    break;
    }

    DecodeBoard(state, cells);
    int blank = System.Array.IndexOf(cells, 0);

    foreach (int neighbour in GetAdjacentIndexes(blank))
    {
    int tile = cells[neighbour];

    cells[blank] = tile;
    cells[neighbour] = 0;

    int next = EncodeBoard(cells);

    if (!previous.ContainsKey(next))
    {
    previous[next] = state;
    tileMoved[next] = tile;
    queue.Enqueue(next);
    }

    cells[neighbour] = tile;
    cells[blank] = 0;
    }
    }

    List<int> moves = new List<int>();

    if (!previous.ContainsKey(goal))
    {
    return moves;
    }

    for (int state = goal; state != start; state = previous[state])
    {
    moves.Add(tileMoved[state]);
    }

    moves.Reverse();
    return moves;
    }

    private int EncodeBoard(int[] cells)
    {
    int code = 0;

    for (int i = 0; i < cells.Length; i++)
    {
    code = code * 10 + cells[i];
    }

    return code;
    }

    private void DecodeBoard(int code, int[] cells)
    {
    for (int i = cells.Length - 1; i >= 0; i--)
    {
    cells[i] = code % 10;
    code /= 10;
    }
    }
}
