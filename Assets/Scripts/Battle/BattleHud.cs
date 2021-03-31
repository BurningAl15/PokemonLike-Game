using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleHud : MonoBehaviour
{
   [SerializeField] private TextMeshProUGUI nameText;
   [SerializeField] private TextMeshProUGUI levelText;
   [SerializeField] private HPBar hpBar;

   private Pokemon _pokemon;

   // private Coroutine currentCoroutine = null;
   
   public void SetData(Pokemon pokemon)
   {
      if (_pokemon == null)
         _pokemon = pokemon;
      nameText.text = _pokemon.Base.Name;
      levelText.text = "Lvl " + _pokemon.Level;
      hpBar.SetHP((float) _pokemon.MaxHP/_pokemon.MaxHP);
   }

   public IEnumerator UpdateHP()
   {
      yield return hpBar.SetHPSmooth((float) _pokemon.HP / _pokemon.MaxHP);
   }

   // public IEnumerator UpdateHP()
   // {
   //    float targetHP = (float) _pokemon.HP / _pokemon.MaxHP;
   //    float initialHP= (float) _pokemon.OldHP / _pokemon.MaxHP;
   //    
   //    for (float i = initialHP; i > targetHP; i -= 0.01f)
   //    {
   //       hpBar.SetHP(i);
   //       yield return null;
   //    }
   //
   //    _pokemon.UpdateOldHP();
   //    // currentCoroutine = null;
   // }
}
