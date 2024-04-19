using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewportSizeWatcher : MonoBehaviour
{
    private RectTransform viewportRectTransform;
    private Vector2 lastScreenSize;
    public MidPanelController panelController;
    public MidPanelView midPanelView;
    void Start()
    {
        viewportRectTransform = GetComponent<RectTransform>();
        lastScreenSize = new Vector2(Screen.width, Screen.height);
    }

    // Update is called once per frame
    void Update()
    {
        // �����Ļ�ߴ��Ƿ����仯
        if (Screen.width != lastScreenSize.x || Screen.height != lastScreenSize.y)
        {
            // ���¼�¼����Ļ�ߴ�
            lastScreenSize.x = Screen.width;
            lastScreenSize.y = Screen.height;

            // ��ȡViewport���³ߴ�
            GetViewportSize();
        }
    }
    void GetViewportSize()
    {
        //����б����³�ʼ��
        foreach (GameObject obj in midPanelView.MailItemList) 
        {
            Destroy(obj);
        }
        midPanelView.MailItemList.Clear();
        panelController.InitInfo();
  
    }
}
