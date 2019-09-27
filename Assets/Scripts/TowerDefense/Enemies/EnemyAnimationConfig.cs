using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EnemyAnimationConfig : ScriptableObject
{
    [SerializeField] AnimationClip move = default;

    public AnimationClip Move => move;
}
