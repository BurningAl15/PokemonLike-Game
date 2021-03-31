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

    [SerializeField] private BattleState state;

    //Main Menu Actions
    private int currentAction;

    //Attack Menu Actions
    private int currentMove;
    
    public event Action OnFinishBattle;
    
    private void OnEnable()
    {
        currentAction = 0;
        currentMove = 0;
    }

    private PokemonParty playerParty;
    private Pokemon wildPokemon;
    
    public void StartBattle(PokemonParty _playerParty, Pokemon _wildPokemon)
    {
        Debug.Log("Choosing pokemons ...");

        this.playerParty = _playerParty;
        this.wildPokemon = _wildPokemon;
        
        if (currentCoroutine == null)
            currentCoroutine = StartCoroutine(SetupBattle());
    }

    IEnumerator SetupBattle(string dialog = "")
    {
        Debug.Log("Setting Up ...");

        playerUnit.Setup(playerParty.GetHealthyPokemon());
        playerHud.SetData(playerUnit._Pokemon);

        wildUnit.Setup(wildPokemon);
        wildHud.SetData(wildUnit._Pokemon);
        Debug.Log("Pokemons Choosed ...");

        yield return TransitionManager._instance.TransitionEffect_FadeOut();
        
        dialogBox.SetMoveNames(playerUnit._Pokemon.Moves);
        
        string _dialog = $"A wild {wildUnit._Pokemon.Base.Name} appeared!";

        yield return dialogBox.WriteType(_dialog);

        yield return StartCoroutine(PlayerAction_Interaction());
        // dialogBox.EnableActionSelector(true);
        currentCoroutine = null;
    }

    IEnumerator PlayerAction_Interaction()
    {
        yield return dialogBox.WriteType("Choose an action");
        dialogBox.EnableActionSelector(true);
        yield return new WaitForSeconds(.35f);
        state = BattleState.PlayerAction;
    }

    void PlayerMove_Interaction()
    {
        state = BattleState.PlayerMove;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    public void HandleUpdate()
    {
        if (state == BattleState.PlayerAction)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.PlayerMove)
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
                PlayerMove_Interaction();
            }
            else if (currentAction == 1)
            {
                //Run
                // GameStateManager._instance.GameState_Overworld();
                if (currentCoroutine == null)
                    currentCoroutine = StartCoroutine(Run());
            }
        }
    }

    IEnumerator Run()
    {
        print("Run init");
        yield return TransitionManager._instance.TransitionEffect_FadeIn();
        OnFinishBattle?.Invoke();
        this.gameObject.SetActive(false);
        currentCoroutine = null;
        print("Run End");
    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentMove < playerUnit._Pokemon.Moves.Count-1)
                ++currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentMove > 0)
                --currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentMove < playerUnit._Pokemon.Moves.Count - 2)
                currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentMove > 1)
                currentMove -= 2;
        }

        dialogBox.UpdateMoveSelection(currentMove,playerUnit._Pokemon.Moves[currentMove]);
        
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
        playerUnit._Pokemon.PP_Move(currentMove);
        var move = playerUnit._Pokemon.Moves[currentMove];
        dialogBox.UpdateMovePP(move);
       
        yield return dialogBox.WriteType(
            $"{playerUnit._Pokemon.Base.Name} used {move.Base.Name}");

        yield return playerUnit.PlayAttackAnimation();
        
        yield return wildUnit.PlayHitAnimation();
        
        var damageDetails= wildUnit._Pokemon.TakeDamage(move, playerUnit._Pokemon);
        yield return wildHud.UpdateHP();

        yield return ShowDamageDetails(damageDetails);
        
        if (damageDetails.Fainted)
        {
            yield return dialogBox.WriteType(
                $"{wildUnit._Pokemon.Base.Name} Fainted!");
            yield return wildUnit.PlayFaintAnimation();
            yield return new WaitUntil(() => wildUnit.endAnimation);
            yield return TransitionManager._instance.TransitionEffect_FadeIn();
            OnFinishBattle?.Invoke();
            // GameStateManager._instance.GameState_Overworld();
        }
        else
        {
            yield return EnemyMove();
        }
        
        currentCoroutine = null;
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
            yield return dialogBox.WriteType("A critical hit!");

        if (damageDetails.TypeEfectiveness > 1)
            yield return dialogBox.WriteType("It's super effective!");
        else if (damageDetails.TypeEfectiveness < 1)
            yield return dialogBox.WriteType("It's not super effective!");
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.EnemyMove;

        var move = wildUnit._Pokemon.GetRandomMove();
        
        yield return dialogBox.WriteType(
            $"{wildUnit._Pokemon.Base.Name} used {move.Base.Name}");

        yield return wildUnit.PlayAttackAnimation();
        yield return playerUnit.PlayHitAnimation();
        
        var damageDetails = playerUnit._Pokemon.TakeDamage(move, playerUnit._Pokemon);
        yield return playerHud.UpdateHP();
        
        yield return ShowDamageDetails(damageDetails);
        
        if (damageDetails.Fainted)
        { 
            yield return dialogBox.WriteType(
                $"{playerUnit._Pokemon.Base.Name} Fainted!");
            
            yield return playerUnit.PlayFaintAnimation();
            yield return new WaitUntil(() => playerUnit.endAnimation);
            yield return TransitionManager._instance.TransitionEffect_FadeIn();
            OnFinishBattle?.Invoke();
            currentCoroutine = null;
        }
        else
        {
            yield return StartCoroutine(PlayerAction_Interaction());
        }
    }
}