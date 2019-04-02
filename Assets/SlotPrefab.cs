using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotPrefab : MonoBehaviour
{
    public enum SlotTypes
    {
        fliese = 0,
        plug = 1,
        pepe = 2,
        ofen = 3,
        ban = 4,
        nyan = 5,
        ball = 6,
        reich = 7,
        pr0mium = 8,
        pr0gramm = 9,
    }

    public int type = (int)SlotTypes.fliese;
}
