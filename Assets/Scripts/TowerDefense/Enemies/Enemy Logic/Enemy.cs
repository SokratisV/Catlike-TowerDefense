﻿using UnityEngine;

public class Enemy : GameBehavior
{
    [SerializeField] EnemyAnimationConfig animationConfig = default;
    EnemyAnimator animator;
    Collider targetPointCollider;
    EnemyFactory originFactory;
    GameTile tileFrom, tileTo;
    Vector3 positionFrom, positionTo;
    float progress, progressFactor, pathOffset, speed;
    Direction direction;
    DirectionChange directionChange;
    float directionAngleFrom, directionAngleTo;
    [SerializeField] Transform model = default;
    public float Scale { get; private set; }
    float Health { get; set; }
    public bool IsValidTarget => animator.CurrentClip == EnemyAnimator.Clip.Move;
    public EnemyFactory OriginFactory
    {
        get => originFactory;
        set
        {
            Debug.Assert(originFactory == null, "Redefined origin factory!");
            originFactory = value;
        }
    }
    public Collider TargetPointCollider
    {
        set
        {
            Debug.Assert(targetPointCollider == null, "Redefined collider!");
            targetPointCollider = value;
        }
    }
    void Awake()
    {
        animator.Configure(
            model.GetChild(0).gameObject.AddComponent<Animator>(),
            animationConfig
        );
    }
    public void Initialize(float scale, float pathOffset, float speed, float health)
    {
        Scale = scale;
        model.localScale = new Vector3(scale, scale, scale);
        this.pathOffset = pathOffset;
        this.speed = speed;
        Health = health;
        animator.PlayIntro();
        targetPointCollider.enabled = false;
    }
    void OnDestroy()
    {
        animator.Destroy();
    }
    public void ApplyDamage(float damage)
    {
        Debug.Assert(damage >= 0f, "Negative damage applied.");
        Health -= damage;
    }
    internal void SpawnOn(GameTile tile)
    {
        Debug.Assert(tile.NextTileOnPath != null, "Nowhere to go!", this);
        tileFrom = tile;
        tileTo = tile.NextTileOnPath;
        progress = 0f;
        PrepareIntro();
    }
    private void PrepareIntro()
    {
        positionFrom = tileFrom.transform.localPosition;
        transform.localPosition = positionFrom;
        positionTo = tileFrom.ExitPoint;
        direction = tileFrom.PathDirection;
        directionChange = DirectionChange.None;
        directionAngleFrom = directionAngleTo = direction.GetAngle();
        model.localPosition = new Vector3(pathOffset, 0f);
        transform.localRotation = direction.GetRotation();
        progressFactor = 2f * speed;
    }
    void PrepareOutro()
    {
        positionTo = tileFrom.transform.localPosition;
        directionChange = DirectionChange.None;
        directionAngleTo = direction.GetAngle();
        model.localPosition = new Vector3(pathOffset, 0f);
        transform.localRotation = direction.GetRotation();
        progressFactor = 2f * speed;
    }
    void PrepareNextState()
    {
        tileFrom = tileTo;
        tileTo = tileTo.NextTileOnPath;
        positionFrom = positionTo;
        if (tileTo == null)
        {
            PrepareOutro();
            return;
        }
        positionTo = tileFrom.ExitPoint;
        directionChange = direction.GetDirectionChangeTo(tileFrom.PathDirection);
        direction = tileFrom.PathDirection;
        directionAngleFrom = directionAngleTo;
        switch (directionChange)
        {
            case DirectionChange.None: PrepareForward(); break;
            case DirectionChange.TurnRight: PrepareTurnRight(); break;
            case DirectionChange.TurnLeft: PrepareTurnLeft(); break;
            default: PrepareTurnAround(); break;
        }
    }
    void PrepareForward()
    {
        transform.localRotation = direction.GetRotation();
        directionAngleTo = direction.GetAngle();
        progressFactor = speed;
        model.localPosition = new Vector3(pathOffset, 0f);
    }
    void PrepareTurnRight()
    {
        directionAngleTo = directionAngleFrom + 90f;
        progressFactor = speed / (Mathf.PI * 0.5f * (0.5f - pathOffset));
        model.localPosition = new Vector3(pathOffset - 0.5f, 0f);
        transform.localPosition = positionFrom + direction.GetHalfVector();
    }
    void PrepareTurnLeft()
    {
        directionAngleTo = directionAngleFrom - 90f;
        progressFactor = speed / (Mathf.PI * 0.5f * (0.5f + pathOffset));
        model.localPosition = new Vector3(pathOffset + 0.5f, 0f);
        transform.localPosition = positionFrom + direction.GetHalfVector();
    }
    void PrepareTurnAround()
    {
        directionAngleTo = directionAngleFrom + (pathOffset < 0f ? 180f : -180f);
        model.localPosition = new Vector3(pathOffset, 0f);
        transform.localPosition = positionFrom;
        progressFactor = speed / (Mathf.PI * Mathf.Max(Mathf.Abs(pathOffset), 0.2f));
    }
    public override bool GameUpdate()
    {
#if UNITY_EDITOR
        if (!animator.IsValid)
        {
            animator.RestoreAfterHotReload(
                model.GetChild(0).GetComponent<Animator>(),
                animationConfig,
                animationConfig.MoveAnimationSpeed * speed / Scale
            );
        }
#endif
        animator.GameUpdate();
        if (animator.CurrentClip == EnemyAnimator.Clip.Intro)
        {
            if (!animator.IsDone)
            {
                return true;
            }
            animator.PlayMove(animationConfig.MoveAnimationSpeed * speed / Scale);
            targetPointCollider.enabled = true;
        }
        else if (animator.CurrentClip >= EnemyAnimator.Clip.Outro)
        {
            if (animator.IsDone)
            {
                Recycle();
                return false;
            }
            return true;
        }
        if (Health <= 0f)
        {
            animator.PlayDying();
            targetPointCollider.enabled = false;
            return true;
        }

        progress += Time.deltaTime * progressFactor;
        while (progress >= 1f)
        {
            if (tileTo == null)
            {
                Game.EnemyReachedDestination();
                animator.PlayOutro();
                targetPointCollider.enabled = false;
                return true;
            }
            progress = (progress - 1f) / progressFactor;
            PrepareNextState();
            progress *= progressFactor;
        }
        if (directionChange == DirectionChange.None)
        {
            transform.localPosition =
                Vector3.LerpUnclamped(positionFrom, positionTo, progress);
        }
        else
        {
            float angle = Mathf.LerpUnclamped(
                directionAngleFrom, directionAngleTo, progress
            );
            transform.localRotation = Quaternion.Euler(0f, angle, 0f);
        }
        return true;
    }
    public override void Recycle()
    {
        animator.Stop();
        OriginFactory.Reclaim(this);
    }
}
