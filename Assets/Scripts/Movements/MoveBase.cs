using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="Move",menuName = "Pokemon/Create new move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] private string name;
    [TextArea] [SerializeField] private string description;

    [SerializeField] private PokemonType type;
    [SerializeField] private int power;
    [SerializeField] private int accuracy;
    [SerializeField] private int pp;

    [SerializeField] private bool isSpecial;
    
    public string Name => name;

    public string Description => description;

    public PokemonType Type => type;

    public int Power => power;

    public int Accuracy => accuracy;

    public int Pp => pp;

    public bool IsSpecial => isSpecial;

    // get
    // {
    //     if (type == PokemonType.Fire || type == PokemonType.Water || type == PokemonType.Grass
    //     || type == PokemonType.Ice || type==PokemonType.Electric || type == PokemonType.Dragon)
    //     {
    //         return true;
    //     }
    //     else
    //     {
    //         return false;
    //     }
    // }
}
