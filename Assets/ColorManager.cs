using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ColorKey
{
    Hightlight,Normal
}

[Serializable]
public class ColorClassifier
{
    public Color color;
    [TextArea]
    public string description;
}

public class ColorManager : MonoBehaviour
{
    public static ColorManager _instance;
    
    [SerializeField] private List<ColorClassifier> _colors = new List<ColorClassifier>();
    [SerializeField] private Dictionary<ColorKey, Color> _colorsDictionary = new Dictionary<ColorKey, Color>();

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        
        for (int i = 0; i < _colors.Count; i++)
        {
            _colorsDictionary.Add((ColorKey) i ,_colors[i].color);
        }
        
        //Debug
        // for (int i = 0; i < _colorsDictionary.Count; i++)
        // {
        //     print(_colorsDictionary[(ColorKey) i]);
        // }
    }

    public Color GetColorValue(int colorKeyIndex)
    {
        Color temp=Color.black;
        if (_colorsDictionary.TryGetValue((ColorKey) colorKeyIndex, out temp))
        {
            return temp;
        }
        
        print("Error In Color Manager - GetColorValue Method");
        return Color.magenta;
    }
    
    public Color GetColorValue(ColorKey colorKey)
    {
        Color temp=Color.black;
        if (_colorsDictionary.TryGetValue(colorKey, out temp))
        {
            return temp;
        }
        
        print("Error In Color Manager - GetColorValue Method");
        return Color.magenta;
    }
}
