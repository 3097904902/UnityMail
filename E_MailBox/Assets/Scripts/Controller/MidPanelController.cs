using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Mail;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class MidPanelController : MonoBehaviour
{
    static MidPanelController midPanelController = null;
    private MidPanelView midPanelView;
    public MailInfo lastSelectedMailInfo = null;
    public MailInfo FirstSelectedMailInfo = null;
    public ReMoveMail removeMail = null;
    public ReceiveReward receive = null;
    public static int count = 0;
    public int visableItemCount;
    public int mailstartIndex = 0;//当前第一个显示的邮件索引
    public int endIndex  = 0;
    public int currentStartIndex = 0;
    public float Offset;
    public float Spacing = 10f;
    public float viewportHeight;
    public float currentItemHeight;
    public float currentItemWidth;
    public MailData LastSelectmailData;
    //public float 
    public int lastStartIndex = 0;
    static MidPanelController MidpanelController
    {
        get
        {
            return midPanelController;
        }
    }
    private void Awake()
    {
        MailBox.Instance.OnLoadFromXML += UpdateInfo;
        midPanelView = GetComponent<MidPanelView>();
        visableItemCount = 6;

}
private void Start()
    {
        removeMail = GetComponent<ReMoveMail>();
        receive = GetComponent<ReceiveReward>();
        // 启动协程
        StartCoroutine(CheckAndDeleteExpiredMailsEveryHour());
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
    private void OnEnable()
    {
        MailInfo.OnClickedMailItem += ClickedMailItem;
    }
    private void OnDisable()
    {
        MailInfo.OnClickedMailItem -= ClickedMailItem;
        MailBox.Instance.OnLoadFromXML -= UpdateInfo;
    }
    public  void UpdateInfo() 
    {
        if (midPanelView != null)
        {
            midPanelView.UpdateInfo(MailBox.Instance);
        }
    }
    private void Update()
    {
        if (midPanelView != null)
        {
            if (MailBox.Instance.mailList.Count <= 0)
            {
                midPanelView.LeftNull.gameObject.SetActive(true);
                midPanelView.RightNull.gameObject.SetActive(true);
                //没有邮件的时候设置为false
                midPanelView.MailContent.SetActive(false);
            }
            else
            {
                midPanelView.LeftNull.gameObject.SetActive(false);
                midPanelView.RightNull.gameObject.SetActive(false);
            }
        }

    }
    public void OnScrollChanged()
    {
        // 如果startIndex增加，表示用户向下滚动
        if (mailstartIndex > currentStartIndex)
        {
            Debug.Log("用户向下滚动");
            // 将现在上面的邮件项移动到最下面并且设置为初始状态
            MoveTopItemToBottom();
        }
        // 如果startIndex减少，表示用户向上滚动
        else if (mailstartIndex < currentStartIndex)
        {
            Debug.Log("用户向上滚动");
            // 将最下面的邮件项移动到最上面并且设置为初始状态
            MoveBottomItemToTop();
        }
        // 更新当前startIndex为新的值
        currentStartIndex = mailstartIndex;
    }

    // 将最上面的邮件项移动到最下面，并更新其内容
    public void MoveTopItemToBottom()
    {
        // 假设midPanelView.MailItemList存储了所有邮件项的引用
        GameObject topItem = midPanelView.MailItemList[0];//把当前最上面的元素移动到+邮件数的位置
        //记录当前的位置
        RectTransform topItemRect = topItem.GetComponent<RectTransform>();
        float curretntPosition = topItemRect.anchoredPosition.y;
        float newYposition = curretntPosition - currentItemHeight * visableItemCount - visableItemCount * Spacing;
        topItemRect.anchoredPosition = new Vector2(topItemRect.anchoredPosition.x, newYposition);
        midPanelView.MailItemList.RemoveAt(0);
        midPanelView.MailItemList.Add(topItem);
        // 更新移动后的邮件项的内容
        int newMailIndex = currentStartIndex + visableItemCount - 1;
        UpdateVisibleMailItems(newMailIndex);
    }

    // 将最下面的邮件项移动到最上面，并更新其内容
    public void MoveBottomItemToTop()
    {
        GameObject bottomItem = midPanelView.MailItemList[visableItemCount - 1];
        RectTransform bottomItemRect = bottomItem.GetComponent<RectTransform>();
        float curretntPosition = bottomItemRect.anchoredPosition.y;
        float newYposition = curretntPosition + currentItemHeight * visableItemCount + visableItemCount * Spacing;
        bottomItemRect.anchoredPosition = new Vector2(bottomItemRect.anchoredPosition.x, newYposition);
        midPanelView.MailItemList.RemoveAt(visableItemCount - 1);
        midPanelView.MailItemList.Insert(0, bottomItem);
        // 更新移动后的邮件项的内容
        int newMailIndex = currentStartIndex;
        UpdateVisibleMailItems(newMailIndex);
    }
    public void showCancelImage(MailInfo mailinfo)
    {
        //显示选中的图标到对应的item上
        mailinfo.Select.gameObject.SetActive(false);
        mailinfo.isRead.gameObject.SetActive(true);
       //重新设置回白色
        mailinfo.Mail.color = new Color(1f, 1f, 1f, 1f);
    }
    // 更新邮件项的内容
    public void InitInfo()
    {
        InitMidPanelView();
    }
    public void InitMidPanelView() 
    {
        currentItemWidth = midPanelView.ScrollViewTransform.rect.width;
        viewportHeight = midPanelView.ScrollViewTransform.rect.height;
        Spacing = 10f;
        currentItemHeight =  (viewportHeight -(visableItemCount-2)* Spacing )/ (visableItemCount - 1);
        Debug.Log("currentItemHeight" + currentItemHeight);
        Offset = Mathf.Floor(currentItemHeight/2);
        //实例化的时候设置好位置，根据邮件的index设置好位置和高度
        midPanelView.MailPrefab = Resources.Load<GameObject>("Mail");
        for (int i = 0; i < visableItemCount; i++) 
        {
            GameObject obj = Instantiate(midPanelView.MailPrefab, midPanelView.ScrollViewContent);
            obj.transform.SetParent(midPanelView.itemContent.transform, false);
            //设置位置
            float posY = -i * (currentItemHeight + Spacing);
            //设置大小
            RectTransform rect = obj.gameObject.GetComponent<RectTransform>();
            rect.sizeDelta =new Vector2(currentItemWidth, currentItemHeight);
            rect.offsetMin = new Vector2(0, rect.offsetMin.y); 
            rect.offsetMax = new Vector2(0, rect.offsetMax.y);
            obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, posY-Offset);
            midPanelView.MailItemList.Add(obj);
        }
        //初始化
        UpdateVisibleMailItems(0); // 使用当前的startIndex更新邮件项内容
        AdjustContentSize(MailBox.Instance.mailList.Count, currentItemHeight); // 调整内容大小
    }

    public void ReloadMailItems()
    {
        
        UpdateVisibleMailItems(mailstartIndex); // 使用当前的startIndex更新邮件项内容
        AdjustContentSize(MailBox.Instance.mailList.Count, midPanelView.MailitemPrefabSize.rect.height); // 调整内容大小
        
    }
    public void UpdateVisibleMailItems(int startIndex)
    {
        //根据index邮件项进行移动，移动之后再更新邮件的内容
        //i 5
        int lastiindex = startIndex;
        for (int i = 0; i < visableItemCount; i++)//意思是显示初始索引之后的可见的邮件
        {
            int mailIndex = startIndex + i;
            if (mailIndex < MailBox.Instance.mailList.Count)
            {
                // 获取对应的邮件数据
                MailData mail = MailBox.Instance.mailList[mailIndex];
                // 获取对应的邮件项GameObject
                GameObject mailItem = midPanelView.MailItemList[i];
                // 更新邮件项以显示当前邮件数据,不仅更新邮件数据还要根据数据显示ui
                SeteveryMailitem(mail, mailItem);
                mailItem.SetActive(true);
            }
            else
            {
                // 如果当前索引超出邮件列表范围，隐藏多余的邮件项
                midPanelView.MailItemList[i].SetActive(false);
            }
        }

    }
    public void SeteveryMailitem(MailData mail,GameObject obj) 
    {
        MailInfo mailinfo = obj.GetComponent<MailInfo>();
        //把mailInfo中的对应的id赋值
        mailinfo.Mailid = mail.mailId;
        mailinfo.MailTitle.text = SetMailTitle(mail.mailTitle);
        mailinfo.MailSendName.text = mail.sender;
        mailinfo.SentTime = mail.sentTime;
        //如果含有奖励则显示奖励图标
        SetisReward(mail,mailinfo);
        SetisReady(mail, mailinfo);
        SetisSelect(mail, mailinfo);
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
    public void SetisReady(MailData mail, MailInfo mailInfo) 
    {
        if (mail.isRead)
        {
            //红点消失
            mailInfo.isRead.gameObject.SetActive(false);
            SetTransparency(mailInfo);
        }
        else 
        {
            mailInfo.isRead.gameObject.SetActive(true);
            SetNoTransparency(mailInfo);
        }

    }
    public void SetisReward(MailData mail, MailInfo mailInfo) 
    {
        if (mail.isReward)
        {
            mailInfo.ImageReward.gameObject.SetActive(true);
            mailInfo.ImageMessage.gameObject.SetActive(false);
            if (mail.isCompleted)//如果已完成领取
            {
                midPanelView.btnDelete.gameObject.SetActive(true);
                midPanelView.btnRecieve.gameObject.SetActive(false);
            }
        }
        else 
        {
            mailInfo.ImageReward.gameObject.SetActive(false);
            mailInfo.ImageMessage.gameObject.SetActive(true);

        }
    }
    public void SetisSelect(MailData mail,MailInfo mailInfo) 
    {
        if (mail.isSelect)
        {
            mailInfo.Select.gameObject.SetActive(true);
            showSelectImage(mailInfo);
        }
        else 
        {
            mailInfo.Select.gameObject.SetActive(false);
            showCancelImage(mailInfo);
        }
    }
    public void SetRightMail(MailData mail)
    {
        midPanelView.contentTitle.text = mail.mailTitle;
        midPanelView.Time.text = mail.sentTime.ToString("yyyy-MM-dd HH:mm");
        midPanelView.contentSendMessage.text = mail.sender;
        midPanelView.contentBody.text = mail.mailContent;
    }
    public void AdjustContentSize(int MailCounts, float height) //调整Content的高度
    {
        float totalHeight = height * MailCounts + (MailCounts - 1) * (Spacing);
        midPanelView.itemContent.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(this.GetComponent<RectTransform>().sizeDelta.x, totalHeight);
    }
    public void ClickedMailItem(GameObject Mailitem)//鼠标点击邮件事件
    {
        //首先把右半部分置为真
        midPanelView.MailContent.SetActive(true);
        ShowMailContent(Mailitem);
        MailBox.Instance.SaveMailsToXml("mails.xml");
    }
    public void ShowMailContent(GameObject Mailitem) //返回的是点击的mailInfo
    {
        MailInfo mailinfo = Mailitem.GetComponent<MailInfo>();
        Debug.Log(mailinfo.Mailid);

        if (lastSelectedMailInfo != null && lastSelectedMailInfo != mailinfo)
        {
            //上一个的选中框失活
            LastSelectmailData = MailBox.Instance.mailList[lastSelectedMailInfo.Mailid];
            LastSelectmailData.isSelect = false;
            lastSelectedMailInfo.isSelect = false;
            lastSelectedMailInfo.Select.gameObject.SetActive(false);
            lastSelectedMailInfo.Mail.color = new Color(1, 1, 1, 1);
            SetTransparency(lastSelectedMailInfo);
        }
        midPanelView.attachment.gameObject.SetActive(false);
        //根据对应的邮件id找到对应的邮件，把信息显示过来
        foreach (MailData mail in MailBox.Instance.mailList)
        {
            if (mailinfo.Mailid == mail.mailId)//对应的进行操作
            {
                SetRightMail(mail);
                MailBox.Instance.setIsReady(mail);
                removeMail.MailID = mail.mailId;
                showSelectImage(mailinfo);
                mail.isSelect = true;
                 // 更新最后选中的邮件项引用
                lastSelectedMailInfo = mailinfo;
                ClickisReward(mail);
                break; // 找到匹配的邮件后就可以停止循环了
            }
        }
    }
    public void showSelectImage(MailInfo mailinfo) 
    {
        //显示选中的图标到对应的item上
        mailinfo.Select.gameObject.SetActive(true);
        mailinfo.isRead.gameObject.SetActive(false);
        //选中的Item要变灰色
        mailinfo.Mail.color = new Color(0.5f, 0.5f, 0.5f, 1);
    }
   
    public void showFirstSelectImage(MailInfo mailinfo) 
    {
        //显示选中的图标到对应的item上
        mailinfo.Select.gameObject.SetActive(true);
        //选中的Item要变灰色
        mailinfo.Mail.color = new Color(0.5f, 0.5f, 0.5f, 1);
    }
    public void ClickisReward(MailData mail) 
    {
        if (mail.isReward)
        {
            midPanelView.attachment.gameObject.SetActive(true);
            //按钮变为领取
            midPanelView.btnDelete.gameObject.SetActive(false);
            midPanelView.btnRecieve.gameObject.SetActive(true);
            receive.MailID = mail.mailId;
            if (mail.isCompleted)
            {
                midPanelView.btnDelete.gameObject.SetActive(true);
                midPanelView.btnRecieve.gameObject.SetActive(false);
            }
        }
        else
        {
            midPanelView.btnDelete.gameObject.SetActive(true);
            midPanelView.btnRecieve.gameObject.SetActive(false);
        }
    }
    public void SetTransparency(MailInfo mailInfo) //设置半透明
    {
        Color color = mailInfo.Mail.color;
        color.a = 0.5f; // 设置透明度，0.5为半透明
        mailInfo.Mail.color = color;
    }
    public void SetNoTransparency(MailInfo mailInfo) //设置不透明
    {
        //Debug.Log("设置透明度为1");
        Color color = mailInfo.Mail.color;
        color.a = 1f; // 设置透明度，0.5为半透明
        mailInfo.Mail.color = color;
    }
}
