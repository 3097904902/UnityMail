using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollEvent : MonoBehaviour
{
    public ScrollRect scrollRect;
    public MidPanelController midPanelController;
    public MidPanelView midPanelView;
    private void Awake()
    {
       
    }
    void Start()
    {
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.AddListener(OnScroll);//监听滚动事件
        }
    }

    void OnScroll(Vector2 position)
    {
        // 这里处理滚动事件
        //Debug.Log("滚动位置: " + position);
        int totalItems = MailBox.Instance.mailList.Count;
        float itemHeight = midPanelController.currentItemHeight + midPanelController.Spacing; //
        float viewportHeight = scrollRect.viewport.rect.height;
        float contentHeight = scrollRect.content.rect.height;// 或直接使用scrollRect.content.rect.height
        
        int index = CalculateStartIndexFromPosition(position, totalItems, itemHeight, viewportHeight, contentHeight);
        //接受的是当前可视范围内第一个的索引
        midPanelController.OnScrollChanged();
        midPanelController.UpdateVisibleMailItems(index);//更新内容

    }
    int CalculateStartIndexFromPosition(Vector2 position, int totalItems, float itemHeight, float viewportHeight, float contentHeight)
    {
        // position.y 是ScrollRect的垂直归一化位置，0表示在底部，1表示在顶部
        // 注意Unity UI的滚动方向：向下滚动时position.y减小，向上滚动时position.y增大
        // 计算当前视口顶部相对于内容顶部的距离
        float contentTopToViewportTop = (1f - position.y) * (contentHeight - viewportHeight);
        //Debug.Log(position);
        // 根据当前的滚动位置计算startIndex
        int startIndex = Mathf.FloorToInt(contentTopToViewportTop / itemHeight);
        // 确保startIndex加上可见项数量不会超出邮件列表的总数
        startIndex = Mathf.Clamp(startIndex, 0, Mathf.Max(0, totalItems - midPanelController.visableItemCount));
        midPanelController.mailstartIndex = startIndex;
        midPanelController.endIndex = startIndex + midPanelController.visableItemCount;
        Debug.Log("startIndex"+startIndex.ToString());
        return startIndex;
    }//startindex是第一个要显示的索引
    
}