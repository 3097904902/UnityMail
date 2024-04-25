using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MidPanel : MonoBehaviour
{
    [Header("������ͼ�͹�����ͼContent")]
    public RectTransform ScrollViewContentRect;
    public RectTransform ScrollViewRect;
    [Header("�Ҳ��ʼ������Լ�����")]
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
        btnDelete.onClick.AddListener(DeleteMail);//����ɾ����ť
        btnRecieve.onClick.AddListener(ReceiveMail);//������ȡ��ť
        btnCreateMail.onClick.AddListener(CreateMail);//����������ť
        btnRecieveAll.onClick.AddListener(RecieveAllMail);//������ȡȫ����ť
        btnDeleteAlready.onClick.AddListener(DeleteAlreadyMail);//����ɾ���Ѷ���ť
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
        MailBox.Instance.CreateMail("������Ӫ�Ŷ�", "�������", "<<����>>ԤԼ����ý��ע����",
               "�װ�����ң�\n����ά���Ѿ�����������ά���ڼ����޷�������¼��Ϸ," +
               "��Ŀ������������²�����ϣ��������Ϸ��죡", DateTime.Now);
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
