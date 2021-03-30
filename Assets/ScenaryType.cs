using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Overworld,
    Battle
}

public class ScenaryType : MonoBehaviour
{
    public static ScenaryType _instance;

    private GameState gameState;

    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject battlePhase;
  
    private void Awake()
    {
        if (_instance == null)
            _instance = this;

        GameState_Overworld();
    }

    public void GameState_Overworld()
    {
        gameState = GameState.Overworld;
        mainCamera.SetActive(true);
        battlePhase.SetActive(false);
    }
    
    public void GameState_Battle()
    {
        gameState = GameState.Battle;
        battlePhase.SetActive(true);
        mainCamera.SetActive(false);
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
