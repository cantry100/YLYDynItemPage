using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// ui动态加载item页组件
/// 支持横向、纵向列表
/// email: cantry100@163.com
/// author: 雨Lu尧
/// </summary>
public class YLYDynItemPage : UIBehaviour
{
    public GameObject itemPrefab = null; //item预设体
    [Tooltip("当滚动列表为纵向时表示列数，当滚动列表为横向时表示行数")]
    public int column = 2; //当滚动列表为纵向时表示列数，当滚动列表为横向时表示行数
    public int itemNumPerPage = 8; //每页显示多少个item
    public RectOffset padding;
    public Vector2 cellSize = new Vector2(100f, 100f);
    public bool defSelectFirst = true;
    public bool useTweenAnim = true;
    
    [NonSerialized]
    public Action<ItemBase> OnAddItemCb = null;
    
    GameObject mGo;
    RectTransform m_Rect;
    RectTransform mTrans;
    ScrollRect scrollView = null;
    IEnumerator buildItemsIE = null;
    ItemBase[] items = null;
    
    int curDataIndex = -1;
    int lastDataIndex = -1;
    List<object> selectedDatas = null;
    object clickData = null;
    object[] datas = null;
    Bounds viewPortBounds = default(Bounds);
    Bounds contentBounds = default(Bounds);

    float lastPageTime;
    float oldElasticity = 0f;
    ContentSizeFitter contentSizeFitter = null;
    float cellWidthHalf;
    float cellHeightHalf;
    RectTransform relativeParentTrans = null;
    Stack<ItemBase> itemPool = new Stack<ItemBase>(); //ItemBase对象池
    
    public ScrollRect ScrollView
    {
        get
        {
            if (scrollView == null)
            {
                scrollView = this.GetComponentInParent<ScrollRect>();
            }
            return scrollView;
        }
    }

    public RectTransform RelativeParentTrans
    {
        get
        {
            if (relativeParentTrans == null)
            {
                if (ScrollView)
                {
                    relativeParentTrans = scrollView.GetComponent<RectTransform>();
                }
                else
                {
                    relativeParentTrans = this.GetComponentInParent<RectTransform>();
                }
            }
            return relativeParentTrans;
        }
    }

    public GameObject cachedGameObject { get { if (mGo == null) mGo = gameObject; return mGo; } }

    public RectTransform cachedTransform { get { if (mTrans == null) mTrans = this.GetComponent<RectTransform>(); return mTrans; } }
    
    public RectTransform rectTransform
    {
        get
        {
            if (m_Rect == null)
                m_Rect = GetComponent<RectTransform>();
            return m_Rect;
        }
    }
    
    public ItemBase[] Items
    {
        get
        {
            if (items == null)
            {
                items = this.GetComponentsInChildren<ItemBase>(false);
            }
            return items;
        }
    }
    
    protected override void Awake()
    {
        base.Awake();
        
        rectTransform.anchorMin = Vector2.up;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = Vector2.up;
        
        contentSizeFitter = this.GetComponent<ContentSizeFitter>();

        cellWidthHalf = this.cellSize.x / 2f;
        cellHeightHalf = this.cellSize.y / 2f;
        
        if (this.ScrollView && this.scrollView.horizontal)
        {
            float height = padding.top + padding.bottom + column * cellSize.y;
            rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)1, height);
        }
        else
        {
            float width = padding.left + padding.right + column * cellSize.x;
            rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)0, width);
        }

        contentBounds = UGUIExtend.CalculateRelativeRectTransformBounds(RelativeParentTrans, rectTransform);
        
        if (scrollView)
        {
            oldElasticity = scrollView.elasticity;
            viewPortBounds = UGUIExtend.CalculateRelativeRectTransformBounds(RelativeParentTrans, scrollView.viewport);
        }
    }
    
    /// <summary>
    /// 填充数据
    /// </summary>
    /// <param name="datas"></param>
    /// <param name="toTop"></param>
    /// <param name="unSelectAll"></param>
    public void FillDatas(object[] datas, bool toTop = true, bool unSelectAll = true)
    {
        this.Dispose();
        this.datas = datas;
        int count = this.datas != null ? this.datas.Length : 0;
        this.lastDataIndex = count - 1;

        if (unSelectAll)
        {
            UnSelectAllData();
        }

        this.StartBuildItems(0, toTop);
    }
    
    public void SelectData(object data)
    {
        if (data != null)
        {
            if(this.selectedDatas == null)
            {
                this.selectedDatas = new List<object>();
            }
            if (!this.selectedDatas.Contains(data))
            {
                this.selectedDatas.Add(data);
                ItemBase item = GetItemByData(data);
                if (item != null)
                {
                    item.IsSelect = true;
                }
            }
        }
    }

    public void UnSelectData(object data)
    {
        if (data != null)
        {
            if (this.selectedDatas != null)
            {
                if (this.selectedDatas.Contains(data))
                {
                    ItemBase item = GetItemByData(data);
                    if (item != null)
                    {
                        item.IsSelect = false;
                    }
                    this.selectedDatas.Remove(data);
                }
            }
        }
    }

    public void UnSelectAllData()
    {
        if (this.selectedDatas != null)
        {
            for (int i = 0; i< this.selectedDatas.Count; i++)
            {
                ItemBase item = GetItemByData(this.selectedDatas[i]);
                if (item != null)
                {
                    item.IsSelect = false;
                }
            }
            this.selectedDatas = null;
        }
    }

    public void ClickData(object data)
    {
        this.clickData = data;
        if (this.clickData != null)
        {
            ItemBase item = GetItemByData(this.clickData);
            if (item != null)
            {
                this.clickData = null;
                item.ClickSelf();
            }
        }
    }
    
    private void LateUpdate()
    {
        if (scrollView != null && scrollView.velocity != Vector2.zero)
        {
            float num = Time.realtimeSinceStartup - this.lastPageTime;
            if (num < 0.15f)
            {
                return;
            }
            this.lastPageTime = Time.realtimeSinceStartup;
            
            contentBounds = UGUIExtend.CalculateRelativeRectTransformBounds(RelativeParentTrans, rectTransform);
            
            if (scrollView.horizontal)
            {
                if ((int)contentBounds.max.x < (int)viewPortBounds.max.x)
                {
                    UpdatePage();
                }
            }
            else
            {
                if ((int)contentBounds.min.y > (int)viewPortBounds.min.y)
                {
                    UpdatePage();
                }
            }
        }
    }
    
    private void UpdatePage()
    {
        if (curDataIndex < lastDataIndex)
        {
            //内容区域底部或右边已经到达裁剪区域
            StartBuildItems(1);
        }
    }
    
    private void StartBuildItems(int direction, bool toTop = false)
    {
        if (this.buildItemsIE != null && this.buildItemsIE.MoveNext())
        {
            return;
        }
        
        this.StopBuildItems();
        this.buildItemsIE = this.OnBuildItems(direction, toTop);
        UIRoot.Instance.StartCoroutine(this.buildItemsIE);
    }

    public void StopBuildItems()
    {
        if (this.buildItemsIE != null)
        {
            UIRoot.Instance.StopCoroutine(this.buildItemsIE);
            this.buildItemsIE = null;
        }
    }
    
    private IEnumerator OnBuildItems(int direction, bool toTop = false)
    {
        if (this.datas != null)
        {
            if (direction < 0)
            {
                yield break;
            }

            int count = this.datas.Length;
            if (count == 0 || curDataIndex >= lastDataIndex)
            {
                yield break;
            }
            
            float posStartX = this.padding.left + cellWidthHalf;
            float posStartY = -(this.padding.top + cellHeightHalf);
            Vector3 pos = Vector3.zero;
            float posDeltaX = 0f;
            float posDeltaY = 0f;
            if (this.scrollView && this.scrollView.horizontal)
            {
                posDeltaX = this.cellSize.x;
                posDeltaY = -this.cellSize.y;
            }
            else
            {
                posDeltaX = this.cellSize.x;
                posDeltaY = -this.cellSize.y;
            }

            if (contentSizeFitter)
            {
                contentSizeFitter.enabled = false;
            }
            
            int xRemainder;
            int yDivisor;
            int buildItemNum = 0;
            int buildItemMax = 0;
            
            if (this.scrollView != null)
            {
                this.scrollView.elasticity = 0f; //屏蔽回弹效果
                buildItemMax = direction == 0 ? (2 * this.itemNumPerPage) : this.itemNumPerPage;
            }
            else
            {
                buildItemMax = count;
            }
            buildItemMax = Mathf.Min(lastDataIndex - curDataIndex, buildItemMax);
            
            contentBounds = UGUIExtend.CalculateRelativeRectTransformBounds(RelativeParentTrans, rectTransform);
            
            if (this.scrollView && this.scrollView.horizontal)
            {
                float newWidth = contentBounds.size.x + Mathf.CeilToInt((float)buildItemMax / column) * this.cellSize.x;
                rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)0, newWidth);
            }
            else
            {
                float newHeight = contentBounds.size.y + Mathf.CeilToInt((float)buildItemMax / column) * this.cellSize.y;
                rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)1, newHeight);
            }

            while (buildItemNum < buildItemMax)
            {
                if (curDataIndex >= lastDataIndex)
                {
                    break;
                }

                buildItemNum++;
                curDataIndex++;
                xRemainder = curDataIndex % column;
                yDivisor = Mathf.FloorToInt((float)curDataIndex / column);
                
                object hd = this.datas[curDataIndex];
                ItemBase item = this.GetItem(curDataIndex);
                if (item == null)
                {
                    Debug.LogError("item的预设体上找不到ItemBase相关组件！！！");
                }
                
                if (this.scrollView && this.scrollView.horizontal)
                {
                    pos.x = posStartX + yDivisor * posDeltaX;
                    pos.y = posStartY + xRemainder * posDeltaY;
                }
                else
                {
                    pos.x = posStartX + xRemainder * posDeltaX;
                    pos.y = posStartY + yDivisor * posDeltaY;
                }
                
                RectTransform itemRectTransform = item.CacheRectTransform;
                itemRectTransform.anchoredPosition3D = pos;

                item.Init(hd);
                item.DataIndex = curDataIndex;
                OnAddItem(item);

                if (selectedDatas != null && selectedDatas.Count > 0)
                {
                    int j = 0;
                    while (j < this.selectedDatas.Count)
                    {
                        object ihs = selectedDatas[j];
                        if ((hd != null) && (ihs == hd))
                        {
                            item.IsSelect = true;
                            break;
                        }
                        j++;
                    }
                }
                else if (clickData != null && clickData == hd)
                {
                    clickData = null;
                    item.ClickSelf();
                }
                else if (selectedDatas == null && defSelectFirst && curDataIndex == 0)
                {
                    item.ClickSelf();
                }

                if (useTweenAnim)
                {
                    Graphic graphic = item.CacheGraphic;
                    if (graphic)
                    {
                        graphic.SetAlpha(0f);
                        TweenAlpha.Begin(itemRectTransform.gameObject, 0.2f, 1f);
                    }
                    yield return new WaitForSeconds(0.03f);
                }
                else
                {
                    yield return null;
                }
                
                if (toTop)
                {
                    if (this.scrollView)
                    {
                        if (this.scrollView.horizontal)
                        {
                            this.scrollView.horizontalNormalizedPosition = 0f;
                        }
                        else
                        {
                            this.scrollView.verticalNormalizedPosition = 1f;
                        }
                    }
                    toTop = false;
                }
            }

            if (contentSizeFitter)
            {
                contentSizeFitter.enabled = true;
            }

            yield return null;

            if (this.scrollView)
            {
                this.scrollView.elasticity = oldElasticity; //重置回弹效果
            }
        }

        StopBuildItems();
    }
    
    void OnAddItem(ItemBase item)
    {
        items = null; //设置为null，使其重新获取
        if (OnAddItemCb != null)
        {
            OnAddItemCb(item);
        }
    }

    public ItemBase GetItemByData(object data)
    {
        ItemBase[] items = Items;
        if (items != null)
        {
            for (int i = 0; i < items.Length; i++)
            {
                ItemBase item = items[i];
                if (item != null && item.Data == data)
                {
                    return item;
                }
            }
        }
        return null;
    }

    public ItemBase GetItemByDataIndex(int dataIndex)
    {
        ItemBase[] items = Items;
        if (items != null)
        {
            for (int i = 0; i < items.Length; i++)
            {
                ItemBase item = items[i];
                if (item != null && item.DataIndex == dataIndex)
                {
                    return item;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 从对象池中获取对象复用，如果对象池中没有，则生成
    /// </summary>
    /// <returns></returns>
    ItemBase GetItem(int dataIndex)
    {
        ItemBase item;
        if (itemPool.Count == 0)
        {
            GameObject itemGo = GameObject.Instantiate(itemPrefab);
            itemGo.name = "item_" + dataIndex.ToString();
            itemGo.transform.SetParent(this.cachedTransform, false);
            item = itemGo.GetComponent<ItemBase>();

            //item的锚点设置为左上角
            RectTransform itemRectTransform = item.CacheRectTransform;
            itemRectTransform.anchorMin = Vector2.up;
            itemRectTransform.anchorMax = Vector2.up;
            //itemRectTransform.sizeDelta = cellSize;
        }
        else
        {
            item = itemPool.Pop();
            item.gameObject.name = "item_" + dataIndex.ToString();
            item.gameObject.SetActive(true);
        }
        
        return item;
    }

    /// <summary>
    /// 把item释放回对象池中
    /// </summary>
    /// <param name="items"></param>
    void ReleaseItem(ItemBase[] items)
    {
        if (items != null)
        {
            for (int i = 0; i < items.Length; i++)
            {
                ItemBase item = items[i];
                this.ReleaseItem(item);
            }
        }
    }
    
    void ReleaseItem(ItemBase item)
    {
        if (item)
        {
            if (itemPool.Count > 0 && ReferenceEquals(itemPool.Peek(), item))
            {
                return;
            }

            item.gameObject.SetActive(false);
            item.Dispose();
            itemPool.Push(item);
        }
    }

    public void Dispose()
    {
        this.StopBuildItems();
        this.datas = null;
        this.curDataIndex = -1;
        this.lastDataIndex = -1;
        if (this.scrollView)
        {
            if (this.scrollView.horizontal)
            {
                this.scrollView.horizontalNormalizedPosition = 0f;
            }
            else
            {
                this.scrollView.verticalNormalizedPosition = 1f;
            }
        }
        this.ReleaseItem(this.Items);
        items = null;
        if (this.scrollView && this.scrollView.horizontal)
        {
            rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)0, 0f);
        }
        else
        {
            rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)1, 0f);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        StopBuildItems();
        datas = null;
        items = null;
        itemPrefab = null;
        itemPool.Clear();
        selectedDatas = null;
        OnAddItemCb = null;
        clickData = null;
    }
}
