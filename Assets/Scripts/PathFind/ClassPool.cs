// ==================================================
// Copyright (c) All rights reserved.
// @Author: GodWinTsai
// @Maintainer: 
// @Date: 
// @Desc: 
// ==================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public sealed class ClassPool<T> where T : new()
{
    private readonly HashSet<T> _hashSet;
    private readonly UnityAction<T> _actionOnGet;
    private readonly UnityAction<T> _actionOnRelease;

    public int CountAll { get; private set; }
    public int CountActive { get { return CountAll - CountInactive; } }
    public int CountInactive { get { return _hashSet.Count; } }

    public ClassPool(UnityAction<T> actionOnGet, UnityAction<T> actionOnRelease)
    {
        _actionOnGet = actionOnGet;
        _actionOnRelease = actionOnRelease;
        _hashSet = new HashSet<T>();
    }

    public T Get()
    {
        T activeClass = default;

        if (CountInactive == 0)
        {
            activeClass = new T();
            CountAll++;
        }
        else
        {
            foreach (var pooledClass in _hashSet)
            {
                activeClass = pooledClass;
                break;
            }

            _hashSet.Remove(activeClass);
        }

        _actionOnGet?.Invoke(activeClass);

        return activeClass;
    }

    public void Release(T releaseClass)
    {
        if (releaseClass == null)
        {
            return;
        }

        if (_hashSet.Contains(releaseClass))
        {
            Debug.LogError("[ClassPool] internal error, trying to release class-instance that already released to pool");
            return;
        }

        _actionOnRelease?.Invoke(releaseClass);
        _hashSet.Add(releaseClass);
    }

    public void Clear()
    {
        _hashSet.Clear();
    }
}