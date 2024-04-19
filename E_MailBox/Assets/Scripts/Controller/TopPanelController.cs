using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopPanelController : MonoBehaviour
{
    TopPanelView topPanelView;
    private void Awake()
    {
        
    }
    private void Start()
    {
        MailBox.Instance.OnMailCount += showCount;
        topPanelView = this.gameObject.GetComponent<TopPanelView>();
    }
    private void OnDestroy()
    {
        MailBox.Instance.OnMailCount -= showCount;
    }
    private void showCount()
    {
        if (topPanelView != null) 
        {
            topPanelView.updateInfo();
        }
    }
}
