using UnityEngine;

[CreateAssetMenu]
public class EnemyFactory : GameObjectFactory
{

    [SerializeField]
    Enemy prefab = default;
    [SerializeField, Range(1f, 2f)]
    float scale = 1f;
    [SerializeField]
    [Tooltip("Sets upper/lower bounds of random multiplier of scale. Suggested values: .8/1.6")]
    Vector2 randomness = default;
    public Enemy Get()
    {
        Enemy instance = CreateGameObjectInstance(prefab);
        instance.OriginFactory = this;
        instance.Initialize(scale * Random.Range(randomness.x, randomness.y));
        return instance;
    }

    public void Reclaim(Enemy enemy)
    {
        Debug.Assert(enemy.OriginFactory == this, "Wrong factory reclaimed!");
        Destroy(enemy.gameObject);
    }
}
