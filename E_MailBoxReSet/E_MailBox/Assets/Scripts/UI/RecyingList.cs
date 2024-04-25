using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
using UnityEngine.UI;

public class RecyingList : MonoBehaviour
{
    public List<MailItem> MailItemList;
    public ScrollRect scrollRect;
    public MidPanel midPanel;
    public GameObject Mail;
    public RectTransform MailRect;
    [Header("邮件具体信息")]
    public float itemSpacing;//item之间的间隔
    public float currentItemHeight;//当前邮件的高度
    public float currentItemWidth;//当前邮件的宽度
    public int mailstartIndex;
    public int currentStartIndex;
    public int visableItemCount;
    public int endIndex;
    public float viewportHeight;
    public float Offset;
    MailData LastSelectMail;
    MailData CurrentSelectMail;
    MailItem SelectMailItem;
    private void Awake()
    {
        visableItemCount = 8;
        MailBox.Instance.LoadMailsFromXml("mails.xml");     
    }
    private void Start()
    {
            MailBox.Instance.mailList = MailBox.Instance.SortMails(MailBox.Instance.mailList);
            Mail.gameObject.SetActive(false);
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.AddListener(OnScroll);//监听滚动事件
        }
        MailItem.OnClickedMailItem += ClickedMail;
        MailBox.Instance.OnLoadFromXML += updateFromData;
        MailBox.Instance.OnMailDelete += SetFirstSelect;
        MailBox.Instance.OnMailCount += ShowMailCount;
        MailBox.Instance.LoadIdCount();
        if (MailBox.Instance.mailList.Count <= 0)
        {
            midPanel.LeftNull.gameObject.SetActive(true);
            midPanel.RightNull.gameObject.SetActive(true);
            //没有邮件的时候设置为false
            midPanel.MailContent.SetActive(false);
        }
        else
        {
            midPanel.MailContent.SetActive(true);
            midPanel.LeftNull.gameObject.SetActive(false);
            midPanel.RightNull.gameObject.SetActive(false);
        }
        if (MailBox.Instance.mailList.Count >= 0) 
        {
            InitMidPanelView();
        }
        StartCoroutine(CheckAndDeleteExpiredMailsEveryHour());
    }

    private void ShowMailCount()
    {
        if (midPanel != null) 
        {
            midPanel.MailCountText.text = MailBox.Instance.mailList.Count.ToString() + "/" + MailBox.Instance.MailMaxCount.ToString();
        }
     
    }
    private IEnumerator CheckAndDeleteExpiredMailsEveryHour()
    {
        // 无限循环
        while (true)
        {
            // 执行检查和删除过期邮件的逻辑
            CheckAndDeleteExpiredMails();
            // 等待一个小时（3600秒）
            yield return new WaitForSeconds(3600f);
        }
    }
    private void CheckAndDeleteExpiredMails()
    {
        //定时检测邮件有没有过期
        foreach (MailData mail in MailBox.Instance.mailList)
        {
            TimeSpan timeSince = DateTime.Now - mail.sentTime;
            if (timeSince.Days >= 10) //超过10天过期删除
            {
                MailBox.Instance.RemoveMail(mail.mailId);
            }
        }
        Debug.Log("检查并删除过期邮件");
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            MailBox.Instance.InitMail();
        }
    }
    public void CloseAllSelect() 
    {
        foreach (MailData mail in MailBox.Instance.mailList) 
        {
            mail.isSelect = false;
        }
    }
    public void SetFirstSelect()
    {
        if (MailBox.Instance.mailList[0] != null)
        {
            CloseAllSelect();
            MailData maildata = MailBox.Instance.mailList[0]; ;
            CurrentSelectMail = maildata;
            MailBox.Instance.mailList[0].isSelect = true;
            LastSelectMail = maildata;
            SetRightMail(CurrentSelectMail, SelectMailItem);
            CurrentSelectMail.isSelect = true;
            CurrentSelectMail.isRead = true;
            SetisReady(maildata,SelectMailItem);
            SetisSelect(maildata, SelectMailItem);
            updateFromData();
        }

    }
    public void InitMidPanelView()
    {
        currentItemWidth = midPanel.ScrollViewContentRect.rect.width;
        viewportHeight = midPanel.ScrollViewRect.rect.height;
        itemSpacing = 10f;
        currentItemHeight = MailRect.rect.height;
        Debug.Log("currentItemHeight" + currentItemHeight);
        Offset = Mathf.Floor(currentItemHeight / 2);
        //实例化的时候设置好位置，根据邮件的index设置好位置和高度
        for (int i = 0; i < visableItemCount; i++)
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
            obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, posY );
            MailItem mailItem = obj.GetComponent<MailItem>();
            MailItemList.Add(mailItem);
        }
        SelectMailItem = MailItemList[0];
        midPanel.SelectMailItem = MailItemList[0];
        SetFirstSelect();
        //初始化
        UpdateVisibleMailItems(0); // 使用当前的startIndex更新邮件项内容
        AdjustContentSize(MailBox.Instance.mailList.Count, currentItemHeight); // 调整内容大小
    }
    void OnScroll(Vector2 position)
    {
        // 这里处理滚动事件
        //Debug.Log("滚动位置: " + position);
        int totalItems = MailBox.Instance.mailList.Count;
        float itemHeight = currentItemHeight + itemSpacing; //当前的item高度+间隔
        float viewportHeight = scrollRect.viewport.rect.height;//当前视图的高度
        float contentHeight = scrollRect.content.rect.height;// 或直接使用scrollRect.content.rect.height
        int index = CalculateStartIndexFromPosition(position, totalItems, itemHeight, viewportHeight, contentHeight);
        //接受的是当前可视范围内第一个的索引
        OnScrollChanged();
    }
    public void AdjustContentSize(int MailCounts, float height) //调整Content的高度
    {
        float totalHeight = height * MailCounts + (MailCounts - 1) * (itemSpacing);
        midPanel.ScrollViewContentRect.sizeDelta = new Vector2(midPanel.ScrollViewContentRect.sizeDelta.x, totalHeight);
    }
    private void OnScrollChanged()
    {
        // 如果startIndex增加，表示用户向下滚动
        if (mailstartIndex > currentStartIndex)
        {
            //Debug.Log("用户向下滚动");
            // 将现在上面的邮件项移动到最下面并且设置为初始状态
            for(int i = 0;i< mailstartIndex- currentStartIndex;i++)
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
    public void MoveTopItemToBottom()
    {
        // 假设midPanelView.MailItemList存储了所有邮件项的引用
        MailItem topItem = MailItemList[0];//把当前最上面的元素移动到+邮件数的位置
        //记录当前的位置
        RectTransform topItemRect = topItem.GetComponent<RectTransform>();
        float curretntPosition = topItemRect.anchoredPosition.y;
        float newYposition = curretntPosition - currentItemHeight * visableItemCount - visableItemCount * itemSpacing;
        topItemRect.anchoredPosition = new Vector2(topItemRect.anchoredPosition.x, newYposition);
        MailItemList.RemoveAt(0);
        MailItemList.Add(topItem);
        // 更新移动后的邮件项的内容
        UpdateVisibleMailItems(mailstartIndex);
    }
    public void MoveBottomItemToTop()
    {
        MailItem bottomItem = MailItemList[visableItemCount - 1];
        RectTransform bottomItemRect = bottomItem.GetComponent<RectTransform>();
        float curretntPosition = bottomItemRect.anchoredPosition.y;
        float newYposition = curretntPosition + currentItemHeight * visableItemCount + visableItemCount * itemSpacing;
        bottomItemRect.anchoredPosition = new Vector2(bottomItemRect.anchoredPosition.x, newYposition);
        MailItemList.RemoveAt(visableItemCount - 1);
        MailItemList.Insert(0, bottomItem);
        // 更新移动后的邮件项的内容
        int newMailIndex = currentStartIndex;
        UpdateVisibleMailItems(mailstartIndex);
    }
    public void ResetMailState(MailItem mailItem) 
    {
        SetNoTransparency(mailItem);
        NoSelectImage(mailItem);
        mailItem.Select.gameObject.SetActive(false);
        mailItem.isRead.gameObject.SetActive(false);
    }
    public void updateFromData() 
    {
        UpdateVisibleMailItems(mailstartIndex);
    }
    private void UpdateVisibleMailItems(int startIndex)
    {
        //根据index邮件项进行移动，移动之后再更新邮件的内容
        //i 5
        for (int i = 0; i < visableItemCount; i++)//意思是显示初始索引之后的可见的邮件
        {
            int mailIndex = startIndex + i;
            if (mailIndex < MailBox.Instance.mailList.Count)
            {
                // 获取对应的邮件数据
                MailData mail = MailBox.Instance.mailList[mailIndex];
                // 获取对应的邮件项GameObject
                MailItem mailItem = MailItemList[i];
                // 更新邮件项以显示当前邮件数据,不仅更新邮件数据还要根据数据显示ui
                mailItem.gameObject.SetActive(true);
                ResetMailState(MailItemList[i]);
                setMailItem(mail, mailItem);
            }
            else
            {
                // 如果当前索引超出邮件列表范围，隐藏多余的邮件项
               MailItemList[i].gameObject.SetActive(false);
            }
        }
        if (MailBox.Instance.mailList.Count <= 0)
        {
            midPanel.LeftNull.gameObject.SetActive(true);
            midPanel.RightNull.gameObject.SetActive(true);
            //没有邮件的时候设置为false
            midPanel.MailContent.SetActive(false);
        }
        else
        {
            midPanel.MailContent.SetActive(true);
            midPanel.LeftNull.gameObject.SetActive(false);
            midPanel.RightNull.gameObject.SetActive(false);
        }
        SetMidPanelUI(CurrentSelectMail);
        AdjustContentSize(MailBox.Instance.mailList.Count, currentItemHeight);
    }
    private void setMailItem(MailData mail, MailItem mailItem)
    {
        //把mailInfo中的对应的id赋值
        mailItem.Mailid = mail.mailId;
        mailItem.MailTitle.text = SetMailTitle(mail.mailTitle);
        mailItem.MailSendName.text = mail.sender;
        mailItem.SentTime = mail.sentTime;
        mailItem.MailTime.text = TimeAgo(mail.sentTime);
        SetisReady(mail, mailItem);
        SetisReward(mail, mailItem);
        SetisSelect(mail, mailItem);
    }
    private string TimeAgo(DateTime sentTime)
    {
        TimeSpan timeSince = DateTime.Now - sentTime;
        // 这里使用与前面相同的TimeAgo逻辑
        if (timeSince.TotalDays >= 1) return $"{(int)timeSince.TotalDays}天前";
        if (timeSince.TotalHours >= 1) return $"{(int)timeSince.TotalHours}小时前";
        if (timeSince.TotalMinutes >= 1) return $"{(int)timeSince.TotalMinutes}分钟前";
        return "刚刚";
    }
    public void SetisReady(MailData mail, MailItem mailItem)
    {
        if (mail.isRead)
        {
            //红点消失
            mailItem.isRead.gameObject.SetActive(false);
            SetTransparency(mailItem);//变透明
        }
        else
        {
            mailItem.isRead.gameObject.SetActive(true);
            SetNoTransparency(mailItem);//不透明
        }
    }
    public void SetTransparency(MailItem mailItem) //设置半透明
    {
        Color color = mailItem.Mail.color;
        color.a = 0.3f; // 设置透明度，0.5为半透明
        mailItem.Mail.color = color;
    }
    public void SetNoTransparency(MailItem mailItem) //设置不透明
    {
        //Debug.Log("设置透明度为1");
        Color color = mailItem.Mail.color;
        color.a = 1f; // 设置透明度，0.5为半透明
        mailItem.Mail.color = color;
    }
    public void SetisReward(MailData mail, MailItem mailItem)
    {
        bool hasReward = mail.isReward;
        mailItem.ImageReward.gameObject.SetActive(hasReward);
        mailItem.ImageMessage.gameObject.SetActive(!hasReward);

    }
    private void SetMidPanelUI(MailData mail)
    {
        bool hasReward = mail.isReward;
        bool isCompleted = mail.isCompleted;
        if (hasReward)
        {
            midPanel.btnDelete.gameObject.SetActive(isCompleted);
            midPanel.btnRecieve.gameObject.SetActive(!isCompleted);
            midPanel.attachment.gameObject.SetActive(hasReward);
        }
        else 
        {
            midPanel.btnDelete.gameObject.SetActive(true);
            midPanel.attachment.gameObject.SetActive(false);
            midPanel.btnRecieve.gameObject.SetActive(false);
        }
    }
    public void SetisSelect(MailData mail, MailItem mailItem)
    {
        
        if (mail.isSelect)
        {
            mailItem.Select.gameObject.SetActive(true);
            showSelectImage(mailItem);//变灰色

        }
        else
        {
            mailItem.Select.gameObject.SetActive(false);
        }
    }
    public void showSelectImage(MailItem mailItem)
    {
        mailItem.Mail.color = new Color(0.5f, 0.5f, 0.5f, 1);
    }
    public void NoSelectImage(MailItem mailItem)
    {
        mailItem.Mail.color = new Color(1f, 1f, 1f, 1);
    }
    public string SetMailTitle(string str)
    {
        if (str.Length < 12)
        {
            return str;
        }
        else
        {
            return str.Substring(0, 12) + "...";
        }
    }
    public void SetRightMail(MailData mail,MailItem mailItem)
    {
        midPanel.contentTitle.text = mail.mailTitle;
        midPanel.Time.text = mail.sentTime.ToString("yyyy-MM-dd HH:mm");
        midPanel.contentSendMessage.text = mail.sender;
        midPanel.contentBody.text = mail.mailContent;

    }
   
    int CalculateStartIndexFromPosition(Vector2 position, int totalItems, float itemHeight, float viewportHeight, float contentHeight)
    {
        // position.y 是ScrollRect的垂直归一化位置，0表示在底部，1表示在顶部
        // 注意Unity UI的滚动方向：向下滚动时position.y减小，向上滚动时position.y增大
        // 计算当前视口顶部相对于内容顶部的距离
        float contentTopToViewportTop = (1f - position.y) * (contentHeight - viewportHeight);
        int startIndex = Mathf.FloorToInt(contentTopToViewportTop / itemHeight);
        // 确保startIndex加上可见项数量不会超出邮件列表的总数
        startIndex = Mathf.Clamp(startIndex, 0, Mathf.Max(0, totalItems - visableItemCount));
        mailstartIndex = startIndex;
        endIndex = startIndex + visableItemCount;
        //Debug.Log("startIndex" + startIndex.ToString());
        return startIndex;
    }//startindex是第一个要显示的索引
    private void ClickedMail(MailItem mailItem)
    {
        if (LastSelectMail != null) 
        {
            LastSelectMail.isSelect = false;
        }
        Debug.Log(mailItem.Mailid);
        foreach (MailData mail in MailBox.Instance.mailList)
        {
            if (mail.mailId == mailItem.Mailid) //选中
            {
                midPanel.SelectMailItem = mailItem;
                MailBox.Instance.setIsSelect(mail);
                MailBox.Instance.setIsReady(mail);
                SetRightMail(mail,mailItem);
                SelectMailItem = mailItem;
                CurrentSelectMail = mail;
                LastSelectMail = mail;
                updateFromData();
                break;
            }     
        }
    }
}
