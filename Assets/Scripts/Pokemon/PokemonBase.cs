using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="Pokemon",menuName = "Pokemon/Create new Pokemon")]
public class PokemonBase : ScriptableObject
{
    [SerializeField] private string name;

    [TextArea] [SerializeField] private string description;

    [SerializeField] private Sprite frontSprite;
    [SerializeField] private Sprite backSprite;
    [SerializeField] private PokemonType type1;
    [SerializeField] private PokemonType type2;
    
    //Base Stats
    [SerializeField] private int maxHP;
    [SerializeField] private int attack;
    [SerializeField] private int defense;
    [SerializeField] private int spAttack;
    [SerializeField] private int spDefense;
    [SerializeField] private int speed;

    [SerializeField] private List<LearnableMove> _learnableMoves;
    
    public string Name => name;

    public string Description => description;

    public int MAXHp => maxHP;

    public int Attack => attack;

    public int Defense => defense;

    public int SpAttack => spAttack;

    public int SpDefense => spDefense;

    public int Speed => speed;

    public PokemonType Type1 => type1;
    public PokemonType Type2 => type2;

    public Sprite FrontSprite => frontSprite;

    public Sprite BackSprite => backSprite;

    public List<LearnableMove> LearnableMoves
    {
        get { return _learnableMoves; }
    }
}

[Serializable]
public class LearnableMove
{
    [SerializeField] private MoveBase moveBase;
    [SerializeField] private int level;

    public MoveBase Base
    {
        get { return moveBase; }
    }

    public int Level
    {
        get { return level; }
    }
}

public enum PokemonType
{
    None,
    Normal,
    Fire,
    Water,
    Electric,
    Grass,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Fairy,
    Dragon
}

public enum Stat
{
    Attack,
    Defense,
    SpAttack,
    SpDefense,
    Speed
}


public class TypeChart
{
    static float[][] chart =
    {   //                       Nor  Fir  Wat Ele  Gra Ice Fig Poi
        /*Normal*/ new float[] { 1f,  1f,  1f,  1f,  1f, 1f, 1f,  1f},
        /*Fire*/ new float[]   { 1f, .5f, .5f,  1f,  2f, 2f, 1f,  1f},
        /*Water*/ new float[]  { 1f,  2f, .5f,  2f, .5f, 1f, 1f,  1f},
        /*Elec*/ new float[]   { 1f,  1f,  2f, .5f, .5f, 2f, 1f,  1f},
        /*Grass*/ new float[]  { 1f, .5f,  2f,  2f, .5f, 1f, 1f, .5f},
        /*Ice*/ new float[]    { 1f,  2f, .5f,  2f, .5f, 1f, 1f,  1f},
        /*Fight*/ new float[]  { 1f,  2f, .5f,  2f, .5f, 1f, 1f,  1f},
        /*Poison*/ new float[] { 1f,  2f, .5f,  2f, .5f, 1f, 1f,  1f},
    };

    public static float GetEffectiveness(PokemonType attackType, PokemonType defenseType)
    {
        if (attackType == PokemonType.None || defenseType == PokemonType.None)
            return 1;

        int row = (int) attackType - 1;
        int col = (int) defenseType - 1;

        return chart[row][col];
    }
}