using UnityEngine;

public class WallEvaluator : MonoBehaviour
{
    public WallGridManager wallGridManager;

    public bool EvaluateWall()
    {
        if (wallGridManager == null)
        {
            Debug.LogWarning("WallEvaluator: WallGridManager eksik!");
            return false;
        }

        int placed = wallGridManager.GetPlacedBrickCount();
        int required = wallGridManager.GetRequiredBrickCount();

        Debug.Log("Placed bricks: " + placed + " / " + required);

        return wallGridManager.IsWallComplete();
    }
}