using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    private void Awake()
    {
        
    }
    private void Start()
    {
        btnDelete.onClick.AddListener(DeleteMail);//监听删除按钮
        btnRecieve.onClick.AddListener(ReceiveMail);//监听领取按钮
        btnCreateMail.onClick.AddListener(CreateMail);//监听创建按钮
        btnRecieveAll.onClick.AddListener(RecieveAllMail);//监听领取全部按钮
        btnDeleteAlready.onClick.AddListener(DeleteAlreadyMail);//监听删除已读按钮
    }
    private void Update()
    {
       
    }
    private void DeleteAlreadyMail()
    {
        MailBox.Instance.DeleteIsReadyMail();
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
    }
    private void ReceiveMail()
    {
        MailBox.Instance.ReceiveReward(SelectMailItem.Mailid);

    }
    private void DeleteMail()
    {    
        MailBox.Instance.RemoveMail(SelectMailItem.Mailid);
    }
}
