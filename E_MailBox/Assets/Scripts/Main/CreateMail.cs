using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CreateMail : MonoBehaviour
{
    public Button btnCreateMail;
    void Start()
    {
        if (btnCreateMail != null) 
        {
            btnCreateMail.onClick.AddListener(MyButtonClicked);
        }
    }
    private void MyButtonClicked()
    {
        MailBox.Instance.CreateMail("望月运营团队", "玩家名字", "<<望月>>预约及社媒关注奖励",
                "亲爱的玩家：\n本次维护已经结束，对于维护期间您无法正常登录游戏," +
                "项目组对您做出如下补偿，希望您能游戏愉快！", DateTime.Now);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
