using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MidPanel : MonoBehaviour
{
    [Header("滚动视图和滚动视图Content")]
    public RectTransform ScrollViewContentRect;
    public RectTransform ScrollViewRect;
    [Header("右侧邮件内容以及附件")]
    public GameObject MailContent;
    public GameObject attachment;
    [Header("Text")]
    public Text contentTitle;
    public Text Time;
    public Text contentSendMessage;
    public Text contentBody;
    public Text MailCountText;
    [Header("Button")]
    public Button btnDeleteAlready;
    public Button btnRecieveAll;
    public Button btnDelete;
    public Button btnRecieve;
    public Button btnCreateMail;
    [Header("Image")]
    public Image LeftNull;
    public Image RightNull;
    [Header("MailItemList")]
    public List<MailItem> MailItemList = new List<MailItem>();
    public MailItem SelectMailItem;
    MailData LastSelectMail;
    MailData CurrentSelectMail;
    [Header("ListInfo")]
    public RecyingList ScrollList;
    public RectTransform Mail;
  
    private void Awake()
    {
        MailBox.Instance.LoadMailsFromXml("mails.xml");
        MailBox.Instance.LoadIdCount(); 
    }
    private void OnEnable()
    {
        btnDelete.onClick.AddListener(DeleteMail);//监听删除按钮
        btnRecieve.onClick.AddListener(ReceiveMail);//监听领取按钮
        btnCreateMail.onClick.AddListener(CreateMail);//监听创建按钮
        btnRecieveAll.onClick.AddListener(RecieveAllMail);//监听领取全部按钮
        btnDeleteAlready.onClick.AddListener(DeleteAlreadyMail);//监听删除已读按钮

        MailBox.Instance.OnMailCount += ShowMailCount;
        MailBox.Instance.OnLoadFromXML += updateFromData;
        MailItem.OnClickedMailItem += ClickedMail;
        MailBox.Instance.OnMailDelete += SetFirstSelect;//删除邮件的时候重新设置第一个为默认
        ScrollList.OnSetMailItem += SetMailItem;
    }
    private void OnDisable()
    {
        MailBox.Instance.OnMailCount -= ShowMailCount;
        MailBox.Instance.OnLoadFromXML -= updateFromData;
        MailItem.OnClickedMailItem -= ClickedMail;
        MailBox.Instance.OnMailDelete -= SetFirstSelect;
        ScrollList.OnSetMailItem -= SetMailItem;
    }
    private void Start()
    {
        Mail.gameObject.SetActive(false);
        StartCoroutine(CheckAndDeleteExpiredMailsEveryHour());
        MailBox.Instance.mailList = MailBox.Instance.SortMails(MailBox.Instance.mailList);//先排序
        MailBox.Instance.LoadIdCount();//右上角显示邮件数量
        InitScrollList();//初始化链表
        if (boolIsShowNullMailList())//判断邮箱是否为空
        {
            ShowNullMailList();
        }
        else
        {
            updateFromData();//先加载一次邮件列表
            SetFirstSelect();//设置第一个为默认选中的邮件
            StartCoroutine(CheckAndDeleteExpiredMailsEveryHour());//过期删除邮件
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
    private void ShowMailCount()
    {
       MailCountText.text = MailBox.Instance.mailList.Count.ToString() + "/" + MailBox.Instance.MailMaxCount.ToString();
    }
   
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            MailBox.Instance.InitMail();
            if (MailBox.Instance.mailList.Count > 0)
            {
                NotshowNullMailList();
            }
        }
    }
    private void InitScrollList() 
    {
        //设置链表的Cell数量
        ScrollList.SetCellNum(8);
        //设置每个链表的长宽
        ScrollList.setCellSize();
        //设置视口的高度
        ScrollList.SetviewportHeight(ScrollViewRect.rect.height);
        //设置间隔
        ScrollList.SetItemSpace(10f);
        ScrollList.SetVisableCount();
        ScrollList.InitRecyingList();
    }
    private void updateFromData() 
    {
        ScrollList.RefreshAll();//刷新UI显示
    }
    private bool boolIsShowNullMailList()
    {
        if (MailBox.Instance.mailList.Count <= 0) return true;
        return false;
    }
    private void ShowNullMailList()
    {
        LeftNull.gameObject.SetActive(true);
        RightNull.gameObject.SetActive(true);
        //没有邮件的时候设置为false
        MailContent.SetActive(false);
    }
    private void NotshowNullMailList() 
    {
        LeftNull.gameObject.SetActive(false);
        RightNull.gameObject.SetActive(false);
        //没有邮件的时候设置为false
        MailContent.SetActive(true);
    }
    private void SetMailItem(MailData mail,MailItem mailItem)
    {
        //把mailInfo中的对应的id赋值
        mailItem.Mailid = mail.mailId;
        mailItem.MailTitle.text = SetMailTitle(mail.mailTitle);
        mailItem.MailSendName.text = mail.sender;
        mailItem.SentTime = mail.sentTime;
        mailItem.MailTime.text = TimeAgo(mail.sentTime);
        CheckisRead(mail, mailItem);
        CheckisSelect(mail, mailItem);
        CheckisReward(mail, mailItem);
        if (CurrentSelectMail != null)
        {
            SetMidPanelUI(CurrentSelectMail);
        }
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
    public void SetFirstSelect()
    {
        if (MailBox.Instance.mailList != null&& MailBox.Instance.mailList.Count > 0)
        {
            CloseAllSelect();
            MailData maildata = MailBox.Instance.mailList[0];
            CurrentSelectMail = maildata;
            MailBox.Instance.mailList[0].isSelect = true;
            LastSelectMail = maildata;
            SetRightMail(CurrentSelectMail, SelectMailItem);
            CurrentSelectMail.isSelect = true;
            CurrentSelectMail.isRead = true;
            CheckisRead(maildata, SelectMailItem);
            CheckisSelect(maildata, SelectMailItem);
            updateFromData();
        }
    }
    public void CheckisRead(MailData mail, MailItem mailItem)
    {
        if (mailItem != null)
        {
            if (mail.isRead)
            {
                // 红点消失
                mailItem.isRead.gameObject.SetActive(false);
                SetTransparency(mailItem); // 变透明
            }
            else
            {
                mailItem.isRead.gameObject.SetActive(true);
                //SetNoTransparency(mailItem); // 不透明
                NoSelectImage(mailItem);//变回白色
            }
        }
        else
        {
            Debug.LogWarning("MailItem is null.");
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
        Color color = mailItem.Mail.color;
        color.a = 1f; // 设置透明度，0.5为半透明
        mailItem.Mail.color = color;
    }
    public void CheckisReward(MailData mail, MailItem mailItem)
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
            btnDelete.gameObject.SetActive(isCompleted);
            btnRecieve.gameObject.SetActive(!isCompleted);
            attachment.gameObject.SetActive(hasReward);
        }
        else
        {
            btnDelete.gameObject.SetActive(true);
            attachment.gameObject.SetActive(false);
            btnRecieve.gameObject.SetActive(false);
        }
    }
    public void CheckisSelect(MailData mail, MailItem mailItem)
    {
        if (mailItem != null)
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
        else
        {
            Debug.LogWarning("MailItem is null.");
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
    public void CloseAllSelect()
    {
        foreach (MailData mail in MailBox.Instance.mailList)
        {
            mail.isSelect = false;
        }
    }
    public void SetRightMail(MailData mail, MailItem mailItem)
    {
        contentTitle.text = mail.mailTitle;
        Time.text = mail.sentTime.ToString("yyyy-MM-dd HH:mm");
        contentSendMessage.text = mail.sender;
        contentBody.text = mail.mailContent;
    }
    private void ClickedMail(MailItem mailItem)
    {
        Debug.Log(mailItem.Mailid);
        if (LastSelectMail != null)
        {
            LastSelectMail.isSelect = false;
        }
        Debug.Log(mailItem.Mailid);
        foreach (MailData mail in MailBox.Instance.mailList)
        {
            if (mail.mailId == mailItem.Mailid) //选中
            {
                MailBox.Instance.setIsSelect(mail);
                MailBox.Instance.setIsReady(mail);
                SetRightMail(mail, mailItem);
                SelectMailItem = mailItem;
                CurrentSelectMail = mail;
                LastSelectMail = mail;
                updateFromData();
                break;
            }
        }
    }
    private void DeleteAlreadyMail()
    {
        MailBox.Instance.DeleteIsReadyMail();
        if (MailBox.Instance.mailList.Count <= 0)
        {
            ShowNullMailList();
        }
    }
    private void RecieveAllMail()
    {
        MailBox.Instance.ReceiveAllReward();
    }
    private void CreateMail()
    {
        MailBox.Instance.CreateMail("望月运营团队", "玩家名字", "<<望月>>预约及社媒关注奖励",
               "亲爱的玩家：\n本次维护已经结束，对于维护期间您无法正常登录游戏," +
               "项目组对您做出如下补偿，希望您能游戏愉快！", DateTime.Now);
        if (MailBox.Instance.mailList.Count > 0) 
        {
            NotshowNullMailList();
        }
    }
    private void ReceiveMail()
    {
        MailBox.Instance.ReceiveReward(SelectMailItem.Mailid);
    }
    private void DeleteMail()
    {    
        MailBox.Instance.RemoveMail(SelectMailItem.Mailid);
        if (MailBox.Instance.mailList.Count<=0) 
        {
            ShowNullMailList();
        }
    }
}
