using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    public int rows = 7; // Поле 7x7
    public int columns = 7;
    public float cellSize = 100f; // Размер ячеек
    public float spacing = 5f; // Расстояние между ячейками
    public GameObject buttonPrefab;
    public RectTransform gridPanel; // Панель для размещения кнопок
    private GameObject[,] grid;

    void Start()
    {
        grid = new GameObject[rows, columns];
        GenerateGrid();
        ResizePanel();
    }

    void GenerateGrid()
    {
        Color32 customColor = new Color32(76, 176, 176, 255); // Определение кастомного цвета

        GridLayoutGroup gridLayoutGroup = gridPanel.GetComponent<GridLayoutGroup>();
        gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);
        gridLayoutGroup.spacing = new Vector2(spacing, spacing);
        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayoutGroup.constraintCount = columns;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                GameObject newButton = Instantiate(buttonPrefab, gridPanel);
                RectTransform rectTransform = newButton.GetComponent<RectTransform>();
                rectTransform.localScale = Vector3.one;

                newButton.name = $"Cell_{i}_{j}";
                grid[i, j] = newButton;

                int row = i;
                int col = j;
                newButton.GetComponent<Button>().onClick.AddListener(() => FindObjectOfType<ShipPlacement>().OnCellClicked(row, col));
                newButton.GetComponent<Image>().color = customColor; // Устанавливаем кастомный цвет ячейки
            }
        }
    }


    public GameObject[,] GetGrid()
    {
        return grid;
    }

    void ResizePanel()
    {
        float panelWidth = (columns * (cellSize + spacing)) - spacing;
        float panelHeight = (rows * (cellSize + spacing)) - spacing;
        gridPanel.sizeDelta = new Vector2(panelWidth, panelHeight);
        gridPanel.anchoredPosition = Vector2.zero;
    }

    void OnCellClicked(int row, int col)
    {
        ShipPlacement shipPlacement = FindObjectOfType<ShipPlacement>();
        if (shipPlacement.currentState == ShipPlacement.GameState.PlacingShips)
        {
            shipPlacement.PlaceShip(row, col);
        }
        else if (shipPlacement.currentState == ShipPlacement.GameState.Battle)
        {
            GameBoard currentBoard = shipPlacement.GetCurrentBoard();
            bool hit = currentBoard.Attack(row, col);
            if (hit)
            {
                grid[row, col].GetComponent<Image>().color = Color.red; // Попадание
            }
            else
            {
                grid[row, col].GetComponent<Image>().color = Color.blue; // Промах
            }
        }
    }
}
