﻿using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public struct EnemyAnimator
{
    public enum Clip { Move, Intro, Outro, Dying, Appear, Disappear }
    bool hasAppearClip, hasDisappearClip;
    PlayableGraph graph;
    AnimationMixerPlayable mixer;
    Clip previousClip;
    float transitionProgress;
    const float transitionSpeed = 5f;
    public Clip CurrentClip { get; private set; }
    public bool IsDone => GetPlayable(CurrentClip).IsDone();
#if UNITY_EDITOR
    public bool IsValid => graph.IsValid();
#endif
#if UNITY_EDITOR
    double clipTime;
#endif
    public void Configure(Animator animator, EnemyAnimationConfig config)
    {
        hasAppearClip = config.Appear;
        hasDisappearClip = config.Disappear;

        graph = PlayableGraph.Create();
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        mixer = AnimationMixerPlayable.Create(
            graph, hasAppearClip || hasDisappearClip ? 6 : 4);

        var clip = AnimationClipPlayable.Create(graph, config.Move);
        clip.Pause();
        mixer.ConnectInput((int)Clip.Move, clip, 0);

        clip = AnimationClipPlayable.Create(graph, config.Intro);
        clip.SetDuration(config.Intro.length);
        mixer.ConnectInput((int)Clip.Intro, clip, 0);

        clip = AnimationClipPlayable.Create(graph, config.Outro);
        clip.SetDuration(config.Outro.length);
        clip.Pause();
        mixer.ConnectInput((int)Clip.Outro, clip, 0);

        clip = AnimationClipPlayable.Create(graph, config.Dying);
        clip.SetDuration(config.Dying.length);
        clip.Pause();
        mixer.ConnectInput((int)Clip.Dying, clip, 0);

        if (hasAppearClip)
        {
            clip = AnimationClipPlayable.Create(graph, config.Appear);
            clip.SetDuration(config.Appear.length);
            clip.Pause();
            mixer.ConnectInput((int)Clip.Appear, clip, 0);
        }

        if (hasDisappearClip)
        {
            clip = AnimationClipPlayable.Create(graph, config.Disappear);
            clip.SetDuration(config.Disappear.length);
            clip.Pause();
            mixer.ConnectInput((int)Clip.Disappear, clip, 0);
        }

        var output = AnimationPlayableOutput.Create(graph, "Enemy", animator);
        output.SetSourcePlayable(mixer);
    }
    void BeginTransition(Clip nextClip)
    {
        previousClip = CurrentClip;
        CurrentClip = nextClip;
        transitionProgress = 0f;
        GetPlayable(nextClip).Play();
    }
    public void PlayIntro()
    {
        SetWeight(Clip.Intro, 1f);
        CurrentClip = Clip.Intro;
        graph.Play();
        transitionProgress = -1f;

        if (hasAppearClip)
        {
            GetPlayable(Clip.Appear).Play();
            SetWeight(Clip.Appear, 1f);
        }
    }
    void SetWeight(Clip clip, float weight)
    {
        mixer.SetInputWeight((int)clip, weight);
    }
    public void PlayMove(float speed)
    {
        GetPlayable(Clip.Move).SetSpeed(speed);
        BeginTransition(Clip.Move);
        if (hasAppearClip)
        {
            SetWeight(Clip.Appear, 0f);
        }
        SetWeight(CurrentClip, 0f);
        SetWeight(Clip.Move, 1f);
        var clip = GetPlayable(Clip.Move);
        clip.SetSpeed(speed);
        clip.Play();
        CurrentClip = Clip.Move;

    }
    Playable GetPlayable(Clip clip)
    {
        return mixer.GetInput((int)clip);
    }
    public void PlayOutro()
    {
        BeginTransition(Clip.Outro);
        if (hasDisappearClip)
        {
            PlayDisappearFor(Clip.Outro);
        }
        SetWeight(CurrentClip, 0f);
        SetWeight(Clip.Outro, 1f);
        GetPlayable(Clip.Outro).Play();
        CurrentClip = Clip.Outro;
    }
    public void PlayDying()
    {
        BeginTransition(Clip.Dying);
        if (hasDisappearClip)
        {
            PlayDisappearFor(Clip.Dying);
        }
    }
    public void Stop() { graph.Stop(); }
    public void Destroy()
    {
        graph.Destroy();
    }
    public void GameUpdate()
    {
        if (transitionProgress >= 0f)
        {
            transitionProgress += Time.deltaTime * transitionSpeed;
            if (transitionProgress >= 1f)
            {
                transitionProgress = -1f;
                SetWeight(CurrentClip, 1f);
                SetWeight(previousClip, 0f);
            }
            else
            {
                SetWeight(CurrentClip, transitionProgress);
                SetWeight(previousClip, 1f - transitionProgress);
            }
        }
#if UNITY_EDITOR
        clipTime = GetPlayable(CurrentClip).GetTime();
#endif
    }

    [System.Obsolete]
    void PlayDisappearFor(Clip otherClip)
    {
        var clip = GetPlayable(Clip.Disappear);
        clip.Play();
        clip.SetDelay(GetPlayable(otherClip).GetDuration() - clip.GetDuration());
        SetWeight(Clip.Disappear, 1f);
    }
#if UNITY_EDITOR
    public void RestoreAfterHotReload(
        Animator animator, EnemyAnimationConfig config, float speed
    )
    {
        Configure(animator, config);
        GetPlayable(Clip.Move).SetSpeed(speed);
        SetWeight(CurrentClip, 1f);
        var clip = GetPlayable(CurrentClip);
        clip.SetTime(clipTime);
        clip.Play();
        graph.Play();
        if (CurrentClip == Clip.Intro && hasAppearClip)
        {
            clip = GetPlayable(Clip.Appear);
            clip.SetTime(clipTime);
            clip.Play();
            SetWeight(Clip.Appear, 1f);
        }
        else if (CurrentClip >= Clip.Outro && hasDisappearClip)
        {
            clip = GetPlayable(Clip.Disappear);
            clip.Play();
            double delay =
                GetPlayable(CurrentClip).GetDuration() -
                clip.GetDuration() -
                clipTime;
            if (delay >= 0f)
            {
                clip.SetDelay(delay);
            }
            else
            {
                clip.SetTime(-delay);
            }
            SetWeight(Clip.Disappear, 1f);
        }
    }

#endif
}
