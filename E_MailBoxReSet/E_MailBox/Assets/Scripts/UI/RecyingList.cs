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
    [Header("�ʼ�������Ϣ")]
    public float itemSpacing;//item֮��ļ��
    public float currentItemHeight;//��ǰ�ʼ��ĸ߶�
    public float currentItemWidth;//��ǰ�ʼ��Ŀ��
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
            scrollRect.onValueChanged.AddListener(OnScroll);//���������¼�
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
            //û���ʼ���ʱ������Ϊfalse
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
        //ʵ������ʱ�����ú�λ�ã������ʼ���index���ú�λ�ú͸߶�
        for (int i = 0; i < visableItemCount; i++)
        {
            var obj = Instantiate(Mail);
            obj.transform.SetParent(scrollRect.content, false);
            //����λ��
            float posY = -i * (currentItemHeight + itemSpacing);
            //���ô�С
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
        //��ʼ��
        UpdateVisibleMailItems(0); // ʹ�õ�ǰ��startIndex�����ʼ�������
        AdjustContentSize(MailBox.Instance.mailList.Count, currentItemHeight); // �������ݴ�С
    }
    void OnScroll(Vector2 position)
    {
        // ���ﴦ������¼�
        //Debug.Log("����λ��: " + position);
        int totalItems = MailBox.Instance.mailList.Count;
        float itemHeight = currentItemHeight + itemSpacing; //��ǰ��item�߶�+���
        float viewportHeight = scrollRect.viewport.rect.height;//��ǰ��ͼ�ĸ߶�
        float contentHeight = scrollRect.content.rect.height;// ��ֱ��ʹ��scrollRect.content.rect.height
        int index = CalculateStartIndexFromPosition(position, totalItems, itemHeight, viewportHeight, contentHeight);
        //���ܵ��ǵ�ǰ���ӷ�Χ�ڵ�һ��������
        OnScrollChanged();
    }
    public void AdjustContentSize(int MailCounts, float height) //����Content�ĸ߶�
    {
        float totalHeight = height * MailCounts + (MailCounts - 1) * (itemSpacing);
        midPanel.ScrollViewContentRect.sizeDelta = new Vector2(midPanel.ScrollViewContentRect.sizeDelta.x, totalHeight);
    }
    private void OnScrollChanged()
    {
        // ���startIndex���ӣ���ʾ�û����¹���
        if (mailstartIndex > currentStartIndex)
        {
            //Debug.Log("�û����¹���");
            // ������������ʼ����ƶ��������沢������Ϊ��ʼ״̬
            for(int i = 0;i< mailstartIndex- currentStartIndex;i++)
                MoveTopItemToBottom();
        }
        // ���startIndex���٣���ʾ�û����Ϲ���
        else if (mailstartIndex < currentStartIndex)
        {
            //Debug.Log("�û����Ϲ���");
            // ����������ʼ����ƶ��������沢������Ϊ��ʼ״̬
            for (int i = 0; i < currentStartIndex - mailstartIndex; i++)
                MoveBottomItemToTop();
        }
        // ���µ�ǰstartIndexΪ�µ�ֵ
        currentStartIndex = mailstartIndex;
    }
    public void MoveTopItemToBottom()
    {
        // ����midPanelView.MailItemList�洢�������ʼ��������
        MailItem topItem = MailItemList[0];//�ѵ�ǰ�������Ԫ���ƶ���+�ʼ�����λ��
        //��¼��ǰ��λ��
        RectTransform topItemRect = topItem.GetComponent<RectTransform>();
        float curretntPosition = topItemRect.anchoredPosition.y;
        float newYposition = curretntPosition - currentItemHeight * visableItemCount - visableItemCount * itemSpacing;
        topItemRect.anchoredPosition = new Vector2(topItemRect.anchoredPosition.x, newYposition);
        MailItemList.RemoveAt(0);
        MailItemList.Add(topItem);
        // �����ƶ�����ʼ��������
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
        // �����ƶ�����ʼ��������
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
        //����index�ʼ�������ƶ����ƶ�֮���ٸ����ʼ�������
        //i 5
        for (int i = 0; i < visableItemCount; i++)//��˼����ʾ��ʼ����֮��Ŀɼ����ʼ�
        {
            int mailIndex = startIndex + i;
            if (mailIndex < MailBox.Instance.mailList.Count)
            {
                // ��ȡ��Ӧ���ʼ�����
                MailData mail = MailBox.Instance.mailList[mailIndex];
                // ��ȡ��Ӧ���ʼ���GameObject
                MailItem mailItem = MailItemList[i];
                // �����ʼ�������ʾ��ǰ�ʼ�����,���������ʼ����ݻ�Ҫ����������ʾui
                mailItem.gameObject.SetActive(true);
                ResetMailState(MailItemList[i]);
                setMailItem(mail, mailItem);
            }
            else
            {
                // �����ǰ���������ʼ��б�Χ�����ض�����ʼ���
               MailItemList[i].gameObject.SetActive(false);
            }
        }
        if (MailBox.Instance.mailList.Count <= 0)
        {
            midPanel.LeftNull.gameObject.SetActive(true);
            midPanel.RightNull.gameObject.SetActive(true);
            //û���ʼ���ʱ������Ϊfalse
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
        //��mailInfo�еĶ�Ӧ��id��ֵ
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
        // ����ʹ����ǰ����ͬ��TimeAgo�߼�
        if (timeSince.TotalDays >= 1) return $"{(int)timeSince.TotalDays}��ǰ";
        if (timeSince.TotalHours >= 1) return $"{(int)timeSince.TotalHours}Сʱǰ";
        if (timeSince.TotalMinutes >= 1) return $"{(int)timeSince.TotalMinutes}����ǰ";
        return "�ո�";
    }
    public void SetisReady(MailData mail, MailItem mailItem)
    {
        if (mail.isRead)
        {
            //�����ʧ
            mailItem.isRead.gameObject.SetActive(false);
            SetTransparency(mailItem);//��͸��
        }
        else
        {
            mailItem.isRead.gameObject.SetActive(true);
            SetNoTransparency(mailItem);//��͸��
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
        //Debug.Log("����͸����Ϊ1");
        Color color = mailItem.Mail.color;
        color.a = 1f; // ����͸���ȣ�0.5Ϊ��͸��
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
            showSelectImage(mailItem);//���ɫ

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
        // position.y ��ScrollRect�Ĵ�ֱ��һ��λ�ã�0��ʾ�ڵײ���1��ʾ�ڶ���
        // ע��Unity UI�Ĺ����������¹���ʱposition.y��С�����Ϲ���ʱposition.y����
        // ���㵱ǰ�ӿڶ�����������ݶ����ľ���
        float contentTopToViewportTop = (1f - position.y) * (contentHeight - viewportHeight);
        int startIndex = Mathf.FloorToInt(contentTopToViewportTop / itemHeight);
        // ȷ��startIndex���Ͽɼ����������ᳬ���ʼ��б������
        startIndex = Mathf.Clamp(startIndex, 0, Mathf.Max(0, totalItems - visableItemCount));
        mailstartIndex = startIndex;
        endIndex = startIndex + visableItemCount;
        //Debug.Log("startIndex" + startIndex.ToString());
        return startIndex;
    }//startindex�ǵ�һ��Ҫ��ʾ������
    private void ClickedMail(MailItem mailItem)
    {
        if (LastSelectMail != null) 
        {
            LastSelectMail.isSelect = false;
        }
        Debug.Log(mailItem.Mailid);
        foreach (MailData mail in MailBox.Instance.mailList)
        {
            if (mail.mailId == mailItem.Mailid) //ѡ��
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
