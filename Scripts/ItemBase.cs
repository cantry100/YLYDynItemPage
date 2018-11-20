using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// item抽象类
/// author: 雨Lu尧
/// </summary>
public abstract class ItemBase : MonoBehaviour
{
    public bool isIgnoreLayout = false;
    public bool receiveClick = true;
    public Callback<ItemBase, bool> onClick = null;
    public Callback<ItemBase> onDataChange = null;
    protected bool isSelect = false;
    protected int dataIndex = -1;
    protected object data = null;
    protected RectTransform cacheRectTransform = null;
    protected Graphic cacheGraphic = null;

    public RectTransform CacheRectTransform
    {
        get
        {
            if (cacheRectTransform == null)
            {
                cacheRectTransform = this.GetComponent<RectTransform>();
            }
            return cacheRectTransform;
        }
    }

    public Graphic CacheGraphic
    {
        get
        {
            if (cacheGraphic == null)
            {
                cacheGraphic = this.GetComponent<Graphic>();
            }
            return cacheGraphic;
        }
    }

    public int Id { get; protected set; }

    public object Data
    {
        get
        {
            return data;
        }
    }

    public int DataIndex
    {
        get
        {
            return dataIndex;
        }
        set
        {
            dataIndex = value;
        }
    }
    
    public bool IsSelect
    {
        get
        {
            return isSelect;
        }
        set
        {
            if (isSelect == value)
            {
                return;
            }

            isSelect = value;
            OnSelectChange();
        }
    }
    
    protected void Awake()
    {
        if (receiveClick)
        {
            UIClickListener uiClickListener = this.GetComponent<UIClickListener>() ?? this.gameObject.AddComponent<UIClickListener>();
            uiClickListener.onClick = OnClick;
        }
    }
    
    public virtual void Init(object data)
    {
        if (this.data != null && this.data == data)
        {
            return;
        }
        
        this.data = data;
        OnDataChange();
    }

    public T CastData<T>()
    {
        if (this.data is T)
        {
            return (T)this.data;
        }
        return default(T);
    }

    public void ClickSelf()
    {
        if (this.onClick != null)
        {
            this.onClick(this, false);
        }
    }

    protected virtual void OnClick(UIClickListener ue)
    {
        if (this.onClick != null)
        {
            this.onClick(this, true);
        }
    }

    protected virtual void OnSelectChange()
    {
        
    }

    protected virtual void OnDataChange()
    {
        if (onDataChange != null)
        {
            onDataChange.Invoke(this);
        }
    }
    
    IEnumerator DelayedSetDirty(RectTransform rectTransform)
    {
        yield return null;
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }

    public virtual void Dispose()
    {
        this.data = null;
        this.onClick = null;
        this.onDataChange = null;
        this.dataIndex = -1;
    }

    protected virtual void OnDestroy()
    {
        Dispose();
    }
}
