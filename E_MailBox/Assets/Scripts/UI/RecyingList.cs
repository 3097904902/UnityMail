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
    [Header("�ʼ�������Ϣ")]
    public float itemSpacing = 10f;//item֮��ļ��
    public float currentItemHeight;//��ǰ�ʼ��ĸ߶�
    public float currentItemWidth;//��ǰ�ʼ��Ŀ��
    public float currentContentSize;
    [Header("�ʼ�������")]
    public int mailstartIndex;
    public int currentStartIndex;
    public int visiableEndIndex;
    public int endIndex;
    [Header("�ʼ���ʵ������")]
    public int ListItemCount;//���õ��б�ʵ���ĸ���
    public int VisableCount;//����ͼ��ɼ����б����
    [Header("�ӿڵĸ߶�")]
    public float viewportHeight;
    [Header("��ǰ�ʼ��ĸ���")]
    public int MailCounts;//�ʼ��ĸ���
    [Header("�߶����")]
    public float PreAllocHeight = 0;//����Ԥ���ĸ߶�
    protected float previousBuildHeight = 0;//֮ǰ�ĸ߶�
    [Tooltip("�Ƿ�ִ�й����仯����")]                                 
    public bool ignoreScrollChange;
    public event UnityAction<MailData,MailItem> OnSetMailItem;//�����ʼ��ľ������ݺ���ʾ
    private void OnEnable()
    {
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.AddListener(OnScroll);//���������¼�
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
    public void UpdateContentSize(int MailCounts, float height) //����Content�ĸ߶�
    {
        currentContentSize = height * MailCounts + (MailCounts - 1) * (itemSpacing);
        scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, currentContentSize);
    }
    public void InitRecyingList() 
    {
        //ʵ������ʱ�����ú�λ�ã������ʼ���index���ú�λ�ú͸߶�
        for (int i = 0; i < ListItemCount; i++)
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
            obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, posY);
            MailItem mailItem = obj.GetComponent<MailItem>();
            MailItemList.Add(mailItem);
        }
        RefreshAll(); // ˢ��
    }
    public void RefreshAll()
    {
        
        for (int i = 0; i < ListItemCount; i++)//��˼����ʾ��ʼ����֮��Ŀɼ����ʼ�
        {
            int mailIndex = mailstartIndex + i;
            if (mailIndex < MailBox.Instance.mailList.Count)
            {
                // ��ȡ��Ӧ���ʼ�����
                MailData mail = MailBox.Instance.mailList[mailIndex];
                // ��ȡ��Ӧ���ʼ���GameObject
                MailItem mailItem = MailItemList[i];
                // �����ʼ�������ʾ��ǰ�ʼ�����,���������ʼ����ݻ�Ҫ����������ʾui
                mailItem.gameObject.SetActive(true);
                RefreshItem(mail,MailItemList[i]);
            }
            else
            {
                // �����ǰ���������ʼ��б�Χ�����ض�����ʼ���
               MailItemList[i].gameObject.SetActive(false);
            }
        }
        // ���ض�����ʼ���
      
        UpdateContentSize(MailBox.Instance.mailList.Count, currentItemHeight);
    }
    public void RefreshItem(MailData mail,MailItem mailItem)//ˢ��ָ����item
    {
        if (OnSetMailItem == null)
        {
            Debug.Log("RecyclingListView is missing an ItemCallback, cannot function", this);
            return;
        }
        else 
        {
            OnSetMailItem(mail,mailItem);//��������
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
        // ���startIndex���ӣ���ʾ�û����¹���
        if (mailstartIndex > currentStartIndex)
        {
            //Debug.Log("�û����¹���");
            // ������������ʼ����ƶ��������沢������Ϊ��ʼ״̬
            for (int i = 0; i < mailstartIndex - currentStartIndex; i++)
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
    public void CalculateStartIndexFromPosition(Vector2 position, int totalItems, float itemHeight, float viewportHeight, float contentHeight)
    {
        // position.y ��ScrollRect�Ĵ�ֱ��һ��λ�ã�0��ʾ�ڵײ���1��ʾ�ڶ���
        // ע��Unity UI�Ĺ����������¹���ʱposition.y��С�����Ϲ���ʱposition.y����
        // ���㵱ǰ�ӿڶ�����������ݶ����ľ���
        float contentTopToViewportTop = (1f - position.y) * (contentHeight - viewportHeight);
        int startIndex = Mathf.FloorToInt(contentTopToViewportTop / itemHeight);
        // ȷ��startIndex���Ͽɼ����������ᳬ���ʼ��б������
        startIndex = Mathf.Clamp(startIndex, 0, Mathf.Max(0, totalItems - ListItemCount));
        mailstartIndex = startIndex;
        Debug.Log("MailStartIndex" + mailstartIndex);
        endIndex = startIndex + ListItemCount - 1;
        visiableEndIndex = startIndex + VisableCount - 1;
    }//mailstartIndex�ǵ�һ��Ҫ��ʾ������
    public void MoveTopItemToBottom()
    {
        // ����midPanelView.MailItemList�洢�������ʼ��������
        MailItem topItem = MailItemList[0];//�ѵ�ǰ�������Ԫ���ƶ���+�ʼ�����λ��
        //��¼��ǰ��λ��
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
        // �����ƶ�����ʼ��������
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
        return itemSpacing + currentItemHeight;//�߶�+���
    }
    public void SetVisableCount() 
    {
        int Count = Mathf.RoundToInt(viewportHeight / RowHeight());
        VisableCount = Count;
    }
}
