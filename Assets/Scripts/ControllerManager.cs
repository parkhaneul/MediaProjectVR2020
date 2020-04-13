﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ControllerManager : MonoBehaviour
{
    public GameObject testUnit;
    public Dictionary<int, GameObject> unitList;
    
    private List<InputObservableController> controllers;
    
    void OnEnable()
    {
        if(controllers == null)
            controllers = new List<InputObservableController>();

        if(unitList == null)
            unitList = new Dictionary<int, GameObject>();
        
        newController();
        
        foreach (var controller in controllers)
        {
            controller.moveEvent += onMoveEvent;
            controller.actionEvent += onActionEvent;
        }
    }

    private void OnDisable()
    {
        foreach (var controller in controllers)
        {
            controller.moveEvent -= onMoveEvent;
            controller.actionEvent -= onActionEvent;
        }
        controllers = null;
    }

    void newTestUnit(int uid)
    {
        var unit = GameObject.Instantiate(testUnit);
        unit.SetActive(true);
        unitList.Add(uid,unit);
    }

    void newController()
    {
        var uid = 1234;
        var ic = new InputObservableController(uid,this.gameObject);
        newTestUnit(uid);
        addController(ic);
    }

    void addController(InputObservableController ic)
    {
        controllers.Add(ic);
    }

    public void onMoveEvent(object sender, KeyEventArgs<Point> e)
    {
        if (unitList.ContainsKey(e.uid) == false)
        {
            Logger.Log(e.uid + " unit lost");
        }
        else
        {
            GameObject unit;
            unitList.TryGetValue(e.uid,out unit);
            unit.GetComponent<CharacterAction>().move(e.value);
            
            Logger.Log("move");
        }
    }

    public void onActionEvent(object sender, KeyEventArgs<Boolean> e)
    {
        if (unitList.ContainsKey(e.uid) == false)
        {
            Logger.Log(e.uid + " unit lost");
        }
        else
        {
            GameObject unit;
            unitList.TryGetValue(e.uid,out unit);
            unit.GetComponent<CharacterAction>().action(e.value);
            
            Logger.Log("action");
        }
    }
}
