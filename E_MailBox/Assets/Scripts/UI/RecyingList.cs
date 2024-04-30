using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RecyingList : MonoBehaviour
{
    public List<MailItem> MailItemList;
    public ScrollRect scrollRect;
    public MidPanel midPanel;
    public GameObject Mail;
    public RectTransform MailRect;
    [Header("邮件具体信息")]
    public float itemSpacing = 10f;//item之间的间隔
    public float currentItemHeight;//当前邮件的高度
    public float currentItemWidth;//当前邮件的宽度
    public float currentContentSize;
    [Header("邮件的索引")]
    public int mailstartIndex;
    public int currentStartIndex;
    public int visiableEndIndex;
    public int endIndex;
    [Header("邮件的实例个数")]
    public int ListItemCount;//设置的列表实例的个数
    public int VisableCount;//在视图里可见的列表个数
    [Header("视口的高度")]
    public float viewportHeight;
    [Header("当前邮件的个数")]
    public int MailCounts;//邮件的个数
    [Header("高度相关")]
    public float PreAllocHeight = 0;//事先预留的高度
    protected float previousBuildHeight = 0;//之前的高度
    [Tooltip("是否执行滚动变化更新")]                                 
    public bool ignoreScrollChange;
    public event UnityAction<MailData,MailItem> OnSetMailItem;//设置邮件的具体数据和显示
    private void OnEnable()
    {
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.AddListener(OnScroll);//监听滚动事件
        }

    }
    private void Start()
    {
        MailCounts = MailBox.Instance.mailList.Count;
    }
    private void OnDisable()
    {
        scrollRect.onValueChanged.RemoveListener(OnScroll);
    }
    public void UpdateContentSize(int MailCounts, float height) //调整Content的高度
    {
        currentContentSize = height * MailCounts + (MailCounts - 1) * (itemSpacing);
        scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, currentContentSize);
    }
    public void InitRecyingList() 
    {
        //实例化的时候设置好位置，根据邮件的index设置好位置和高度
        for (int i = 0; i < ListItemCount; i++)
        {
            var obj = Instantiate(Mail);
            obj.transform.SetParent(scrollRect.content, false);
            //设置位置
            float posY = -i * (currentItemHeight + itemSpacing);
            //设置大小
            RectTransform rect = obj.gameObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(currentItemWidth, currentItemHeight);
            rect.offsetMin = new Vector2(0, rect.offsetMin.y);
            rect.offsetMax = new Vector2(0, rect.offsetMax.y);
            obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, posY);
            MailItem mailItem = obj.GetComponent<MailItem>();
            MailItemList.Add(mailItem);
        }
        RefreshAll(); // 刷新
    }
    public void RefreshAll()
    {
        
        for (int i = 0; i < ListItemCount; i++)//意思是显示初始索引之后的可见的邮件
        {
            int mailIndex = mailstartIndex + i;
            if (mailIndex < MailBox.Instance.mailList.Count)
            {
                // 获取对应的邮件数据
                MailData mail = MailBox.Instance.mailList[mailIndex];
                // 获取对应的邮件项GameObject
                MailItem mailItem = MailItemList[i];
                // 更新邮件项以显示当前邮件数据,不仅更新邮件数据还要根据数据显示ui
                mailItem.gameObject.SetActive(true);
                RefreshItem(mail,MailItemList[i]);
            }
            else
            {
                // 如果当前索引超出邮件列表范围，隐藏多余的邮件项
               MailItemList[i].gameObject.SetActive(false);
            }
        }
        // 隐藏多余的邮件项
      
        UpdateContentSize(MailBox.Instance.mailList.Count, currentItemHeight);
    }
    public void RefreshItem(MailData mail,MailItem mailItem)//刷新指定的item
    {
        if (OnSetMailItem == null)
        {
            Debug.Log("RecyclingListView is missing an ItemCallback, cannot function", this);
            return;
        }
        else 
        {
            OnSetMailItem(mail,mailItem);//更新数据
            mailItem.gameObject.SetActive(true);
        }
    }
   
    public void SetviewportHeight(float height) 
    {
        viewportHeight = height;
    }
    public void setCellSize()
    {
        currentItemHeight = MailRect.rect.height;
        currentItemWidth = MailRect.rect.width;
    }
    public void SetCellNum(int listNum) 
    {
        ListItemCount = listNum;
    }
    public void SetItemSpace(float space){ 
        itemSpacing = space;
    }
    void OnScroll(Vector2 position)
    {
        CalculateStartIndexFromPosition(position, MailBox.Instance.mailList.Count, RowHeight(), viewportHeight, currentContentSize);
        // 如果startIndex增加，表示用户向下滚动
        if (mailstartIndex > currentStartIndex)
        {
            //Debug.Log("用户向下滚动");
            // 将现在上面的邮件项移动到最下面并且设置为初始状态
            for (int i = 0; i < mailstartIndex - currentStartIndex; i++)
                MoveTopItemToBottom();
        }
        // 如果startIndex减少，表示用户向上滚动
        else if (mailstartIndex < currentStartIndex)
        {
            //Debug.Log("用户向上滚动");
            // 将最下面的邮件项移动到最上面并且设置为初始状态
            for (int i = 0; i < currentStartIndex - mailstartIndex; i++)
                MoveBottomItemToTop();
        }
        // 更新当前startIndex为新的值
        currentStartIndex = mailstartIndex;
    }
    public void CalculateStartIndexFromPosition(Vector2 position, int totalItems, float itemHeight, float viewportHeight, float contentHeight)
    {
        // position.y 是ScrollRect的垂直归一化位置，0表示在底部，1表示在顶部
        // 注意Unity UI的滚动方向：向下滚动时position.y减小，向上滚动时position.y增大
        // 计算当前视口顶部相对于内容顶部的距离
        float contentTopToViewportTop = (1f - position.y) * (contentHeight - viewportHeight);
        int startIndex = Mathf.FloorToInt(contentTopToViewportTop / itemHeight);
        // 确保startIndex加上可见项数量不会超出邮件列表的总数
        startIndex = Mathf.Clamp(startIndex, 0, Mathf.Max(0, totalItems - ListItemCount));
        mailstartIndex = startIndex;
        Debug.Log("MailStartIndex" + mailstartIndex);
        endIndex = startIndex + ListItemCount - 1;
        visiableEndIndex = startIndex + VisableCount - 1;
    }//mailstartIndex是第一个要显示的索引
    public void MoveTopItemToBottom()
    {
        // 假设midPanelView.MailItemList存储了所有邮件项的引用
        MailItem topItem = MailItemList[0];//把当前最上面的元素移动到+邮件数的位置
        //记录当前的位置
        RectTransform topItemRect = topItem.GetComponent<RectTransform>();
        float curretntPosition = topItemRect.anchoredPosition.y;
        float newYposition = curretntPosition - currentItemHeight * ListItemCount - ListItemCount * itemSpacing;
        topItemRect.anchoredPosition = new Vector2(topItemRect.anchoredPosition.x, newYposition);
        MailItemList.RemoveAt(0);
        MailItemList.Add(topItem);
        int newIndex = mailstartIndex + ListItemCount - 1;
        if (newIndex < MailBox.Instance.mailList.Count)
        {
            MailData mail = MailBox.Instance.mailList[newIndex];
            RefreshItem(mail, topItem);
        }
        else
        {
            topItem.gameObject.SetActive(false);
        }
    }
    public void MoveBottomItemToTop()
    {
        MailItem bottomItem = MailItemList[ListItemCount - 1];
        RectTransform bottomItemRect = bottomItem.GetComponent<RectTransform>();
        float curretntPosition = bottomItemRect.anchoredPosition.y;
        float newYposition = curretntPosition + currentItemHeight * ListItemCount + ListItemCount * itemSpacing;
        bottomItemRect.anchoredPosition = new Vector2(bottomItemRect.anchoredPosition.x, newYposition);
        MailItemList.RemoveAt(ListItemCount - 1);
        MailItemList.Insert(0, bottomItem);
        // 更新移动后的邮件项的内容
        int newIndex = endIndex - ListItemCount + 1;
        if (newIndex >= 0 && newIndex < MailBox.Instance.mailList.Count)
        {
            MailData mail = MailBox.Instance.mailList[newIndex];
            RefreshItem(mail, bottomItem);
        }
        else
        {
            bottomItem.gameObject.SetActive(false);
        }
    }
    private float RowHeight()
    {
        return itemSpacing + currentItemHeight;//高度+间隔
    }
    public void SetVisableCount() 
    {
        int Count = Mathf.RoundToInt(viewportHeight / RowHeight());
        VisableCount = Count;
    }
}
