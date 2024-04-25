using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
[Serializable]
public class MailData
{
    public string sender;//邮件发送者
    public string recipient;//邮件接受者
    public int mailId;//邮件的ID
    public string mailTitle;//邮件的标题
    public string mailContent;//邮件的内容
    public bool isRead;//邮件的已读状态
    public DateTime sentTime;//邮件的发送时间
    public bool isReward;//是否包含奖励
    public bool isCompleted;//是否为完成状态
    public bool isSelect;//是否被选中                    
    public int PriorityScore
    {
        get
        {
            if (!isRead && isReward) return 1; // 未读有奖励
            if (isRead && isReward && !isCompleted) return 2; // 已读奖励未完成
            if (!isRead && !isReward) return 3; // 未读无奖励
            return 4; // 已读无奖励
        }
    }
    public void InitMail(string sender, string recipient, int mailId, string mailTitle, string mailContent, DateTime sentTime) 
    {
        this.sender = sender;
        this.recipient = recipient;
        this.mailId = mailId;
        this.mailTitle = mailTitle;
        this.mailContent = mailContent;
        this.isRead = false;
        this.sentTime = sentTime;
    }
    public void setMailisRead() 
    {
        this.isRead = true;
    }
}
