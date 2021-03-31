using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapArea : MonoBehaviour
{
    [SerializeField] private List<Pokemon> wildPokemons;

    public Pokemon GetRandomWildPokemon()
    {
        Pokemon tempPokemon = wildPokemons[Random.Range(0, wildPokemons.Count)];
        tempPokemon.Init();

        return tempPokemon;
    }
}
