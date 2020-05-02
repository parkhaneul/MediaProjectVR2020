﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UniRx;
using UniRx.Triggers;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using Random = System.Random;

public class ReadInputManager
{
    private static string[] axisNameArray;
    public static string[] ReadAxes()
    {
        if (axisNameArray != null)
            return axisNameArray;
        
        var inputManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];
        
        SerializedObject obj = new SerializedObject(inputManager);
        SerializedProperty axisArray = obj.FindProperty("m_Axes");
        
        if(axisArray.arraySize == 0)
            Logger.Log("No Axes");

        axisNameArray = new string[axisArray.arraySize];
        for( int i = 0; i < axisArray.arraySize; ++i )
        {
            var axis = axisArray.GetArrayElementAtIndex(i);
            var name = axis.FindPropertyRelative("m_Name").stringValue;
            axisNameArray[i] = name;
        }

        return axisNameArray;
    }    
}

public class ControllerManager : MonoBehaviour
{
    public GameObject testUnit;
    public Dictionary<int, GameObject> unitList;

    private Camera _camera;
    private List<InputObservableController> controllers;
    public float playerModelScale = 1;
    
    void Awake()
    {
        if(controllers == null)
            controllers = new List<InputObservableController>();

        if(unitList == null)
            unitList = new Dictionary<int, GameObject>();
        
        if(_camera == null)
            _camera = Camera.main;
    }

    private void Start()
    {
        newController(1234);
        
        ReadInputManager.ReadAxes();
    }

    private void OnDisable()
    {
        Destroy(gameObject);
        controllers = null;
    }

    void newTestUnit(int uid)
    {
        var go = GameObject.Instantiate(testUnit);
        go.gameObject.transform.localScale *= playerModelScale;
        go.SetActive(true);
        UIManager.Instance.addUser(uid);

        this.UpdateAsObservable()
            .Select(_ => _camera.WorldToViewportPoint(go.transform.position))
            .Where(_ => (_.x < 1 && _.x > 0 && _.y < 1 && _.y > 0) == false)
            .DistinctUntilChanged(_ => _)
            .Subscribe(_ =>
            {
                UIManager.Instance.setActive(uid,true);
                UIManager.Instance.test(uid,_);
            })
            .AddTo(go);
        
        this.UpdateAsObservable()
            .Select(_ => _camera.WorldToViewportPoint(go.transform.position))
            .Where(_ => (_.x < 1 && _.x > 0 && _.y < 1 && _.y > 0))
            .DistinctUntilChanged(_ => _)
            .Subscribe(_ =>
            {
                UIManager.Instance.setActive(uid,false);
            })
            .AddTo(go);
        
        unitList.Add(uid,go);
    }

    public void newController(int uid)
    {
        if (PlayerConnectionLogic.Instance.addPlayer(uid, uid) == false)
            return;
            
        var index = PlayerConnectionLogic.Instance.currentPlayerNumber;
        var axes = ReadInputManager.ReadAxes();
        var AxisNumber = 5;
        var useAxes = new ArraySegment<string>(axes, (index - 1) * AxisNumber, AxisNumber).ToArray();
        
        var ic = new InputObservableController(uid,gameObject);

        //Arrows
        ic.addNewEvent(0,25, new []{useAxes[0],useAxes[1]},true,false, _ =>
        {
            onMoveEvent(uid,new Point(_[0],0,_[1]));
        });
        
        //Interaction
        ic.addNewEvent(0,500,new []{useAxes[2]},false,false, _ =>
        {
            onInteractionEvent(uid,_[0] > 0);
        });
        
        //Put
        ic.addNewEvent(0,500,new []{useAxes[3]},false,false, _ =>
        {
            Logger.Log("onPutEvent");
        });
        
        //Menu
        ic.addNewEvent(0,500,new []{useAxes[4]},false,false, _ =>
        {
            Logger.Log("onMenuEvent");
        });
        
        newTestUnit(uid);
        addController(ic);
    }

    void addController(InputObservableController ic)
    {
        controllers.Add(ic);
    }

    public void onMoveEvent(int uid, Point e)
    {
        if (unitList.ContainsKey(uid) == false)
        {
            Logger.Log(uid + " unit lost");
        }
        else
        {
            GameObject unit;
            unitList.TryGetValue(uid,out unit);
            
            if (unit == null)
                return;
            
            unit.GetComponent<CharacterAction>().move(e);
        }
    }
    
    public void onInteractionEvent(int uid, bool e)
    {
        if (unitList.ContainsKey(uid) == false)
        {
            Logger.Log(uid + " unit lost");
        }
        else
        {
            GameObject unit;
            unitList.TryGetValue(uid,out unit);

            if (unit == null)
                return;
            
            unit.GetComponent<CharacterAction>().interaction();
        }
    }
}
