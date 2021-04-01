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
    
    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;
        nameText.text = _pokemon.Base.Name;
        levelText.text = "Lvl " + _pokemon.Level;
        hpBar.SetHP((float) _pokemon.HP/_pokemon.MaxHP);
    }

    public void SetSelected(bool selected)
    {
        if (selected)
            nameText.color = ColorManager._instance.GetColorValue(ColorKey.Hightlight);
        else
            nameText.color = ColorManager._instance.GetColorValue(ColorKey.Normal);
    }
}
