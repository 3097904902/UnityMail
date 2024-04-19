using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Xml.Serialization;
using System.IO;
using System.Linq;

public class MailBox
{ 
    public event UnityAction OnLoadFromXML;
    public event UnityAction OnMailCount;
    private static MailBox _instance;
    public List<MailData> mailList = new List<MailData>();
    public static int IdCount;//�ʼ�ID������
    public int MailMaxCount = 1000;
   
    private MailBox() { }
    public static MailBox Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new MailBox();
            }
            return _instance;
        }
    }
    public List<MailData> SortMails(List<MailData> mails)
    {
        var sortedMails = mails
        .OrderByDescending(mail => !mail.isRead)
        .ThenByDescending(mail => !mail.isCompleted)
        .ThenByDescending(mail => mail.isReward)
        .ThenByDescending(mail => mail.sentTime)
        .ToList();
        // ��ӡ��־�Լ��������
        //foreach (var mail in sortedMails)
        //{
        //    Debug.Log($"Mail ID: {mail.mailId}, Completed: {mail.isCompleted}, Reward: {mail.isReward}, Sent Time: {mail.sentTime}");
        //}
        return sortedMails;
    }
    public void SaveIdCount()
    {
        // �����ļ�����·��
        string filePath = Path.Combine(Application.persistentDataPath, "IdCount.txt");

        // ��IdCountֵд���ļ�
        File.WriteAllText(filePath, IdCount.ToString());
    }
    public void LoadIdCount()
    {
        // �����ļ�����·��
        string filePath = Path.Combine(Application.persistentDataPath, "IdCount.txt");

        // ����ļ��Ƿ����
        if (File.Exists(filePath))
        {
            // ��ȡ�ļ����ݲ�����Ϊint
            string content = File.ReadAllText(filePath);
            IdCount = int.Parse(content);
        }
        else
        {
            Debug.Log("IdCount file does not exist. Starting with IdCount = 0.");
            IdCount = 1; // ���κ���ϣ���ĳ�ʼֵ
        }
    }

    public void SaveMailsToXml(string fileName)
    {
        string folderPath = Path.Combine(Application.persistentDataPath, "XML");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, fileName);
        var serializer = new XmlSerializer(typeof(List<MailData>));

        using (var stream = File.Create(filePath))
        {
            serializer.Serialize(stream, mailList);
        }
    }
    public void LoadMailsFromXml(string fileName)
    {
        string filePath = Path.Combine(Application.persistentDataPath, "XML", fileName);
        if (File.Exists(filePath))
        {
            var serializer = new XmlSerializer(typeof(List<MailData>));
            using (var stream = File.OpenRead(filePath))
            {
                mailList = (List<MailData>)serializer.Deserialize(stream);
            }
        }
        else
        {
            Debug.LogWarning("Mail file not found: " + filePath);
        }
        mailList = SortMails(mailList);
        if (OnLoadFromXML != null)
        {
            OnLoadFromXML();
        }
        showMailCount();
    }

    public void CreateMail(string sender, string recipient, string mailTitle, string mailContent, DateTime sentTime)
    {
        if (isOverMail())
        {
            isOverDeleteMail();
        }
        //�����ʼ�
        MailData maildata = new MailData();
        maildata.InitMail(sender, recipient, IdCount, mailTitle, mailContent, sentTime);
        //�ʼ�Id����
        IdCount++;
        mailList.Add(maildata);
        //showMailCount();
        SaveMailsToXml("mails.xml");
        LoadMailsFromXml("mails.xml");
    }
    public void CreateMailInit(string sender, string recipient, string mailTitle, string mailContent, DateTime sentTime) 
    {
        if (isOverMail())
        {
            isOverDeleteMail();
        }
        //�����ʼ�
        MailData maildata = new MailData();
        maildata.InitMail(sender, recipient, IdCount, mailTitle, mailContent, sentTime);
        //�ʼ�Id����
        IdCount++;
        mailList.Add(maildata);
        SaveMailsToXml("mails.xml");
        LoadMailsFromXml("mails.xml");
    }
    public void InitMail()
    {
        //��ʼ���ʼ������ʼ����͹�ȥ
        for (int i = 0; i < 10; i++)
        {
            CreateMailInit("������Ӫ�Ŷ�", "�������", "������ԤԼ����ý��ע������",
                "�װ�����ң�\n����ά���Ѿ�����������ά���ڼ����޷�������¼��Ϸ," +
                "��Ŀ������������²�����ϣ��������Ϸ��죡", DateTime.Now);
        }
        //RemoveMail(3);
        mailList[4].isReward = true;
        mailList[5].isReward = true;
        CreateMailInit("������", "�����޵�����ħ������սʿ", "��",
                "����սʿ��\n����ά���Ѿ�����������ά���ڼ����޷�������¼��Ϸ," +
                "��Ŀ������������²�����ϣ��������Ϸ��죡", DateTime.Now);
        SaveMailsToXml("mails.xml");
        LoadMailsFromXml("mails.xml");
    }
    public bool isOverMail()
    {
        if (mailList.Count >= MailMaxCount)
            return true;
        else
            return false;
    }
    public void isOverDeleteMail() 
    {
        if (mailList.Count == 0) return;
        var mailToDelete = mailList.OrderByDescending(m => m.PriorityScore).First();
        mailList.Remove(mailToDelete);
        SaveMailsToXml("mails.xml");
        LoadMailsFromXml("mails.xml");
    }
    public void RemoveMail(int mailId)
    {
        mailList.RemoveAll(mail => mail.mailId == mailId);
        SaveMailsToXml("mails.xml");
        LoadMailsFromXml("mails.xml");
        showMailCount();

    }
    public void DeleteIsReadyMail() 
    {
        mailList.RemoveAll(mail => mail.isRead==true && !(mail.isReward == true && mail.isCompleted == false));
        SaveMailsToXml("mails.xml");
        LoadMailsFromXml("mails.xml");
        showMailCount();
    }
    public void ReceiveReward(int mailId) 
    {
        foreach (MailData mail in mailList) 
        {
            if (mail.mailId == mailId) 
            {
                mail.isCompleted = true;
            }
        }
        SaveMailsToXml("mails.xml");
        LoadMailsFromXml("mails.xml");
    }
    public void ReceiveAllReward() //��ȡȫ��
    {
        foreach (MailData mail in mailList)
        {
            if (mail.isReward)
            {
                if (!mail.isCompleted) 
                {
                    mail.isCompleted = true;
                    mail.isRead = true;
                }
                   
            }
        }
        SaveMailsToXml("mails.xml");
        LoadMailsFromXml("mails.xml");
    }
    public void setIsReady(MailData mail) 
    {
        mail.isRead = true;
        SaveMailsToXml("mails.xml");
    }
    public void setIsSelect(MailData mail) 
    {
        mail.isSelect = false;
        SaveMailsToXml("mails.xml");
    }
    public void showMailCount() 
    {
        if (OnMailCount != null) 
        {
            OnMailCount();
        }
    }
}
