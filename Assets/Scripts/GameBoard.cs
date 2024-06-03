using System.Collections.Generic;
using UnityEngine;

public class GameBoard
{
    private Cell[,] board;
    public int rows = 7;
    public int columns = 7;
    private int remainingShips;
    private Dictionary<int, List<Vector2Int>> ships; // Хранение всех ячеек для каждого корабля
    private int currentShipId = 0;

    public GameBoard()
    {
        board = new Cell[rows, columns];
        ships = new Dictionary<int, List<Vector2Int>>();
        InitializeBoard();
        remainingShips = 0; // Инициализируем количество оставшихся кораблей
    }

    void InitializeBoard()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                board[i, j] = new Cell();
            }
        }
    }

    public bool PlaceShip(int row, int col)
    {
        if (board[row, col] == null)
        {
            board[row, col] = new Cell();
        }

        if (board[row, col].HasShip)
        {
            return false;
        }

        board[row, col].HasShip = true;
        board[row, col].ShipId = currentShipId;

        if (!ships.ContainsKey(currentShipId))
        {
            ships[currentShipId] = new List<Vector2Int>();
        }
        ships[currentShipId].Add(new Vector2Int(row, col));

        return true;
    }

    public void FinalizeShipPlacement()
    {
        currentShipId++;
        remainingShips++; // Увеличиваем количество оставшихся кораблей при завершении размещения
    }

    public bool Attack(int row, int col)
    {
        if (board[row, col] == null)
        {
            board[row, col] = new Cell();
        }

        if (board[row, col].IsHit)
        {
            return false;
        }

        board[row, col].IsHit = true;
        if (board[row, col].HasShip)
        {
            // Проверим, уничтожен ли весь корабль
            if (IsShipDestroyed(row, col))
            {
                remainingShips--; // Уменьшаем количество оставшихся кораблей при уничтожении всего корабля
                Debug.Log("Корабль уничтожен! Оставшиеся корабли: " + remainingShips);
            }
            return true;
        }
        return false;
    }

    public List<Vector2Int> GetShipCells(int row, int col)
    {
        if (!board[row, col].HasShip)
        {
            return null;
        }

        int shipId = board[row, col].ShipId;
        List<Vector2Int> shipCells = new List<Vector2Int>();
        foreach (var cell in ships[shipId])
        {
            shipCells.Add(cell);
        }

        return shipCells;
    }

    public bool IsShipDestroyed(int row, int col)
    {
        if (!board[row, col].HasShip)
        {
            return false;
        }

        int shipId = board[row, col].ShipId;
        foreach (var cell in ships[shipId])
        {
            if (!board[cell.x, cell.y].IsHit)
            {
                return false;
            }
        }

        return true;
    }

    public bool AllShipsDestroyed()
    {
        return remainingShips <= 0; // Проверяем, уничтожены ли все корабли
    }

    public Cell GetCell(int row, int col)
    {
        return board[row, col];
    }
}
