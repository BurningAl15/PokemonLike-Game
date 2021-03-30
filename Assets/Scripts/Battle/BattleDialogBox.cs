using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] private int lettersPerSecond;
    [SerializeField] private Color highlightedColor;
    
    [FormerlySerializedAs("dialogBox")] [SerializeField] private TextMeshProUGUI dialogText;

    [SerializeField] private GameObject actionSelector;
    [SerializeField] private GameObject moveSelector;
    [SerializeField] private GameObject moveDetails;

    [SerializeField] private List<TextMeshProUGUI> actionTexts;
    [SerializeField] private List<TextMeshProUGUI> moveTexts;

    [SerializeField] private TextMeshProUGUI ppText;
    [SerializeField] private TextMeshProUGUI typeText;
    
    
    public IEnumerator WriteType(string dialog)
    {
        dialogText.text = "";

        for (int i = 0; i < dialog.Length; i++)
        {
            dialogText.text += dialog[i];
            yield return new WaitForSeconds(1/lettersPerSecond);
        }
        
        yield return null;
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
                actionTexts[i].color = highlightedColor;
            else
                actionTexts[i].color=Color.black;
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
                moveTexts[i].color = highlightedColor;
            else
                moveTexts[i].color=Color.black;
        }

        ppText.text = $"PP {move.PP}/{move.Base.Pp}";
        typeText.text = move.Base.Type.ToString();
    }
}
