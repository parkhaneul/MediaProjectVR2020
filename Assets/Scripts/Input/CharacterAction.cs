﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public enum PlayerState
{
    Idle,
    Carry,
    Running,
    UnCarry,
    Smash,
}

public class PlayerStateString
{
    public const string isMove = "IsMove";
    public const string isCarry = "IsCarry";
    public const string isSmash = "IsSmash";
}

public class CharacterAction : MonoBehaviour
{
    public List<Interactable> interactables;
    public Animator animator;
    public float moveSpeed;

    private PlayerState _state;
    
    private PlayerStateMachineObservables _playerStateMachineObservables;

    public void Start()
    {
        _playerStateMachineObservables = animator.GetBehaviour<PlayerStateMachineObservables>();

        _playerStateMachineObservables
            .OnStateEnterObservable
            .Throttle(TimeSpan.FromSeconds(1))
            .Where(x => x.IsName("Base Layer.Smash"))
            .Subscribe(_ =>
            {
                animator.SetBool(PlayerStateString.isSmash, false);
            })
            .AddTo(this);

        Observable.EveryUpdate()
            .SkipUntil(_playerStateMachineObservables.OnStateEnterObservable)
            .Where(_ => animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Running"))
            .Where(_ => Input.anyKey == false)
            .TakeUntil(_playerStateMachineObservables.OnStateExitObservable)
            .Repeat()
            .Subscribe(_ =>
            {
                animator.SetBool(PlayerStateString.isMove,false);
            })
            .AddTo(this);
    }

    public CharacterAction()
    {
        interactables = new List<Interactable>();
    }

    public void move(Point point)
    {
        run();
        turn(point);
        this.gameObject.transform.position += this.gameObject.transform.forward * moveSpeed;
    }

    public void turn(Point point)
    {
        var lookAtVector = this.transform.position + new Vector3(point.x, point.y, point.z);
        this.gameObject.transform.LookAt(lookAtVector);
    }

    public void run()
    {
        _state = PlayerState.Running;
        animator.SetBool(PlayerStateString.isMove,true);
    }

    public void action(bool value)
    {
        foreach(var interactable in interactables)
        {
            interactable.OnDamage();
        }
        animator.SetBool(PlayerStateString.isSmash,true);
    }
}
