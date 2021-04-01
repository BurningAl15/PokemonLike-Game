using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName="Move",menuName = "Pokemon/Create new move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] private string name;
    [TextArea] [SerializeField] private string description;

    [SerializeField] private PokemonType type;
    [SerializeField] private int power;
    [SerializeField] private int accuracy;
    [SerializeField] private int pp;

    [SerializeField] private MoveCategory _category;
    [SerializeField] private MoveEffects _effects;
    [SerializeField] private MoveTarget _target;
    
    public string Name => name;

    public string Description => description;

    public PokemonType Type => type;

    public int Power => power;

    public int Accuracy => accuracy;

    public int Pp => pp;

    public MoveCategory Category => _category;

    public MoveEffects Effects => _effects;

    public MoveTarget Target => _target;
}

[Serializable]
public class MoveEffects
{
    [SerializeField] private List<StatBoost> boosts;

    public List<StatBoost> Boosts => boosts;
}

[Serializable]
public class StatBoost
{
    public Stat stat;
    public int boost;
}

public enum MoveCategory
{
    Physical,Special,Status
}

public enum MoveTarget
{
    Foe,Self
}
