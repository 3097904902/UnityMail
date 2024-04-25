using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MailItem : MonoBehaviour, IPointerClickHandler
{
    public static event UnityAction<MailItem> OnClickedMailItem;
    public Image Mail;
    public Image ImageReward;
    public Image Select;
    public Image ImageMessage;
    public Image isRead;
    public TextMeshProUGUI MailTitle;
    public TextMeshProUGUI MailSendName;
    public TextMeshProUGUI MailTime;
    public GameObject Attachment;
    public int Mailid;
    public DateTime SentTime;
    public bool isSelect;
    public bool BoolIsRead;
    public MailItem LastSeletct;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (OnClickedMailItem != null)
        {
            OnClickedMailItem(this);//把自己传出去
        }
    }
   
}
