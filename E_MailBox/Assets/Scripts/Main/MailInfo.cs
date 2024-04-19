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
        MailTime.text = TimeAgo(SentTime);//����ʱ��
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
