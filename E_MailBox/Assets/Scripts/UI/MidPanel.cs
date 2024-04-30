using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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
        btnDelete.onClick.AddListener(DeleteMail);//����ɾ����ť
        btnRecieve.onClick.AddListener(ReceiveMail);//������ȡ��ť
        btnCreateMail.onClick.AddListener(CreateMail);//����������ť
        btnRecieveAll.onClick.AddListener(RecieveAllMail);//������ȡȫ����ť
        btnDeleteAlready.onClick.AddListener(DeleteAlreadyMail);//����ɾ���Ѷ���ť

        MailBox.Instance.OnMailCount += ShowMailCount;
        MailBox.Instance.OnLoadFromXML += updateFromData;
        MailItem.OnClickedMailItem += ClickedMail;
        MailBox.Instance.OnMailDelete += SetFirstSelect;//ɾ���ʼ���ʱ���������õ�һ��ΪĬ��
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
        MailBox.Instance.mailList = MailBox.Instance.SortMails(MailBox.Instance.mailList);//������
        MailBox.Instance.LoadIdCount();//���Ͻ���ʾ�ʼ�����
        InitScrollList();//��ʼ������
        if (boolIsShowNullMailList())//�ж������Ƿ�Ϊ��
        {
            ShowNullMailList();
        }
        else
        {
            updateFromData();//�ȼ���һ���ʼ��б�
            SetFirstSelect();//���õ�һ��ΪĬ��ѡ�е��ʼ�
            StartCoroutine(CheckAndDeleteExpiredMailsEveryHour());//����ɾ���ʼ�
        }
    }
    private IEnumerator CheckAndDeleteExpiredMailsEveryHour()
    {
        // ����ѭ��
        while (true)
        {
            // ִ�м���ɾ�������ʼ����߼�
            CheckAndDeleteExpiredMails();
            // �ȴ�һ��Сʱ��3600�룩
            yield return new WaitForSeconds(3600f);
        }
    }
    private void CheckAndDeleteExpiredMails()
    {
        //��ʱ����ʼ���û�й���
        foreach (MailData mail in MailBox.Instance.mailList)
        {
            TimeSpan timeSince = DateTime.Now - mail.sentTime;
            if (timeSince.Days >= 10) //����10�����ɾ��
            {
                MailBox.Instance.RemoveMail(mail.mailId);
            }
        }
        Debug.Log("��鲢ɾ�������ʼ�");
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
        //���������Cell����
        ScrollList.SetCellNum(8);
        //����ÿ������ĳ���
        ScrollList.setCellSize();
        //�����ӿڵĸ߶�
        ScrollList.SetviewportHeight(ScrollViewRect.rect.height);
        //���ü��
        ScrollList.SetItemSpace(10f);
        ScrollList.SetVisableCount();
        ScrollList.InitRecyingList();
    }
    private void updateFromData() 
    {
        ScrollList.RefreshAll();//ˢ��UI��ʾ
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
        //û���ʼ���ʱ������Ϊfalse
        MailContent.SetActive(false);
    }
    private void NotshowNullMailList() 
    {
        LeftNull.gameObject.SetActive(false);
        RightNull.gameObject.SetActive(false);
        //û���ʼ���ʱ������Ϊfalse
        MailContent.SetActive(true);
    }
    private void SetMailItem(MailData mail,MailItem mailItem)
    {
        //��mailInfo�еĶ�Ӧ��id��ֵ
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
        // ����ʹ����ǰ����ͬ��TimeAgo�߼�
        if (timeSince.TotalDays >= 1) return $"{(int)timeSince.TotalDays}��ǰ";
        if (timeSince.TotalHours >= 1) return $"{(int)timeSince.TotalHours}Сʱǰ";
        if (timeSince.TotalMinutes >= 1) return $"{(int)timeSince.TotalMinutes}����ǰ";
        return "�ո�";
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
                // �����ʧ
                mailItem.isRead.gameObject.SetActive(false);
                SetTransparency(mailItem); // ��͸��
            }
            else
            {
                mailItem.isRead.gameObject.SetActive(true);
                //SetNoTransparency(mailItem); // ��͸��
                NoSelectImage(mailItem);//��ذ�ɫ
            }
        }
        else
        {
            Debug.LogWarning("MailItem is null.");
        }
    }
    public void SetTransparency(MailItem mailItem) //���ð�͸��
    {
        Color color = mailItem.Mail.color;
        color.a = 0.3f; // ����͸���ȣ�0.5Ϊ��͸��
        mailItem.Mail.color = color;
    }
    public void SetNoTransparency(MailItem mailItem) //���ò�͸��
    {
        Color color = mailItem.Mail.color;
        color.a = 1f; // ����͸���ȣ�0.5Ϊ��͸��
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
                showSelectImage(mailItem);//���ɫ
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
            if (mail.mailId == mailItem.Mailid) //ѡ��
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
        MailBox.Instance.CreateMail("������Ӫ�Ŷ�", "�������", "<<����>>ԤԼ����ý��ע����",
               "�װ�����ң�\n����ά���Ѿ�����������ά���ڼ����޷�������¼��Ϸ," +
               "��Ŀ������������²�����ϣ��������Ϸ��죡", DateTime.Now);
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
