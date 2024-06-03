public class Cell
{
    public bool HasShip { get; set; }
    public bool IsHit { get; set; }
    public int ShipId { get; set; } // Идентификатор корабля


    public Cell()
    {
        HasShip = false;
        IsHit = false;
    }
}
