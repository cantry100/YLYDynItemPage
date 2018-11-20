using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemTest : ItemBase
{
    public Text num;

    protected override void OnDataChange()
    {
        base.OnDataChange();

        num.text = this.CastData<int>().ToString();
    }
}
