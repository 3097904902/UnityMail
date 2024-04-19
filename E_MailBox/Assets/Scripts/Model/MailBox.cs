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
    public static int IdCount;//邮件ID计数器
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
        // 打印日志以检查排序结果
        //foreach (var mail in sortedMails)
        //{
        //    Debug.Log($"Mail ID: {mail.mailId}, Completed: {mail.isCompleted}, Reward: {mail.isReward}, Sent Time: {mail.sentTime}");
        //}
        return sortedMails;
    }
    public void SaveIdCount()
    {
        // 构建文件完整路径
        string filePath = Path.Combine(Application.persistentDataPath, "IdCount.txt");

        // 将IdCount值写入文件
        File.WriteAllText(filePath, IdCount.ToString());
    }
    public void LoadIdCount()
    {
        // 构建文件完整路径
        string filePath = Path.Combine(Application.persistentDataPath, "IdCount.txt");

        // 检查文件是否存在
        if (File.Exists(filePath))
        {
            // 读取文件内容并解析为int
            string content = File.ReadAllText(filePath);
            IdCount = int.Parse(content);
        }
        else
        {
            Debug.Log("IdCount file does not exist. Starting with IdCount = 0.");
            IdCount = 1; // 或任何你希望的初始值
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
        //创建邮件
        MailData maildata = new MailData();
        maildata.InitMail(sender, recipient, IdCount, mailTitle, mailContent, sentTime);
        //邮件Id递增
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
        //创建邮件
        MailData maildata = new MailData();
        maildata.InitMail(sender, recipient, IdCount, mailTitle, mailContent, sentTime);
        //邮件Id递增
        IdCount++;
        mailList.Add(maildata);
        SaveMailsToXml("mails.xml");
        LoadMailsFromXml("mails.xml");
    }
    public void InitMail()
    {
        //初始化邮件并把邮件发送过去
        for (int i = 0; i < 10; i++)
        {
            CreateMailInit("望月运营团队", "玩家名字", "《望月预约及社媒关注奖励》",
                "亲爱的玩家：\n本次维护已经结束，对于维护期间您无法正常登录游戏," +
                "项目组对您做出如下补偿，希望您能游戏愉快！", DateTime.Now);
        }
        //RemoveMail(3);
        mailList[4].isReward = true;
        mailList[5].isReward = true;
        CreateMailInit("神秘人", "超级无敌霹雳魔法暴龙战士", "无",
                "暴龙战士：\n本次维护已经结束，对于维护期间您无法正常登录游戏," +
                "项目组对您做出如下补偿，希望您能游戏愉快！", DateTime.Now);
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
    public void ReceiveAllReward() //领取全部
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
