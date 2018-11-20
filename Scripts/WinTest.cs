using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinTest : MonoBehaviour
{
    public YLYDynItemPage dynItemPage;

	// Use this for initialization
	void Start () {
        object[] datas = new object[200];
        for (int i = 0, n = datas.Length; i < n; i++)
        {
            datas[i] = i;
        }

        dynItemPage.FillDatas(datas);
    }
}
