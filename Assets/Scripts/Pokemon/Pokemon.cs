﻿using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Pokemon
{
    public PokemonBase Base;
    public int Level;

    public int HP { get; set; }
    public List<Move> Moves { get; set; }

    // public int OldHP;
    
    public Pokemon(PokemonBase pBase, int pLevel)
    {
        Base = pBase;
        Level = pLevel;
        HP = MaxHP;
        // OldHP = HP;
        //Generate Moves
        Moves = new List<Move>();
        for (int i = 0; i < Base.LearnableMoves.Count; i++)
        {
            LearnableMove temp = Base.LearnableMoves[i];
            if(temp.Level<=Level)
                Moves.Add(new Move(temp.Base));

            if (Moves.Count >= 4)
                break;
        }
    }

    public void PP_Move(int index)
    {
        Moves[index].PP--;
    }

    public void ResetPP()
    {
        for (int i = 0; i < Moves.Count; i++)
        {
            Moves[i].ResetPP();
        }
    }

    public int Attack
    {
        get { return Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5; }
    }
    public int Defense
    {
        get { return Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5; }
    }
    public int SpAttack
    {
        get { return Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5; }
    }
    public int SpDefense
    {
        get { return Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5; }
    }
    public int Speed
    {
        get { return Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5; }
    }
    public int MaxHP
    {
        get { return Mathf.FloorToInt((Base.MAXHp * Level) / 100f) + 10; }
    }

    public DamageDetails TakeDamage(Move move, Pokemon attacker)
    {
        float critical = 1f;
        if (Random.value * 100f <= 6.25f)
            critical = 2f;
        
        float typeEffectiveness = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) *
                     TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        var damageDetails = new DamageDetails()
        {
            TypeEfectiveness = typeEffectiveness,
            Critical = critical,
            Fainted = false
        };

        float attack = (move.Base.IsSpecial) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.IsSpecial) ? SpDefense : Defense;
        
        float modifiers = Random.Range(0.85f, 1f) * typeEffectiveness * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float) attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        HP -= damage;
        if (HP <= 0)
        {
            HP = 0;
            damageDetails.Fainted = true;
        }

        return damageDetails;
    }

    // public void UpdateOldHP()
    // {
    //     OldHP = HP;
    // }
    
    public Move GetRandomMove()
    {
        int r = Random.Range(0, Moves.Count);
        Moves[r].PP--;
        return Moves[r];
    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEfectiveness { get; set; }
}