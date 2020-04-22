﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GameSystem : MonoBehaviour
{
    public float LimitTime = 0f;
    
    private GameSystem _instance;
    public GameSystem Instance
    {
        get
        {
            if (_instance == null)
                _instance = this;
            return _instance;
        }
    }

    public List<GameLogic> logics = new List<GameLogic>();

    public void Start()
    {
        Logger.Log("start");
        
        var tl = TimeLogic.Instance;
        var ml = MissionLogic.Instance;
        
        tl.setTime(LimitTime);
        
        logics.Add(tl);
        logics.Add(ml);
        
        activeAll();
    }

    public void Update()
    {
        runAll();
    }

    public void runAll()
    {
        foreach (var logic in logics)
        {
            logic.run();
        }
    }

    public void stopAll()
    {
        foreach (var logic in logics)
        {
            logic.stop();
        }
    }

    public void activeAll()
    {
        foreach (var logic in logics)
        {
            logic.active();
        }
    }
}