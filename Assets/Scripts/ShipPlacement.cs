using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

public class ShipPlacement : MonoBehaviour
{
    public enum GameState { PlacingShips, Battle }
    public GameState currentState = GameState.PlacingShips;

    public GameObject[] shipPrefabs; // ������� ��������
    public Sprite[] shipIcons; // ������ ��������
    private int currentShipIndex = -1; // ������ �������� ������� (-1 ��������, ��� ������ �� �������)
    private bool isPlayer1Turn = true; // ����������� ������� ������
    private GameObject[,] grid;
    public Text turnIndicator; // ����� ��� ��������� ����
    public Text gameStateIndicator; // ����� ��� ��������� ��������� ����
    public GameObject transitionPanel; // ������ ��� ��������� ����� ����
    public GameObject shipPanel; // ������ ��� ����������� ��������
    public Text playerNameText; // ����� ��� ����������� ����� ������
    public GameObject shipIconPrefab; // ������ ������ ������� � �����������
    private GameBoard player1Board;
    private GameBoard player2Board;
    private GameBoard currentBoard;
    public Color32 battleColor = new Color32(0, 0, 255, 255); // ����� ���� ��� ������ �����
    public GameObject destroyedShipPrefab; // ������ ��� ����������� ������������� �������
    public Transform destroyedShipsPanel;

    // ���������� �������� ������� ����
    private int[] shipCounts = { 3, 2, 1 }; // 1 ������, 2 ������, 3 ������
    private int[] currentShipPlacement = { 0, 0, 0 }; // ������� ���������� ����������� �������� ������� ����

    private GameObject[] shipIconsUI;
    private List<Vector2Int> selectedCells = new List<Vector2Int>(); // ������ ��������� ����� ��� �������� �������

    // ���� ��� ������ ������
    public GameObject victoryPanel; // ������ ������
    public Text victoryText; // ����� ��� ����������� ����������
    public Button exitButton; // ������ ������
    public Button restartButton; // ������ ������ ������

    void Start()
    {
        grid = FindObjectOfType<GridManager>().GetGrid();
        player1Board = new GameBoard();
        player2Board = new GameBoard();
        currentBoard = player1Board;
        InitializeShipIcons();
        UpdateTurnIndicator();
        UpdateGameStateIndicator();
        transitionPanel.SetActive(false); // ��������� ���������� ������ ��� ������

        // ��������� ������ ������ ��� ������
        victoryPanel.SetActive(false);

        // ��������� ������ ��� ������
        exitButton.onClick.AddListener(ExitGame);
        restartButton.onClick.AddListener(RestartGame);
    }

    void InitializeShipIcons()
    {
        if (shipPrefabs.Length != shipIcons.Length)
        {
            Debug.LogError("���������� �������� �������� �� ��������� � ����������� ������.");
            return;
        }

        shipIconsUI = new GameObject[shipPrefabs.Length];
        for (int i = 0; i < shipPrefabs.Length; i++)
        {
            GameObject icon = Instantiate(shipIconPrefab, shipPanel.transform);
            icon.transform.Find("ShipIcon").GetComponent<Image>().sprite = shipIcons[i];
            icon.transform.Find("ShipCount").GetComponent<Text>().text = shipCounts[i].ToString();
            int shipType = i;
            AddEventTrigger(icon, () => SelectShip(shipType));
            shipIconsUI[i] = icon;
        }
    }

    void AddEventTrigger(GameObject obj, UnityEngine.Events.UnityAction action)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = obj.AddComponent<EventTrigger>();
        }
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener((eventData) => { action(); });
        trigger.triggers.Add(entry);
    }

    void UpdateShipIcons()
    {
        for (int i = 0; i < shipPrefabs.Length; i++)
        {
            int remaining = shipCounts[i] - currentShipPlacement[i];
            shipIconsUI[i].transform.Find("ShipCount").GetComponent<Text>().text = remaining.ToString();
            shipIconsUI[i].SetActive(remaining > 0);
        }
    }

    void SelectShip(int shipType)
    {
        currentShipIndex = shipType;
        selectedCells.Clear();
        Debug.Log($"������ ������� ���� {shipType + 1}");
    }

    void ClearSelection()
    {
        foreach (var cell in selectedCells)
        {
            grid[cell.x, cell.y].GetComponent<Image>().color = Color.white; // ������� �����
        }
        selectedCells.Clear();
    }

    void HighlightCells(List<Vector2Int> cells)
    {
        foreach (var cell in cells)
        {
            grid[cell.x, cell.y].GetComponent<Image>().color = Color.yellow; // ������������ ������
        }
    }

    void ConfirmShipPlacement()
    {
        if (selectedCells.Count == (currentShipIndex + 1))
        {
            bool isValidPlacement = true;

            foreach (var cell in selectedCells)
            {
                if (!IsPlacementValid(cell))
                {
                    isValidPlacement = false;
                    break;
                }
            }

            if (isValidPlacement)
            {
                foreach (var cell in selectedCells)
                {
                    // ������ Instantiate �������, ��������� ������ �� Image ���������� ������
                    Image cellImage = grid[cell.x, cell.y].GetComponent<Image>();
                    cellImage.sprite = shipIcons[currentShipIndex]; // shipIcons - ������ �������� ��� ������� ���� ��������

                    // ������������� ��������� ������� (��������, �������, ���� ����������)
                    cellImage.SetNativeSize(); // ������������� ������ ������� � ������������ � ��� ������������� ���������

                    currentBoard.PlaceShip(cell.x, cell.y);
                }
                currentBoard.FinalizeShipPlacement(); // ���������� ���������� �������� �������
                currentShipPlacement[currentShipIndex]++;
                currentShipIndex = -1;
                ClearSelection();
                UpdateShipIcons();
                Debug.Log($"������� ��������");
            }
            else
            {
                Debug.Log("������� �� ����� ���� ��������� �������� ���� � �����.");
            }
        }
        else
        {
            Debug.Log("������� ������������ ���������� ����� ��� �������.");
        }
    }




    bool IsPlacementValid(Vector2Int cell)
    {
        int[] dx = { -1, 0, 1, 0, -1, -1, 1, 1 };
        int[] dy = { 0, 1, 0, -1, -1, 1, -1, 1 };

        for (int i = 0; i < dx.Length; i++)
        {
            int nx = cell.x + dx[i];
            int ny = cell.y + dy[i];

            if (nx >= 0 && nx < grid.GetLength(0) && ny >= 0 && ny < grid.GetLength(1))
            {
                if (currentBoard.GetCell(nx, ny).HasShip)
                {
                    return false;
                }
            }
        }
        return true;
    }



    public void PlaceShip(int row, int col)
    {
        if (currentState == GameState.PlacingShips)
        {
            if (currentShipIndex == -1)
            {
                Debug.Log("�������� ��� �������");
                return;
            }

            Vector2Int selectedCell = new Vector2Int(row, col);
            if (selectedCells.Contains(selectedCell))
            {
                ConfirmShipPlacement();
            }
            else
            {
                if (selectedCells.Count < (currentShipIndex + 1))
                {
                    if (selectedCells.Count == 0 || IsNeighbor(selectedCell))
                    {
                        if (IsPlacementValid(selectedCell))
                        {
                            selectedCells.Add(selectedCell);
                            HighlightCells(selectedCells);
                            Debug.Log($"������ ��������� � �����: ({row}, {col})");
                        }
                        else
                        {
                            StartCoroutine(HighlightInvalidCell(selectedCell));
                            Debug.Log("���������� ���������� ������� �������� � ������� �������.");
                        }
                    }
                    else
                    {
                        StartCoroutine(HighlightInvalidCell(selectedCell));
                        Debug.Log("��������� ������ ������ ���� ���������.");
                    }
                }
                else
                {
                    Debug.Log("������� ������������ ���������� ����� ��� �������� �������.");
                }
            }
        }
    }


    IEnumerator HighlightInvalidCell(Vector2Int cell)
    {
        Image cellImage = grid[cell.x, cell.y].GetComponent<Image>();
        Color originalColor = cellImage.color;
        cellImage.color = Color.red;
        yield return new WaitForSeconds(1);
        if (!selectedCells.Contains(cell))
        {
            cellImage.color = originalColor;
        }
    }



    bool IsNeighbor(Vector2Int cell)
    {
        foreach (var selectedCell in selectedCells)
        {
            if (Mathf.Abs(selectedCell.x - cell.x) + Mathf.Abs(selectedCell.y - cell.y) == 1)
            {
                return true;
            }
        }
        return false;
    }

    public void EndTurn()
    {
        if (currentState == GameState.PlacingShips)
        {
            // ��������, ��� �� ������� �����������
            if (!AllShipsPlaced())
            {
                Debug.Log("�� ��� ������� �����������");
                ShowMessage("�� ��� ������� �����������");
                return;
            }

            if (isPlayer1Turn)
            {
                isPlayer1Turn = false;
                currentBoard = player2Board;
                ResetBoard();
                currentShipPlacement = new int[] { 0, 0, 0 }; // ����� �������� ���������� �������� ��� ������ 2
                UpdateShipIcons();
                ShowTransitionPanel("����� 2");
                Invoke("StartNextTurn", 2); // ���� 2 ������� � �������� ��������� ���
            }
            else
            {
                currentState = GameState.Battle;
                isPlayer1Turn = true; // ������������, ��� ������ ��� � ��� ����� �� ������� 1
                currentBoard = player2Board;
                UpdateGameStateIndicator();
                ChangeGridColor(battleColor);
                ResetBoard(); // ������� ���� ����� ������� ���
                HideAllShips(); // �������� ��� �������
                ShowTransitionPanel("���������� �����");
                Invoke("StartNextTurn", 2); // ���� 2 ������� � �������� �����
            }
        }
    }

    void HideAllShips()
    {
        foreach (var cell in grid)
        {
            foreach (Transform child in cell.transform)
            {
                if (child.gameObject.CompareTag("Ship"))
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }


    void ChangeGridColor(Color32 newColor)
    {
        foreach (var cell in grid)
        {
            cell.GetComponent<Image>().color = newColor;
        }
    }

    bool AllShipsPlaced()
    {
        for (int i = 0; i < shipCounts.Length; i++)
        {
            if (currentShipPlacement[i] < shipCounts[i])
            {
                return false;
            }
        }
        return true;
    }

    public void OnCellClicked(int row, int col)
    {
        if (currentState == GameState.Battle)
        {
            GameBoard opponentBoard = isPlayer1Turn ? player2Board : player1Board;

            if (!opponentBoard.GetCell(row, col).IsHit)
            {
                bool hit = opponentBoard.Attack(row, col);
                UpdateCellState(row, col, hit);
            }
        }
        else
        {
            PlaceShip(row, col);
        }
    }




    void UpdateCellState(int row, int col, bool hit)
    {
        GameObject cell = grid[row, col];
        Image cellImage = cell.GetComponent<Image>();
        cell.GetComponent<Button>().interactable = false;

        Sprite grayCross = Resources.Load<Sprite>("Sprites/GrayCross");
        Sprite redCross = Resources.Load<Sprite>("Sprites/RedCross");

        if (grayCross == null || redCross == null)
        {
            Debug.LogError("�� ������� ��������� ������� ���������. ��������� ���� � ������� �������� � ����� Resources/Sprites.");
            return;
        }

        if (hit)
        {
            cellImage.sprite = grayCross; // ������ �� ����� �������
            cellImage.color = Color.white;

            GameBoard opponentBoard = isPlayer1Turn ? player2Board : player1Board;

            if (opponentBoard.IsShipDestroyed(row, col))
            {
                List<Vector2Int> shipCells = opponentBoard.GetShipCells(row, col);
                foreach (var shipCell in shipCells)
                {
                    GameObject shipCellObj = grid[shipCell.x, shipCell.y];
                    Image shipCellImage = shipCellObj.GetComponent<Image>();
                    shipCellImage.sprite = redCross; // ������ �� ������� �������
                    shipCellImage.color = Color.white;
                    shipCellObj.GetComponent<Button>().interactable = false;
                }

                if (opponentBoard.AllShipsDestroyed())
                {
                    ShowVictoryPanel(isPlayer1Turn ? 1 : 2);
                    return;
                }
            }
        }
        else
        {
            cellImage.color = Color.gray;
            SwitchTurn(); // ������������ ���� ��� �������
        }
    }

    void ShowVictoryPanel(int playerNumber)
    {
        victoryPanel.SetActive(true);
        victoryText.text = "������� ����� " + playerNumber;
    }
    void SwitchTurn()
    {
        isPlayer1Turn = !isPlayer1Turn;
        currentBoard = isPlayer1Turn ? player1Board : player2Board;
        Debug.Log(isPlayer1Turn ? "������ ����� ����� 1" : "������ ����� ����� 2");
        UpdateGrid();
        ShowTransitionPanel(isPlayer1Turn ? "��� ������ 1" : "��� ������ 2");
        Invoke("StartNextTurn", 2); // ���� 2 ������� � �������� ��������� ���
    }

    void UpdateGrid()
    {
        GameBoard opponentBoard = isPlayer1Turn ? player2Board : player1Board;
        for (int i = 0; i < opponentBoard.rows; i++)
        {
            for (int j = 0; j < opponentBoard.columns; j++)
            {
                GameObject cell = grid[i, j];
                Image cellImage = cell.GetComponent<Image>();
                Cell boardCell = opponentBoard.GetCell(i, j);

                if (boardCell.IsHit)
                {
                    cellImage.color = Color.white;
                    cellImage.sprite = boardCell.HasShip ? Resources.Load<Sprite>("Sprites/GrayCross") : null;
                    if (boardCell.HasShip && opponentBoard.IsShipDestroyed(i, j))
                    {
                        cellImage.sprite = Resources.Load<Sprite>("Sprites/RedCross");
                    }
                }
                else
                {
                    cellImage.color = new Color32(76, 176, 176, 255);
                    cellImage.sprite = null;
                    cell.GetComponent<Button>().interactable = true; // ������� ������ ����� ������������
                }
            }
        }
    }

    void ShowMessage(string message)
    {
        // ����������� ��������� ������
        // ���������� ����� ���� ����� UI Text ������� �� ������
        Debug.Log(message);
    }

    void ResetBoard()
    {
        foreach (var cell in grid)
        {
            Image cellImage = cell.GetComponent<Image>();
            cellImage.color = new Color32(76, 176, 176, 255); // ���������� ���� ������
            cellImage.sprite = null; // ������� ������, ���� �� ��� ����������

            Button cellButton = cell.GetComponent<Button>();
            cellButton.interactable = true; // ������ ������ ������������

            foreach (Transform child in cell.transform)
            {
                Destroy(child.gameObject); // ���������� ��� �������� ������� � ������
            }
        }
    }



    private int GetShipType()
    {
        for (int i = 0; i < shipCounts.Length; i++)
        {
            if (currentShipPlacement[i] < shipCounts[i])
            {
                return i;
            }
        }
        return -1; // ��� ������� ���������
    }

    private int GetTotalShipCount()
    {
        int total = 0;
        foreach (int count in shipCounts)
        {
            total += count;
        }
        return total;
    }

    public GameBoard GetCurrentBoard()
    {
        return currentBoard;
    }

    void ShowTransitionPanel(string message)
    {
        transitionPanel.SetActive(true); // �������� ���������� ������
                                                   // �������������� ����� �� ������, ���� �����
        Text panelText = transitionPanel.GetComponentInChildren<Text>();
        if (panelText != null)
        {
            panelText.text = message;
        }
    }


    void StartNextTurn()
    {
        transitionPanel.SetActive(false); // ��������� ���������� ������
        turnIndicator.gameObject.SetActive(true);// �������� ����� ���� ������
        transitionPanel.SetActive(false); // ��������� ���������� ������
        Debug.Log("������ �����: " + turnIndicator.text);
        turnIndicator.text = isPlayer1Turn ? "��� ������ 1" : "��� ������ 2";
        UpdateTurnIndicator(); // ��������� ��������� ����
        UpdateGameStateIndicator(); // ��������� ��������� ����
    }


    void UpdateTurnIndicator()
    {
        turnIndicator.text = isPlayer1Turn ? "��� ������ 1" : "��� ������ 2";
        playerNameText.text = isPlayer1Turn ? "��� ������ 1" : "��� ������ 2";
    }

    void UpdateGameStateIndicator()
    {
        gameStateIndicator.text = currentState == GameState.PlacingShips ? "����������� ��������" : "���";
    }

    void ExitGame()
    {
        // ����� �� ���� (��� ��������� Unity)
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void RestartGame()
    {
        // ������������ ������� �����
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
