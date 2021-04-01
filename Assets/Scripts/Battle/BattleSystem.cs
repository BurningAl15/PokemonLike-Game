using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public enum BattleState
{
    Start,
    ActionSelection,
    MoveSelection,
    PerformMove,
    Busy,
    PartyScreen,
    BattleOver
}

public class BattleSystem : MonoBehaviour
{
    [Header("Player Pokemon")] 
    [SerializeField] private BattleUnit playerUnit;
    // [SerializeField] private BattleHud playerHud;

    [Header("Wild Pokemon")] 
    [SerializeField] private BattleUnit wildUnit;
    // [SerializeField] private BattleHud wildHud;

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

        currentCoroutine = null;
        
        if (currentCoroutine == null)
            currentCoroutine = StartCoroutine(SetupBattle());
    }

    IEnumerator SetupBattle(string dialog = "")
    {
        Debug.Log("Setting Up ...");

        playerUnit.Setup(playerParty.GetHealthyPokemon());
        // playerHud.SetData(playerUnit._Pokemon);

        wildUnit.Setup(wildPokemon);
        // wildHud.SetData(wildUnit._Pokemon);
        Debug.Log("Pokemons Choosed ...");
        
        _partyScreen.Init();

        yield return TransitionManager._instance.TransitionEffect_FadeOut();
        
        dialogBox.SetMoveNames(playerUnit._Pokemon.Moves);
        
        string _dialog = $"A wild {wildUnit._Pokemon.Base.Name} appeared!";

        yield return dialogBox.WriteType(_dialog);

        // yield return StartCoroutine(ActionSelection_Interaction());
        yield return ChooseFirstTurn();
        currentCoroutine = null;
    }

    IEnumerator ChooseFirstTurn()
    {
        if (playerUnit._Pokemon.Speed >= wildUnit._Pokemon.Speed)
        {
            yield return StartCoroutine(ActionSelection_Interaction());
            print($"{playerUnit._Pokemon.Base.Name} go first");            
        }
        else
        {
            yield return EnemyMove();
            print($"{playerUnit._Pokemon.Base.Name} go first");            
        }
    }
    
    IEnumerator BattleOver()
    {
        state = BattleState.BattleOver;
        yield return TransitionManager._instance.TransitionEffect_FadeIn();
        playerParty.PokemonTeam.ForEach(p=>p.OnBattleOver());
        OnFinishBattle?.Invoke();
    }
    
    #endregion

    public void HandleUpdate()
    {
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
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
        yield return BattleOver();
        this.gameObject.SetActive(false);
        currentCoroutine = null;
    }

    IEnumerator PerformPlayerMove()
    {
        state = BattleState.PerformMove;
        playerUnit._Pokemon.PP_Move(currentMove_Index);
        var move = playerUnit._Pokemon.Moves[currentMove_Index];
        dialogBox.UpdateMovePP(move);

        yield return RunMove(playerUnit, wildUnit, move);

        print("Coroutine Cleared - Performed Move");
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
        state = BattleState.PerformMove;
        var move = wildUnit._Pokemon.GetRandomMove();

        yield return RunMove(wildUnit, playerUnit, move);
    }

    // ReSharper disable Unity.PerformanceAnalysis
    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        print("Running Move");
        yield return dialogBox.WriteType(
            $"{sourceUnit._Pokemon.Base.Name} used {move.Base.Name}");

        yield return sourceUnit.PlayAttackAnimation();
        yield return targetUnit.PlayHitAnimation();

        if (move.Base.Category == MoveCategory.Status)
        {
            yield return RunMoveEffects(move, sourceUnit._Pokemon, targetUnit._Pokemon);
        }
        else
        {
            var damageDetails = targetUnit._Pokemon.TakeDamage(move, sourceUnit._Pokemon);
            yield return targetUnit.Hud.UpdateHP();
            yield return ShowDamageDetails(damageDetails);
        }
        
        
        if (targetUnit._Pokemon.HP <= 0)
        { 
            yield return dialogBox.WriteType(
                $"{targetUnit._Pokemon.Base.Name} Fainted!");
            
            yield return targetUnit.PlayFaintAnimation();
            yield return new WaitUntil(() => targetUnit.endAnimation);

            yield return CheckForBattleOver(targetUnit);

            if (sourceUnit.IsPlayerUnit)
            {
                print("Coroutine Cleared! - Enemy Fainted");                
                currentCoroutine = null;
            }
        }
        else
        {
            if (state == BattleState.PerformMove)
            {
                if (sourceUnit.IsPlayerUnit)
                {
                    yield return EnemyMove();
                }
                else if(!sourceUnit.IsPlayerUnit)
                {
                    yield return ActionSelection_Interaction();
                }
            }
        }
    }

    IEnumerator RunMoveEffects(Move move, Pokemon source, Pokemon target)
    {
        var effects = move.Base.Effects;
        if (effects.Boosts != null)
        {
            if (move.Base.Target == MoveTarget.Self)
                source.ApplyBoosts(move.Base.Effects.Boosts);
            else
                target.ApplyBoosts(move.Base.Effects.Boosts);
        }
            
        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }
    
    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while (pokemon.statusChanges.Count > 0)
        {
            var message = pokemon.statusChanges.Dequeue();
            yield return dialogBox.WriteType(message);
        }
    }

    IEnumerator CheckForBattleOver(BattleUnit faintedUnit)
    {
        //If an enemy pokemon faints our pokemon
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
                OpenPartyScreen();
            else
                yield return BattleOver();
        }
        //If our pokemon faints enemy
        else
        {
            yield return BattleOver();
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
                currentCoroutine = StartCoroutine(ActionSelection_Interaction(true,false));
        }
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        bool currentPokemonFainted = true;
        
        if (playerUnit._Pokemon.HP > 0)
        {
            currentPokemonFainted = false;
            yield return dialogBox.WriteType($"Come back {playerUnit._Pokemon.Base.Name}");
            yield return playerUnit.PlayFaintAnimation();
        }
        
        playerUnit.Setup(newPokemon);
        dialogBox.SetMoveNames(newPokemon.Moves);
        yield return dialogBox.WriteType($"Go {playerUnit._Pokemon.Base.Name}, I choose you!");

        if (currentPokemonFainted)
            yield return ChooseFirstTurn();
        else
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
                    MoveSelection_Interaction();
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
                currentCoroutine = StartCoroutine(ActionSelection_Interaction(true,false));
        }
    }

    IEnumerator ActionSelection_Interaction(bool end=false,bool isTypewrite=true)
    {
        if(isTypewrite)
            yield return dialogBox.WriteType("Choose an action");
        else
            dialogBox.SetDialog("Choose an action");
        state = BattleState.ActionSelection;
        dialogBox.EnableActionSelector(true);
        yield return new WaitForSeconds(.35f);
        if (end)
            currentCoroutine = null;
    }

    void MoveSelection_Interaction()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }
    
    #endregion
    
}