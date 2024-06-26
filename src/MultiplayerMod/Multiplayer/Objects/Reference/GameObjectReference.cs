﻿using System;
using UnityEngine;

namespace MultiplayerMod.Multiplayer.Objects.Reference;

[Serializable]
public abstract class GameObjectReference {

    public GameObject GetGameObject() => Resolve() ?? throw new ObjectNotFoundException(this);

    public T GetComponent<T>() where T : class => GetGameObject().GetComponent<T>();

    public Component? GetComponent(Type type) => GetGameObject().GetComponent(type);

    protected abstract GameObject? Resolve();

}
