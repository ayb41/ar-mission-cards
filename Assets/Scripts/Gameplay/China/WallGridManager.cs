using System.Collections.Generic;
using UnityEngine;

public class WallGridManager : MonoBehaviour
{
    public Transform buildRoot;

    [Header("Wall Grid Size")]
    public int columns = 6;
    public int rows = 4;

    [Header("Slot Spacing")]
    public float cellWidth = 0.085f;
    public float cellHeight = 0.04f;

    [Header("Wall Position Local")]
    public float wallZ = 0.08f;
    public float wallBaseY = 0.025f;

    [Header("Wall Area Local Z")]
    public float wallMinZ = -0.02f;
    public float wallMaxZ = 0.20f;

    private Transform[,] occupiedSlots;
    private readonly Dictionary<Transform, Vector2Int> brickSlots = new Dictionary<Transform, Vector2Int>();

    private void Awake()
    {
        EnsureGrid();
    }

    private void EnsureGrid()
    {
        if (occupiedSlots == null || occupiedSlots.GetLength(0) != columns || occupiedSlots.GetLength(1) != rows)
        {
            occupiedSlots = new Transform[columns, rows];
            brickSlots.Clear();
        }
    }

    public bool IsInWallArea(Vector3 localPoint)
    {
        return localPoint.z >= wallMinZ && localPoint.z <= wallMaxZ;
    }

    public float GetSlotLocalY(int row)
    {
        row = Mathf.Clamp(row, 0, rows - 1);
        return wallBaseY + row * cellHeight;
    }

    public int GetBrickRow(Transform brick)
    {
        EnsureGrid();

        if (brick != null && brickSlots.TryGetValue(brick, out Vector2Int slot))
        {
            return slot.y;
        }

        return -1;
    }

    public void ReleaseBrickSlot(Transform brick)
    {
        EnsureGrid();

        if (brick == null) return;

        if (brickSlots.TryGetValue(brick, out Vector2Int slot))
        {
            if (slot.x >= 0 && slot.x < columns && slot.y >= 0 && slot.y < rows)
            {
                occupiedSlots[slot.x, slot.y] = null;
            }

            brickSlots.Remove(brick);
        }
    }

    public bool TryPlaceBrick(Transform brick, Vector3 localPoint, int row, out Vector3 worldPosition)
    {
        EnsureGrid();

        worldPosition = Vector3.zero;

        if (brick == null || buildRoot == null)
        {
            return false;
        }

        row = Mathf.Clamp(row, 0, rows - 1);

        int desiredColumn = GetColumnFromLocalX(localPoint.x);

        ReleaseBrickSlot(brick);

        int column = FindNearestFreeColumn(desiredColumn, row);

        if (column == -1)
        {
            return false;
        }

        occupiedSlots[column, row] = brick;
        brickSlots[brick] = new Vector2Int(column, row);

        Vector3 localSlotPosition = GetSlotLocalPosition(column, row);
        worldPosition = buildRoot.TransformPoint(localSlotPosition);

        return true;
    }

    private int GetColumnFromLocalX(float localX)
    {
        float startX = -((columns - 1) * cellWidth) * 0.5f;
        int column = Mathf.RoundToInt((localX - startX) / cellWidth);
        return Mathf.Clamp(column, 0, columns - 1);
    }

    private int FindNearestFreeColumn(int desiredColumn, int row)
    {
        if (occupiedSlots[desiredColumn, row] == null)
        {
            return desiredColumn;
        }

        for (int offset = 1; offset < columns; offset++)
        {
            int left = desiredColumn - offset;
            int right = desiredColumn + offset;

            if (left >= 0 && occupiedSlots[left, row] == null)
            {
                return left;
            }

            if (right < columns && occupiedSlots[right, row] == null)
            {
                return right;
            }
        }

        return -1;
    }

    private Vector3 GetSlotLocalPosition(int column, int row)
    {
        float startX = -((columns - 1) * cellWidth) * 0.5f;

        float x = startX + column * cellWidth;
        float y = wallBaseY + row * cellHeight;
        float z = wallZ;

        return new Vector3(x, y, z);
    }

    public int GetPlacedBrickCount()
    {
        EnsureGrid();
        return brickSlots.Count;
    }

    public int GetRequiredBrickCount()
    {
        return columns * rows;
    }

    public bool IsWallComplete()
    {
        EnsureGrid();

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                if (occupiedSlots[x, y] == null)
                {
                    return false;
                }
            }
        }

        return true;
    }
    public void ClearGrid()
    {
        EnsureGrid();

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                occupiedSlots[x, y] = null;
            }
        }

        brickSlots.Clear();
    }

}