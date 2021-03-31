using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Overworld,
    Battle
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager _instance;

    [SerializeField] private GameState gameState;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
    }

    public void GameState_Overworld()
    {
        gameState = GameState.Overworld;
    }
    
    public void GameState_Battle()
    {
        gameState = GameState.Battle;
    }

    public bool isInOverworld()
    {
        return gameState == GameState.Overworld;
    }
    
    public bool isInBattle()
    {
        return gameState == GameState.Battle;
    }
}
