using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private HPBar hpBar;

    private Pokemon _pokemon;

    // private Coroutine currentCoroutine = null;
   
    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;
        nameText.text = _pokemon.Base.Name;
        levelText.text = "Lvl " + _pokemon.Level;
        hpBar.SetHP((float) _pokemon.HP/_pokemon.MaxHP);
    }
}
