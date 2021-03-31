using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameController : MonoBehaviour
{

    [SerializeField] private PlayerController2D_TopDown _playerController;
    [SerializeField] private BattleSystem _battleSystem;
    [SerializeField] private GameObject mainCamera;

    private Coroutine currentCoroutine = null;
    
    private void Start()
    {
        _playerController.OnEncountered += StartBattle;
        _battleSystem.OnFinishBattle += To_Overworld;
        To_Overworld();
    }

    IEnumerator InitialTransition()
    {
        yield return TransitionManager._instance.TransitionEffect_FadeOut();
        currentCoroutine = null;
    }

    private void Update()
    {
        if (GameStateManager._instance.isInOverworld())
        {
            _playerController.HandleUpdate();
        }
        else if (GameStateManager._instance.isInBattle())
        {
            _battleSystem.HandleUpdate();
        }
    }

    void StartBattle()
    {
        GameStateManager._instance.GameState_Battle();
        _battleSystem.gameObject.SetActive(true);
        mainCamera.SetActive(false);
        
        _battleSystem.StartBattle();
    }

    void To_Overworld()
    {
        GameStateManager._instance.GameState_Overworld();
        _battleSystem.gameObject.SetActive(false);
        mainCamera.SetActive(true);

        if (currentCoroutine == null)
            currentCoroutine = StartCoroutine(InitialTransition());
    }
}
