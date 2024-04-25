using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
[Serializable]
public class MailData
{
    public string sender;//�ʼ�������
    public string recipient;//�ʼ�������
    public int mailId;//�ʼ���ID
    public string mailTitle;//�ʼ��ı���
    public string mailContent;//�ʼ�������
    public bool isRead;//�ʼ����Ѷ�״̬
    public DateTime sentTime;//�ʼ��ķ���ʱ��
    public bool isReward;//�Ƿ��������
    public bool isCompleted;//�Ƿ�Ϊ���״̬
    public bool isSelect;//�Ƿ�ѡ��                    
    public int PriorityScore
    {
        get
        {
            if (!isRead && isReward) return 1; // δ���н���
            if (isRead && isReward && !isCompleted) return 2; // �Ѷ�����δ���
            if (!isRead && !isReward) return 3; // δ���޽���
            return 4; // �Ѷ��޽���
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
