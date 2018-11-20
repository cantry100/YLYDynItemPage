using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// ugui点击事件监听器
/// author: 雨Lu尧
/// </summary>
[AddComponentMenu("UI/Event/UIClickListener")]
public class UIClickListener : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public Callback<UIClickListener> onClick;
    public float dragThreshold = 10f;

    protected PointerEventData pointerEventData;
    protected object param;

    private bool isDrag = false;
    private Vector2 pointDownPos;
    
    public void OnPointerClick(PointerEventData eventData)
	{
        this.pointerEventData = eventData;
        if (!isDrag)
        {
            if (onClick != null)
            {
                onClick(this);
            }
        }
	}

    public void OnPointerDown(PointerEventData eventData)
    {
        isDrag = false;
        pointDownPos = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        float dragDis = Vector2.Distance(pointDownPos, eventData.position);
        if (dragDis > dragThreshold)
        {
            isDrag = true;
        }
    }

    public void SetClick(Callback<UIClickListener> onClickCb, object param = null)
    {
        this.param = param;
        this.onClick = onClickCb;
    }
    
    void OnDestroy()
    {
        this.onClick = null;
    }
}