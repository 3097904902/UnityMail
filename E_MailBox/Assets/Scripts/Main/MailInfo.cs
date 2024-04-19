using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MailInfo : MonoBehaviour,IPointerClickHandler
{
    public static event UnityAction<GameObject> OnClickedMailItem;
    public Image Mail;
    public Image ImageReward; 
    public Image Select;
    public Image ImageMessage;
    public Image isRead;
    public TextMeshProUGUI MailTitle;
    public TextMeshProUGUI MailSendName;
    public TextMeshProUGUI MailTime;
    public GameObject Attachment;
    public  int Mailid;
    public DateTime SentTime;
    public bool isSelect;
    private void Update()
    {
        MailTime.text = TimeAgo(SentTime);//更新时间
    }

    private string TimeAgo(DateTime sentTime)
    {
        TimeSpan timeSince = DateTime.Now - sentTime;
        // 这里使用与前面相同的TimeAgo逻辑
        if (timeSince.TotalDays >= 1) return $"{(int)timeSince.TotalDays}天前";
        if (timeSince.TotalHours >= 1) return $"{(int)timeSince.TotalHours}小时前";
        if (timeSince.TotalMinutes >= 1) return $"{(int)timeSince.TotalMinutes}分钟前";
        return "刚刚";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (OnClickedMailItem != null) 
        {
            OnClickedMailItem(this.gameObject);
        }

    }
    public void ResetState() 
    {
        ImageReward.gameObject.SetActive(false);
        ImageMessage.gameObject.SetActive(true);
        isRead.gameObject.SetActive(true);
        Select.gameObject.SetActive(false);
    }
}
