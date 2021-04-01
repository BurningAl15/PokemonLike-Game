using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public enum BattleState
{
    Start,
    PlayerAction,
    PlayerMove,
    EnemyMove,
    Busy,
    PartyScreen
}

public class BattleSystem : MonoBehaviour
{
    [Header("Player Pokemon")] 
    [SerializeField] private BattleUnit playerUnit;
    [SerializeField] private BattleHud playerHud;

    [Header("Wild Pokemon")] 
    [SerializeField] private BattleUnit wildUnit;
    [SerializeField] private BattleHud wildHud;

    [Header("Dialog Box")] [SerializeField]
    private BattleDialogBox dialogBox;

    [Header("Party Screen")] [SerializeField]
    private PartyScreen _partyScreen;
    
    [SerializeField] private BattleState state;

    #region Indexes

    //Main Menu Actions
    private int currentAction_Index;

    //Attack Menu Actions
    private int currentMove_Index;
    
    //Party Menu Actions
    private int currentPartyMember_Index;    

    #endregion
    
    public event Action OnFinishBattle;
    private Coroutine currentCoroutine = null;

    private PokemonParty playerParty;
    private Pokemon wildPokemon;


    #region To Start Battle

    private void OnEnable()
    {
        currentAction_Index = 0;
        currentMove_Index = 0;
    }

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
        
        _partyScreen.Init();

        yield return TransitionManager._instance.TransitionEffect_FadeOut();
        
        dialogBox.SetMoveNames(playerUnit._Pokemon.Moves);
        
        string _dialog = $"A wild {wildUnit._Pokemon.Base.Name} appeared!";

        yield return dialogBox.WriteType(_dialog);

        yield return StartCoroutine(PlayerAction_Interaction());
        currentCoroutine = null;
    }

    #endregion

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
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
    }

    #region Coroutines
    
    IEnumerator Run()
    {
        print("Run init");
        yield return TransitionManager._instance.TransitionEffect_FadeIn();
        OnFinishBattle?.Invoke();
        this.gameObject.SetActive(false);
        currentCoroutine = null;
        print("Run End");
    }

    IEnumerator PerformPlayerMove()
    {
        state = BattleState.Busy;
        playerUnit._Pokemon.PP_Move(currentMove_Index);
        var move = playerUnit._Pokemon.Moves[currentMove_Index];
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
            
            var nextPokemon = playerParty.GetHealthyPokemon();
            print(nextPokemon);
            if (nextPokemon != null)
            {
                // playerUnit.Setup(nextPokemon);
                // playerHud.SetData(playerUnit._Pokemon);
                //
                // dialogBox.SetMoveNames(playerUnit._Pokemon.Moves);
                // yield return dialogBox.WriteType($"Go {playerUnit._Pokemon.Base.Name}, I choose you!");
                //
                // yield return PlayerAction_Interaction();
                OpenPartyScreen();
            }
            else
            {
                yield return TransitionManager._instance.TransitionEffect_FadeIn();
                OnFinishBattle?.Invoke();
            }

            currentCoroutine = null;
        }
        else
        {
            yield return StartCoroutine(PlayerAction_Interaction());
        }
    }

    #endregion

    #region Handle Input

    private void HandlePartySelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentPartyMember_Index;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentPartyMember_Index;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentPartyMember_Index += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentPartyMember_Index -= 2;

        currentPartyMember_Index = Mathf.Clamp(currentPartyMember_Index, 0, playerParty.PokemonTeam.Count - 1);

        _partyScreen.UpdatePartyMemberSelection(currentPartyMember_Index);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);

            var selectedMember = playerParty.PokemonTeam[currentPartyMember_Index];
            if (selectedMember.HP <= 0)
            {
                _partyScreen.SetMessageText("You can't send out a fainted pokemon");
                return;
            }

            if (selectedMember == playerUnit._Pokemon)
            {
                _partyScreen.SetMessageText("You can't switch with the same pokemon");
                return;
            }
            _partyScreen.gameObject.SetActive(false);
            state = BattleState.Busy;
            
            if (currentCoroutine == null)
                currentCoroutine = StartCoroutine(SwitchPokemon(selectedMember));
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            _partyScreen.gameObject.SetActive(false);

            if (currentCoroutine == null)
                currentCoroutine = StartCoroutine(PlayerAction_Interaction(true,false));
        }
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        if (playerUnit._Pokemon.HP > 0)
        {
            yield return dialogBox.WriteType($"Come back {playerUnit._Pokemon.Base.Name}");
            yield return playerUnit.PlayFaintAnimation();
        }
        // else
        // {
        //     yield return dialogBox.WriteType($"Good Job {playerUnit._Pokemon.Base.Name}, come back");
        //     yield return playerUnit.PlayFaintAnimation();
        // }

        
        playerUnit.Setup(newPokemon);
        playerHud.SetData(newPokemon);
                
        dialogBox.SetMoveNames(newPokemon.Moves);
        yield return dialogBox.WriteType($"Go {playerUnit._Pokemon.Base.Name}, I choose you!");

        yield return EnemyMove();
        
        currentCoroutine = null;
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentAction_Index;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentAction_Index;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentAction_Index += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentAction_Index -= 2;

        currentAction_Index = Mathf.Clamp(currentAction_Index, 0, 3);

        dialogBox.UpdateActionSelection(currentAction_Index);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            switch (currentAction_Index)
            {
                //Fight
                case 0:
                    PlayerMove_Interaction();
                    break;
                //Bag
                case 1:

                    break;
                //Pokemon
                case 2:
                    OpenPartyScreen();
                    break;
                //Run
                case 3:
                    if (currentCoroutine == null)
                        currentCoroutine = StartCoroutine(Run());
                    break;
            }
        }
    }

    private void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        _partyScreen.SetPartyData(playerParty.PokemonTeam);
        _partyScreen.gameObject.SetActive(true);
    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentMove_Index;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentMove_Index;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMove_Index += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMove_Index -= 2;

        currentMove_Index = Mathf.Clamp(currentMove_Index, 0, playerUnit._Pokemon.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(currentMove_Index,playerUnit._Pokemon.Moves[currentMove_Index]);
        
        if (Input.GetKeyDown(KeyCode.Z))
        {
          dialogBox.EnableMoveSelector(false);
          dialogBox.EnableDialogText(true);
          if (currentCoroutine == null)
              currentCoroutine = StartCoroutine(PerformPlayerMove());
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            if (currentCoroutine == null)
                currentCoroutine = StartCoroutine(PlayerAction_Interaction(true,false));
        }
    }

    IEnumerator PlayerAction_Interaction(bool end=false,bool isTypewrite=true)
    {
        if(isTypewrite)
            yield return dialogBox.WriteType("Choose an action");
        else
            dialogBox.SetDialog("Choose an action");
        state = BattleState.PlayerAction;
        dialogBox.EnableActionSelector(true);
        yield return new WaitForSeconds(.35f);
        if (end)
            currentCoroutine = null;
    }

    void PlayerMove_Interaction()
    {
        state = BattleState.PlayerMove;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }
    
    #endregion
    
}