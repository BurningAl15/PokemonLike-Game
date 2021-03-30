using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] private PokemonBase _base;
    [SerializeField] private int level;
    [SerializeField] private bool isPlayerUnit;
    [SerializeField] private Image pokemonImage;
    
    public  Pokemon Pokemon { get; set; }
    
    public void Setup()
    {
        print("Calling");
        Pokemon = new Pokemon(_base, level);
        pokemonImage.sprite = isPlayerUnit ? _base.BackSprite : _base.FrontSprite;
    }
}
