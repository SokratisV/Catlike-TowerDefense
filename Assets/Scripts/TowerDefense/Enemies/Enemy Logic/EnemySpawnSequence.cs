﻿using UnityEngine;

[System.Serializable]
public class EnemySpawnSequence
{
    [SerializeField]
    EnemyFactory factory = default;
    [SerializeField]
    EnemyType type = EnemyType.Medium;
    [SerializeField, Range(1, 100)]
    int amount = 1;
    [SerializeField, Range(0.1f, 10f)]
    float cooldown = 1f;
    public State Begin() => new State(this);
    [System.Serializable]
    public struct State
    {
        int count;
        float cooldown;
        EnemySpawnSequence sequence;
        public State(EnemySpawnSequence sequence)
        {
            this.sequence = sequence;
            count = 0;
            cooldown = sequence.cooldown;
        }
        public float Progress(float deltaTime)
        {
            cooldown += deltaTime;
            while (cooldown >= sequence.cooldown)
            {
                cooldown -= sequence.cooldown;
                // Game.Instance.UpdateText(count,
                // sequence.cooldown * sequence.amount - sequence.cooldown * count);
                if (count >= sequence.amount)
                {
                    return cooldown;
                }
                count += 1;
                Game.SpawnEnemy(sequence.factory, sequence.type);
            }
            return -1f;
        }
    }
}