using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class HPBar : MonoBehaviour
{
    [SerializeField] private Transform health;

    public void SetHP(float hpNormalized)
    {
        health.localScale = new Vector3(hpNormalized, 1f);
    }

    public IEnumerator SetHPSmooth(float newHP)
    {
        float curHP = health.transform.localScale.x;
        float changeAmt = curHP - newHP;

        while (curHP - newHP > Mathf.Epsilon)
        {
            curHP -= changeAmt * Time.deltaTime;
            health.transform.localScale = new Vector3(curHP, 1f);
            yield return null;
        }

        health.transform.localScale = new Vector3(newHP, 1f);
    }
}
