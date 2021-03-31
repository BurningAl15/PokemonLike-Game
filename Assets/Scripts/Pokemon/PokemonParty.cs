using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] private List<Pokemon> pokemon_team;

    private void Start()
    {
        for (int i = 0; i < pokemon_team.Count; i++)
        {
            pokemon_team[i].Init();
            print(i + ". " + pokemon_team[i].Base.Name);
        }
    }

    public Pokemon GetHealthyPokemon()
    {
        //return the list of pokemon that satisfy the condition
        Pokemon tempPokemon = pokemon_team.FirstOrDefault(x => x.HP > 0);
        print(tempPokemon.Base.Name + ", I choose you!");
        return tempPokemon;
        // return pokemon_team.FirstOrDefault(x => x.HP > 0);
    }
}
