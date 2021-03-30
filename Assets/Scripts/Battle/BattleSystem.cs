using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState
{
    Start,
    PlayerAction,
    PlayerMove,
    EnemyMove,
    Busy
}

public class BattleSystem : MonoBehaviour
{
    [Header("Player Pokemon")] [SerializeField]
    private BattleUnit playerUnit;

    [SerializeField] private BattleHud playerHud;

    [Header("Wild Pokemon")] [SerializeField]
    private BattleUnit wildUnit;

    [SerializeField] private BattleHud wildHud;

    [Header("Dialog Box")] [SerializeField]
    private BattleDialogBox dialogBox;

    private Coroutine currentCoroutine = null;

    private BattleState state;

    [SerializeField] private Camera mainCamera;
    
    //Main Menu Actions
    private int currentAction;

    //Attack Menu Actions
    private int currentMove;
    
    private void OnEnable()
    {
        currentAction = 0;
        currentMove = 0;
    }

    void Start()
    {
        if (currentCoroutine == null)
            currentCoroutine = StartCoroutine(SetupBattle());
    }

    IEnumerator SetupBattle(string dialog = "")
    {
        playerUnit.Setup();
        playerHud.SetData(playerUnit.Pokemon);

        wildUnit.Setup();
        wildHud.SetData(wildUnit.Pokemon);

        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
        
        string _dialog = $"A wild {wildUnit.Pokemon.Base.Name} appeared!";

        yield return dialogBox.WriteType(_dialog);
        yield return new WaitForSeconds(1f);

        yield return PlayerAction();
        currentCoroutine = null;
        print("coroutine cleared");
    }

    IEnumerator PlayerAction()
    {
        state = BattleState.PlayerAction;
        yield return dialogBox.WriteType("Choose an action");
        // dialogBox.EnableDialogText(false);
        yield return new WaitForSeconds(.35f);
        dialogBox.EnableActionSelector(true);
    }

    void PlayerMove()
    {
        state = BattleState.PlayerMove;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    private void Update()
    {
        if (state == BattleState.PlayerAction)
        {
            HandleActionSelection();
        }else if (state == BattleState.PlayerMove)
        {
            HandleMoveSelection();
        }
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentAction < 1)
                ++currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentAction > 0)
                --currentAction;
        }

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                //Fight
                PlayerMove();
            }
            else if (currentAction == 1)
            {
                //Run
                this.gameObject.SetActive(false);
                ScenaryType._instance.GameState_Overworld();
            }
        }
    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentMove < playerUnit.Pokemon.Moves.Count-1)
                ++currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentMove > 0)
                --currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentMove < playerUnit.Pokemon.Moves.Count - 2)
                currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentMove > 1)
                currentMove -= 2;
        }

        dialogBox.UpdateMoveSelection(currentMove,playerUnit.Pokemon.Moves[currentMove]);
        
        if (Input.GetKeyDown(KeyCode.Z))
        {
          dialogBox.EnableMoveSelector(false);
          dialogBox.EnableDialogText(true);
          if (currentCoroutine == null)
              currentCoroutine = StartCoroutine(PerformPlayerMove());
        }
    }

    IEnumerator PerformPlayerMove()
    {
        state = BattleState.Busy;
        
        var move = playerUnit.Pokemon.Moves[currentMove];
        yield return dialogBox.WriteType(
            $"{playerUnit.Pokemon.Base.Name} used {move.Base.Name}");

        yield return new WaitForSeconds(1f);

        bool isFainted = wildUnit.Pokemon.TakeDamage(move, playerUnit.Pokemon);
        yield return wildHud.UpdateHP();

        if (isFainted)
        {
            yield return dialogBox.WriteType(
                $"{wildUnit.Pokemon.Base.Name} Fainted!");
            yield return new WaitForSeconds(1f);
            ScenaryType._instance.GameState_Overworld();
        }
        else
        {
            yield return EnemyMove();
        }
        
        currentCoroutine = null;
    }


    IEnumerator EnemyMove()
    {
        state = BattleState.EnemyMove;

        var move = wildUnit.Pokemon.GetRandomMove();
        
        yield return dialogBox.WriteType(
            $"{wildUnit.Pokemon.Base.Name} used {move.Base.Name}");

        yield return new WaitForSeconds(1f);

        bool isFainted = playerUnit.Pokemon.TakeDamage(move, playerUnit.Pokemon);
        yield return playerHud.UpdateHP();
        
        if (isFainted)
        {
            yield return dialogBox.WriteType(
                $"{playerUnit.Pokemon.Base.Name} Fainted!");
            yield return new WaitForSeconds(1f);
            ScenaryType._instance.GameState_Overworld();
        }
        else
        {
            yield return PlayerAction();
        }
    }
}