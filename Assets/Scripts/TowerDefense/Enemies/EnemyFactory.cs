using UnityEngine;

[CreateAssetMenu]
public class EnemyFactory : GameObjectFactory
{
    [SerializeField]
    Enemy prefab = default;
    [Header("Right Click to apply suggested values")]
    [ContextMenuItem("Apply Suggested Values", "ApplySuggestedValues")]
    public bool RightClick;

    [Space]
    [Header("Scale Values")]
    [SerializeField, Range(1f, 2f)]
    float scale = 1f;
    [SerializeField]
    [Tooltip("Sets upper/lower bounds of random multiplier of scale. Suggested values: 0.8/1.6")]
    Vector2 scaleRandomness = new Vector2(.7f, 1.4f);

    [Space]
    [Header("Path Offset Values")]
    [SerializeField, Range(1f, 2f)]
    float pathOffset = 0f;
    [SerializeField]
    [Tooltip("Sets upper/lower bounds of random multiplier of offset value. Suggested values: -0.4/0.4")]
    Vector2 offSetRandomness = new Vector2(-.4f, .4f);

    [Space]
    [Header("Enemy Speed Values")]
    [SerializeField, Range(1f, 2f)]
    float speed = 1f;
    [SerializeField]
    [Tooltip("Sets upper/lower bounds of random multiplier of speed. Suggested values: 0.2/5")]
    Vector2 speedRandomness = new Vector2(.2f, 5f);



    public Enemy Get()
    {
        Enemy instance = CreateGameObjectInstance(prefab);
        instance.OriginFactory = this;
        instance.Initialize(
            scale * Random.Range(scaleRandomness.x, scaleRandomness.y),
            pathOffset * Random.Range(offSetRandomness.x, offSetRandomness.y),
            speed * Random.Range(speedRandomness.x, speedRandomness.y));
        return instance;
    }

    public void Reclaim(Enemy enemy)
    {
        Debug.Assert(enemy.OriginFactory == this, "Wrong factory reclaimed!");
        Destroy(enemy.gameObject);
    }

    private void ApplySuggestedValues()
    {
        scale = 1f;
        scaleRandomness.x = .7f;
        scaleRandomness.y = 1.4f;
        pathOffset = 0f;
        offSetRandomness.x = -.4f;
        offSetRandomness.y = .4f;
        speed = 1f;
        speedRandomness.x = .2f;
        speedRandomness.y = 5f;
    }
}
