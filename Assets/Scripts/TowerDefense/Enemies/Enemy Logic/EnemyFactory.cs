using UnityEngine;

public enum EnemyType
{
    Small, Medium, Large
}
[CreateAssetMenu]
public class EnemyFactory : GameObjectFactory
{
    [System.Serializable]
    class EnemyConfig
    {
        public Enemy prefab = default;

        [Space]
        [Header("Scale Values")]
        [Range(1f, 2f)]
        public float scale = 1f;
        [Tooltip("Sets upper/lower bounds of random multiplier of scale. Suggested values: 0.8/1.6")]
        public Vector2 scaleRandomness = new Vector2(.7f, 1.4f);

        [Space]
        [Header("Path Offset Values")]
        [Range(1f, 2f)]
        public float pathOffset = 0f;
        [Tooltip("Sets upper/lower bounds of random multiplier of offset value. Suggested values: -0.4/0.4")]
        public Vector2 offSetRandomness = new Vector2(-.4f, .4f);

        [Space]
        [Header("Enemy Speed Values")]
        [Range(1f, 2f)]
        public float speed = 1f;
        [Tooltip("Sets upper/lower bounds of random multiplier of speed. Suggested values: 0.2/5")]
        public Vector2 speedRandomness = new Vector2(.2f, 5f);
        [Space]
        [Header("Enemy Health Values")]
        [Range(10f, 100f)]
        public float health = 100f;
        [Tooltip("Sets upper/lower bounds of random multiplier of speed. Suggested values: 0.2/5")]
        public Vector2 healthRandomness = new Vector2(.2f, 5f);
    }

    [SerializeField]
    EnemyConfig small = default, medium = default, large = default;

    EnemyConfig GetConfig(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Small: return small;
            case EnemyType.Medium: return medium;
            case EnemyType.Large: return large;
        }
        Debug.Assert(false, "Unsupported enemy type!");
        return null;
    }
    public Enemy Get(EnemyType type = EnemyType.Medium)
    {
        EnemyConfig config = GetConfig(type);
        Enemy instance = CreateGameObjectInstance(config.prefab);
        instance.OriginFactory = this;
        instance.Initialize(
            config.scale * Random.Range(config.scaleRandomness.x, config.scaleRandomness.y),
            config.pathOffset * Random.Range(config.offSetRandomness.x, config.offSetRandomness.y),
            config.speed * Random.Range(config.speedRandomness.x, config.speedRandomness.y),
            config.health * Random.Range(config.healthRandomness.x, config.healthRandomness.y));
        return instance;
    }

    public void Reclaim(Enemy enemy)
    {
        Debug.Assert(enemy.OriginFactory == this, "Wrong factory reclaimed!");
        Destroy(enemy.gameObject);
    }
}
