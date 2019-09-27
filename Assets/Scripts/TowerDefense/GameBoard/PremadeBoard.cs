using UnityEngine;

[CreateAssetMenu]
public class PremadeBoard : ScriptableObject
{
    // 0 - Empty, 1 - Destination, 2 - Wall, 3 - SpawnPoint, 4 - LaserTower, 5 - Mortar
    public int[] boardTiles;
}
