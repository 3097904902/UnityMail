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
    public int mailstartIndex = 0;//��ǰ��һ����ʾ���ʼ�����
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
        // ����Э��
        StartCoroutine(CheckAndDeleteExpiredMailsEveryHour());
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
                //û���ʼ���ʱ������Ϊfalse
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
        // ���startIndex���ӣ���ʾ�û����¹���
        if (mailstartIndex > currentStartIndex)
        {
            Debug.Log("�û����¹���");
            // ������������ʼ����ƶ��������沢������Ϊ��ʼ״̬
            MoveTopItemToBottom();
        }
        // ���startIndex���٣���ʾ�û����Ϲ���
        else if (mailstartIndex < currentStartIndex)
        {
            Debug.Log("�û����Ϲ���");
            // ����������ʼ����ƶ��������沢������Ϊ��ʼ״̬
            MoveBottomItemToTop();
        }
        // ���µ�ǰstartIndexΪ�µ�ֵ
        currentStartIndex = mailstartIndex;
    }

    // ����������ʼ����ƶ��������棬������������
    public void MoveTopItemToBottom()
    {
        // ����midPanelView.MailItemList�洢�������ʼ��������
        GameObject topItem = midPanelView.MailItemList[0];//�ѵ�ǰ�������Ԫ���ƶ���+�ʼ�����λ��
        //��¼��ǰ��λ��
        RectTransform topItemRect = topItem.GetComponent<RectTransform>();
        float curretntPosition = topItemRect.anchoredPosition.y;
        float newYposition = curretntPosition - currentItemHeight * visableItemCount - visableItemCount * Spacing;
        topItemRect.anchoredPosition = new Vector2(topItemRect.anchoredPosition.x, newYposition);
        midPanelView.MailItemList.RemoveAt(0);
        midPanelView.MailItemList.Add(topItem);
        // �����ƶ�����ʼ��������
        int newMailIndex = currentStartIndex + visableItemCount - 1;
        UpdateVisibleMailItems(newMailIndex);
    }

    // ����������ʼ����ƶ��������棬������������
    public void MoveBottomItemToTop()
    {
        GameObject bottomItem = midPanelView.MailItemList[visableItemCount - 1];
        RectTransform bottomItemRect = bottomItem.GetComponent<RectTransform>();
        float curretntPosition = bottomItemRect.anchoredPosition.y;
        float newYposition = curretntPosition + currentItemHeight * visableItemCount + visableItemCount * Spacing;
        bottomItemRect.anchoredPosition = new Vector2(bottomItemRect.anchoredPosition.x, newYposition);
        midPanelView.MailItemList.RemoveAt(visableItemCount - 1);
        midPanelView.MailItemList.Insert(0, bottomItem);
        // �����ƶ�����ʼ��������
        int newMailIndex = currentStartIndex;
        UpdateVisibleMailItems(newMailIndex);
    }
    public void showCancelImage(MailInfo mailinfo)
    {
        //��ʾѡ�е�ͼ�굽��Ӧ��item��
        mailinfo.Select.gameObject.SetActive(false);
        mailinfo.isRead.gameObject.SetActive(true);
       //�������ûذ�ɫ
        mailinfo.Mail.color = new Color(1f, 1f, 1f, 1f);
    }
    // �����ʼ��������
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
        //ʵ������ʱ�����ú�λ�ã������ʼ���index���ú�λ�ú͸߶�
        midPanelView.MailPrefab = Resources.Load<GameObject>("Mail");
        for (int i = 0; i < visableItemCount; i++) 
        {
            GameObject obj = Instantiate(midPanelView.MailPrefab, midPanelView.ScrollViewContent);
            obj.transform.SetParent(midPanelView.itemContent.transform, false);
            //����λ��
            float posY = -i * (currentItemHeight + Spacing);
            //���ô�С
            RectTransform rect = obj.gameObject.GetComponent<RectTransform>();
            rect.sizeDelta =new Vector2(currentItemWidth, currentItemHeight);
            rect.offsetMin = new Vector2(0, rect.offsetMin.y); 
            rect.offsetMax = new Vector2(0, rect.offsetMax.y);
            obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, posY-Offset);
            midPanelView.MailItemList.Add(obj);
        }
        //��ʼ��
        UpdateVisibleMailItems(0); // ʹ�õ�ǰ��startIndex�����ʼ�������
        AdjustContentSize(MailBox.Instance.mailList.Count, currentItemHeight); // �������ݴ�С
    }

    public void ReloadMailItems()
    {
        
        UpdateVisibleMailItems(mailstartIndex); // ʹ�õ�ǰ��startIndex�����ʼ�������
        AdjustContentSize(MailBox.Instance.mailList.Count, midPanelView.MailitemPrefabSize.rect.height); // �������ݴ�С
        
    }
    public void UpdateVisibleMailItems(int startIndex)
    {
        //����index�ʼ�������ƶ����ƶ�֮���ٸ����ʼ�������
        //i 5
        int lastiindex = startIndex;
        for (int i = 0; i < visableItemCount; i++)//��˼����ʾ��ʼ����֮��Ŀɼ����ʼ�
        {
            int mailIndex = startIndex + i;
            if (mailIndex < MailBox.Instance.mailList.Count)
            {
                // ��ȡ��Ӧ���ʼ�����
                MailData mail = MailBox.Instance.mailList[mailIndex];
                // ��ȡ��Ӧ���ʼ���GameObject
                GameObject mailItem = midPanelView.MailItemList[i];
                // �����ʼ�������ʾ��ǰ�ʼ�����,���������ʼ����ݻ�Ҫ����������ʾui
                SeteveryMailitem(mail, mailItem);
                mailItem.SetActive(true);
            }
            else
            {
                // �����ǰ���������ʼ��б�Χ�����ض�����ʼ���
                midPanelView.MailItemList[i].SetActive(false);
            }
        }

    }
    public void SeteveryMailitem(MailData mail,GameObject obj) 
    {
        MailInfo mailinfo = obj.GetComponent<MailInfo>();
        //��mailInfo�еĶ�Ӧ��id��ֵ
        mailinfo.Mailid = mail.mailId;
        mailinfo.MailTitle.text = SetMailTitle(mail.mailTitle);
        mailinfo.MailSendName.text = mail.sender;
        mailinfo.SentTime = mail.sentTime;
        //������н�������ʾ����ͼ��
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
            //�����ʧ
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
            if (mail.isCompleted)//����������ȡ
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
    public void AdjustContentSize(int MailCounts, float height) //����Content�ĸ߶�
    {
        float totalHeight = height * MailCounts + (MailCounts - 1) * (Spacing);
        midPanelView.itemContent.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(this.GetComponent<RectTransform>().sizeDelta.x, totalHeight);
    }
    public void ClickedMailItem(GameObject Mailitem)//������ʼ��¼�
    {
        //���Ȱ��Ұ벿����Ϊ��
        midPanelView.MailContent.SetActive(true);
        ShowMailContent(Mailitem);
        MailBox.Instance.SaveMailsToXml("mails.xml");
    }
    public void ShowMailContent(GameObject Mailitem) //���ص��ǵ����mailInfo
    {
        MailInfo mailinfo = Mailitem.GetComponent<MailInfo>();
        Debug.Log(mailinfo.Mailid);

        if (lastSelectedMailInfo != null && lastSelectedMailInfo != mailinfo)
        {
            //��һ����ѡ�п�ʧ��
            LastSelectmailData = MailBox.Instance.mailList[lastSelectedMailInfo.Mailid];
            LastSelectmailData.isSelect = false;
            lastSelectedMailInfo.isSelect = false;
            lastSelectedMailInfo.Select.gameObject.SetActive(false);
            lastSelectedMailInfo.Mail.color = new Color(1, 1, 1, 1);
            SetTransparency(lastSelectedMailInfo);
        }
        midPanelView.attachment.gameObject.SetActive(false);
        //���ݶ�Ӧ���ʼ�id�ҵ���Ӧ���ʼ�������Ϣ��ʾ����
        foreach (MailData mail in MailBox.Instance.mailList)
        {
            if (mailinfo.Mailid == mail.mailId)//��Ӧ�Ľ��в���
            {
                SetRightMail(mail);
                MailBox.Instance.setIsReady(mail);
                removeMail.MailID = mail.mailId;
                showSelectImage(mailinfo);
                mail.isSelect = true;
                 // �������ѡ�е��ʼ�������
                lastSelectedMailInfo = mailinfo;
                ClickisReward(mail);
                break; // �ҵ�ƥ����ʼ���Ϳ���ֹͣѭ����
            }
        }
    }
    public void showSelectImage(MailInfo mailinfo) 
    {
        //��ʾѡ�е�ͼ�굽��Ӧ��item��
        mailinfo.Select.gameObject.SetActive(true);
        mailinfo.isRead.gameObject.SetActive(false);
        //ѡ�е�ItemҪ���ɫ
        mailinfo.Mail.color = new Color(0.5f, 0.5f, 0.5f, 1);
    }
   
    public void showFirstSelectImage(MailInfo mailinfo) 
    {
        //��ʾѡ�е�ͼ�굽��Ӧ��item��
        mailinfo.Select.gameObject.SetActive(true);
        //ѡ�е�ItemҪ���ɫ
        mailinfo.Mail.color = new Color(0.5f, 0.5f, 0.5f, 1);
    }
    public void ClickisReward(MailData mail) 
    {
        if (mail.isReward)
        {
            midPanelView.attachment.gameObject.SetActive(true);
            //��ť��Ϊ��ȡ
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
    public void SetTransparency(MailInfo mailInfo) //���ð�͸��
    {
        Color color = mailInfo.Mail.color;
        color.a = 0.5f; // ����͸���ȣ�0.5Ϊ��͸��
        mailInfo.Mail.color = color;
    }
    public void SetNoTransparency(MailInfo mailInfo) //���ò�͸��
    {
        //Debug.Log("����͸����Ϊ1");
        Color color = mailInfo.Mail.color;
        color.a = 1f; // ����͸���ȣ�0.5Ϊ��͸��
        mailInfo.Mail.color = color;
    }
}
