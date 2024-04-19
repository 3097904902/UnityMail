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
        // 检查屏幕尺寸是否发生变化
        if (Screen.width != lastScreenSize.x || Screen.height != lastScreenSize.y)
        {
            // 更新记录的屏幕尺寸
            lastScreenSize.x = Screen.width;
            lastScreenSize.y = Screen.height;

            // 获取Viewport的新尺寸
            GetViewportSize();
        }
    }
    void GetViewportSize()
    {
        //清除列表重新初始化
        foreach (GameObject obj in midPanelView.MailItemList) 
        {
            Destroy(obj);
        }
        midPanelView.MailItemList.Clear();
        panelController.InitInfo();
  
    }
}
