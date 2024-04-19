using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollEvent : MonoBehaviour
{
    public ScrollRect scrollRect;
    public MidPanelController midPanelController;
    public MidPanelView midPanelView;
    private void Awake()
    {
       
    }
    void Start()
    {
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.AddListener(OnScroll);//���������¼�
        }
    }

    void OnScroll(Vector2 position)
    {
        // ���ﴦ������¼�
        //Debug.Log("����λ��: " + position);
        int totalItems = MailBox.Instance.mailList.Count;
        float itemHeight = midPanelController.currentItemHeight + midPanelController.Spacing; //
        float viewportHeight = scrollRect.viewport.rect.height;
        float contentHeight = scrollRect.content.rect.height;// ��ֱ��ʹ��scrollRect.content.rect.height
        
        int index = CalculateStartIndexFromPosition(position, totalItems, itemHeight, viewportHeight, contentHeight);
        //���ܵ��ǵ�ǰ���ӷ�Χ�ڵ�һ��������
        midPanelController.OnScrollChanged();
        midPanelController.UpdateVisibleMailItems(index);//��������

    }
    int CalculateStartIndexFromPosition(Vector2 position, int totalItems, float itemHeight, float viewportHeight, float contentHeight)
    {
        // position.y ��ScrollRect�Ĵ�ֱ��һ��λ�ã�0��ʾ�ڵײ���1��ʾ�ڶ���
        // ע��Unity UI�Ĺ����������¹���ʱposition.y��С�����Ϲ���ʱposition.y����
        // ���㵱ǰ�ӿڶ�����������ݶ����ľ���
        float contentTopToViewportTop = (1f - position.y) * (contentHeight - viewportHeight);
        //Debug.Log(position);
        // ���ݵ�ǰ�Ĺ���λ�ü���startIndex
        int startIndex = Mathf.FloorToInt(contentTopToViewportTop / itemHeight);
        // ȷ��startIndex���Ͽɼ����������ᳬ���ʼ��б������
        startIndex = Mathf.Clamp(startIndex, 0, Mathf.Max(0, totalItems - midPanelController.visableItemCount));
        midPanelController.mailstartIndex = startIndex;
        midPanelController.endIndex = startIndex + midPanelController.visableItemCount;
        Debug.Log("startIndex"+startIndex.ToString());
        return startIndex;
    }//startindex�ǵ�һ��Ҫ��ʾ������
    
}