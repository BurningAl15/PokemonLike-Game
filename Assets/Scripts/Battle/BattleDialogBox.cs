using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] private int lettersPerSecond;
    
    [FormerlySerializedAs("dialogBox")] [SerializeField] private TextMeshProUGUI dialogText;

    [SerializeField] private GameObject actionSelector;
    [SerializeField] private GameObject moveSelector;
    [SerializeField] private GameObject moveDetails;

    [SerializeField] private List<TextMeshProUGUI> actionTexts;
    [SerializeField] private List<TextMeshProUGUI> moveTexts;

    [SerializeField] private TextMeshProUGUI ppText;
    [SerializeField] private TextMeshProUGUI typeText;
    

    public void SetDialog(string dialog)
    {
        dialogText.text = dialog;
    }
    
    public IEnumerator WriteType(string dialog)
    {
        dialogText.text = "";

        for (int i = 0; i < dialog.Length; i++)
        {
            dialogText.text += dialog[i];
            yield return new WaitForSeconds(1/lettersPerSecond);
        }
        
        yield return new WaitForSeconds(1f);
    }

    public void EnableDialogText(bool enabled)
    {
        dialogText.enabled = enabled;
    }

    public void EnableActionSelector(bool enabled)
    {
        actionSelector.SetActive(enabled) ;
    }
    
    public void EnableMoveSelector(bool enabled)
    {
        moveSelector.SetActive(enabled);
        moveDetails.SetActive(enabled);
    }

    public void UpdateActionSelection(int selectedAction)
    {
        for (int i = 0; i < actionTexts.Count; ++i)
        {
            if (i == selectedAction)
                actionTexts[i].color = ColorManager._instance.GetColorValue(ColorKey.Hightlight);
            else
                actionTexts[i].color = ColorManager._instance.GetColorValue(ColorKey.Normal);
        }
    }

    public void SetMoveNames(List<Move> moves)
    {
        for (int i = 0; i < moveTexts.Count; ++i)
        {
            if (i < moves.Count)
                moveTexts[i].text = moves[i].Base.Name;
            else
            {
                moveTexts[i].text = "-";
            }
        }
    }

    public void UpdateMoveSelection(int selectedMove, Move move)
    {
        for (int i = 0; i < moveTexts.Count; ++i)
        {
            if (i == selectedMove)
                moveTexts[i].color = ColorManager._instance.GetColorValue(ColorKey.Hightlight);
            else
                moveTexts[i].color = ColorManager._instance.GetColorValue(ColorKey.Normal);
        }

        ppText.text = $"PP {move.PP}/{move.Base.Pp}";
        typeText.text = move.Base.Type.ToString();
    }

    
    public void UpdateMovePP(Move move)
    {
        ppText.text = $"PP {move.PP}/{move.Base.Pp}";
        typeText.text = move.Base.Type.ToString();
    }
}
