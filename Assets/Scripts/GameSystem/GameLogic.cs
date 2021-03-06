﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Schema;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public interface GameLogic
{
    void active();
    void stop();
    void run();
    void mainLogic();
}

public class BasicLogic<T> : GameLogic where T : class, new()
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
                _instance = new T();
            return _instance;
        }
    }
    
    private bool _flag = false;
    
    public void active()
    {
        if(_flag == false)
            _flag = true;
    }

    public void stop()
    {
        if (_flag)
            _flag = false;
    }

    public void run()
    {
        if(_flag)
            mainLogic();
    }

    public virtual void mainLogic()
    {
    }
}

/// <summary>
/// object polling class
/// </summary>
public class ObjectRecyclingLogic : BasicLogic<ObjectRecyclingLogic>
{
    private Dictionary<string, GameObject> trashCan;

    public ObjectRecyclingLogic()
    {
        if(trashCan == null)
            trashCan = new Dictionary<string, GameObject>();
    }

    public void chunk(string name, GameObject go)
    {
        trashCan[name] = go;
        go.SetActive(false);
        
        Logger.Log("TrashCan has " + trashCan.Count + " Items");
    }

    [CanBeNull]
    public GameObject randomPickUp()
    {
        if (trashCan.Count == 0)
            return null;

        var rValue = UnityEngine.Random.Range(0, trashCan.Count);

        return pickUp(trashCan.Keys.ToArray()[rValue]);
    }

    [CanBeNull]
    public GameObject pickUp(string name)
    {
        if (trashCan.ContainsKey(name))
        {
            var go = trashCan[name];
            trashCan.Remove(name);
            go.SetActive(true);
            return go;
        }

        Logger.Log(name + " is not in trashCan.");
        
        return null;
    }
    
    public override void mainLogic()
    {
    }
}

public class TimeLogic : BasicLogic<TimeLogic>
{
    private float _limitTime; //제한 시간
    private float _currentTime; //제한 시간 중 남은 시간
    private float _tikTime;
    
    private Text testText;
    private const float _zeroTime = 0f;

    public TimeLogic()
    {
        Logger.LogWarning("Time Logic Running...");
    }

    public void setText(Text text)
    {
        testText = text;
    }
    
    public void setTime(float time)
    {
        _limitTime = time;
        _currentTime = time;
    }

    public float getTikTime()
    {
        return _tikTime;
    }

    public void countDownTik()
    {
        _tikTime = Time.deltaTime;
        _currentTime -= _tikTime;

        GameTimeManager.Instance.SetTimeText(_currentTime);
        
        if (_currentTime < _zeroTime)
        {
            //Logger.LogError("TimeOver");
            GameOverManager.Instance.Fail();
            stop();
        }
    }

    public override void mainLogic()
    {
        countDownTik();
    }

    public void addTime(float time)
    {
        _currentTime += time;
        if (_currentTime > _limitTime)
            _currentTime = _limitTime;
    }
}

public class MissionLogic : BasicLogic<MissionLogic>
{
    private float missionPercent = 0;
    private float animationPurposePercent = 0;
    private float _interval = 0.5f;
    
    private RequiredItems missionItemList = new RequiredItems();
    private RequiredItems possessingItems = new RequiredItems();

    public event Action OnAnimationDone;
    
    public MissionLogic()
    {
        Logger.LogWarning("Mission Logic Running");
    }

    public void setList(RequiredItems list)
    {
        missionItemList = list;
        possessingItems = new RequiredItems();
        possessingItems.requiredItems = new Dictionary<ItemKind, int>();
    }

    public void setInterval(float value)
    {
        _interval = value;
    }
    
    public bool isItemRequired(ItemKind itemName)
    {
        bool isTarget = missionItemList.requiredItems.Any( i => i.Key == itemName);
        bool isNeededMore = false;
        int required = 0, possess = 0;
        
        if(missionItemList.requiredItems.TryGetValue(itemName, out required))
        {
            possessingItems.requiredItems.TryGetValue(itemName, out possess);
            isNeededMore = required > possess;
        }
        
        return isTarget && isNeededMore;
    }

    public override void mainLogic()
    {
        if (animationPurposePercent < missionPercent)
        {
            animationPurposePercent += _interval;
            if(animationPurposePercent >= missionPercent)
            {
                OnAnimationDone.Invoke();
            }
        }
    }
    public void putItem(Item item)
    { 
        if(missionItemList.requiredItems.ContainsKey(item.kind))
        {
            if(possessingItems.requiredItems.ContainsKey(item.kind))
            {
                if(possessingItems.requiredItems[item.kind] <
                    missionItemList.requiredItems[item.kind])
                {
                    possessingItems.requiredItems[item.kind] = possessingItems.requiredItems[item.kind] + 1;   
                }
                
                if(possessingItems.requiredItems[item.kind] >= missionItemList.requiredItems[item.kind])
                {
                    GameOverManager.Instance.Goal();
                    //Debug.LogError("You can't put item more than requied amount.");
                    return;
                }
            }
            else
            {
                possessingItems.requiredItems.Add(item.kind, 1);
            }
        }
        else
        {
            Debug.LogError("You can't put item to misson logic that is not required.");
            return;
        }
        
        UIManager.Instance.setText();
        updatePercent();
    }

    public Dictionary<ItemKind,int> requiredItems()
    {
        var mil = missionItemList.requiredItems;
        var pil = possessingItems.requiredItems;

        Dictionary<ItemKind, int> tempList = new Dictionary<ItemKind, int>();

        foreach (var item in mil)
        {
            if (pil.ContainsKey(item.Key))
            {
                tempList.Add(item.Key,mil[item.Key] - pil[item.Key]);
            }
            else
            {
                tempList.Add(item.Key,mil[item.Key]);
            }
        }

        return tempList;
    }
    
    private void updatePercent()
    {
        int total = 0, current = 0;
        foreach(var item in missionItemList.requiredItems)
        {
            total += item.Value;
        }
        foreach(var item in possessingItems.requiredItems)
        {
            current += item.Value;
        }

        missionPercent = (float)current / total * 100.0f;
    }

    public float getPercent()
    {
        return animationPurposePercent;
    }
    //For Debug Purpose Only
    public void setPercent(float percent)
    {
        missionPercent = percent;
    }
}

public class PlayerControlLogic : BasicLogic<PlayerControlLogic>
{
    private Dictionary<int,PlayerState> PlayerDics;
    private int _currentPlayerNumber;
    public int currentPlayerNumber
    {
        get { return _currentPlayerNumber; }
    }

    private int _maximumPlayerNumber;
    public int maximumPlayerNumber
    {
        get { return _maximumPlayerNumber; }
    }
    public PlayerControlLogic()
    {
        if(PlayerDics == null)
            PlayerDics = new Dictionary<int, PlayerState>();
    }

    public bool canAddPlayer()
    {
        if (currentPlayerNumber < maximumPlayerNumber)
        {
            return true;
        }

        return false;
    }
    
    public bool addPlayer(int uid, PlayerState state)
    {
        if (currentPlayerNumber < maximumPlayerNumber)
        {
            PlayerDics[uid] = state;
            _currentPlayerNumber++;
            return true;
        }
        else
            return false;
    }

    [CanBeNull]
    public PlayerState getPlayerState(int uid)
    {
        if (PlayerDics.ContainsKey(uid))
            return PlayerDics[uid];

        return null;
    }

    public PlayerState[] getAllPlayerState()
    {
        return PlayerDics.Values.ToArray();
    }

    public void setMaximumNumber(int value)
    {
        _maximumPlayerNumber = value;
    }
    
    public override void mainLogic()
    {
    }
}

public class BorderCube
{
    private float minX;
    private float minY;
    private float minZ;
    private float maxX;
    private float maxY;
    private float maxZ;

    public bool canMove;

    public BorderCube setBorder(float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
    {
        this.minX = minX;
        this.minY = minY;
        this.minZ = minZ;
        this.maxX = maxX;
        this.maxY = maxY;
        this.maxZ = maxZ;

        return this;
    }

    public BorderCube setMove(bool value)
    {
        canMove = value;
        return this;
    }
    public bool isContain(Vector3 position)
    {
        if (!(position.x > minX && position.x < maxX))
            return false;

        if (!(position.y > minY && position.y < maxY))
            return false;

        if (!(position.z > minZ && position.z < maxZ))
            return false;

        return true;
    }
}

public class PlayerMoveLimitLogic : BasicLogic<PlayerMoveLimitLogic>
{
    private List<BorderCube> borders;
    
    public PlayerMoveLimitLogic()
    {
        borders = new List<BorderCube>();
    }
    public override void mainLogic()
    {
    }

    public void addBorder(BorderCube cube)
    {
        borders.Add(cube);
    }

    public bool canMove(Vector3 position)
    {
        if (borders == null || borders.Count == 0)
            return true;

        List<BorderCube> checkList = new List<BorderCube>();
        
        foreach (var cube in borders)
        {
            if(cube.isContain(position))
                checkList.Add(cube);
        }

        if (checkList.Count == 0)
            return false;

        var returnValue = true;
        
        foreach (var cube in checkList)
        {
            returnValue |= cube.canMove;
        }

        return returnValue;
    }
}

public enum BuffKind
{
    SpeedUp,
    SpeedDown,
    Stun
}

public class Buff
{
    public BuffKind kind;
    public List<PlayerState> targets;
    private float holdingTime;
    private bool activeValue;
    
    public Action<List<PlayerState>> startFunc;
    public Action<List<PlayerState>> endFunc;

    public Buff(Action<List<PlayerState>> startFunc,Action<List<PlayerState>> endFunc, float time, params PlayerState[] targets)
    {
        if(this.targets == null)
            this.targets = new List<PlayerState>();
        
        this.startFunc = startFunc;
        this.endFunc = endFunc;
        holdingTime = time;
        
        activeValue = true;

        this.targets.AddRange(targets);
        
        this.startFunc(this.targets);
    }
    
    public void addTime(float time)
    {
        holdingTime += time;
        activeValue = true;
    }
    
    public void active()
    {
        if (activeValue == false)
            return;
        
        holdingTime -= TimeLogic.Instance.getTikTime();

        if (holdingTime < 0)
        {
            holdingTime = 0;
            activeValue = false;
            endFunc(this.targets);
            
            BuffLogic.Instance.deleteBuff(this);
        }
    }
}

public class BuffLogic : BasicLogic<BuffLogic>
{
    private List<Buff> _buffList;
    private List<Buff> _setDirty;
    
    public BuffLogic()
    {
        if(_buffList == null)
            _buffList = new List<Buff>();
        
        if(_setDirty == null)
            _setDirty = new List<Buff>();
    }

    public void deleteBuff(Buff buff)
    {
        _setDirty.Add(buff);
    }

    public void addBuff(BuffKind kind,Action<List<PlayerState>> startFunc,Action<List<PlayerState>> endFunc, float time, params PlayerState[] targets)
    {
        if(_buffList == null)
            _buffList = new List<Buff>();
        
        var newBuff = new Buff(startFunc,endFunc,time,targets);

        foreach (var buff in _buffList)
        {
            if (buff.targets.Equals(newBuff.targets) && buff.kind == newBuff.kind)
            {
                newBuff.endFunc = mergeAction(buff.endFunc, newBuff.endFunc);
                _buffList.Remove(buff);
            }
        }
        
        _buffList.Add(newBuff);
    }

    public Action<List<PlayerState>> mergeAction(Action<List<PlayerState>> a, Action<List<PlayerState>> b)
    {
        return (value) =>
        {
            a(value);
            b(value);
        };
    }
    
    public override void mainLogic()
    {
        if (_buffList == null)
            return;

        foreach (var buff in _buffList)
        {
            buff.active();
        }

        foreach (var buff in _setDirty)
        {
            if(_buffList.Contains(buff))
                _buffList.Remove(buff);
        }
        
        if(_setDirty.Count > 0)
            _setDirty.Clear();
    }
}