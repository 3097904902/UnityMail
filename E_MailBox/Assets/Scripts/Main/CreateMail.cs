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
        MailBox.Instance.CreateMail("������Ӫ�Ŷ�", "�������", "<<����>>ԤԼ����ý��ע����",
                "�װ�����ң�\n����ά���Ѿ�����������ά���ڼ����޷�������¼��Ϸ," +
                "��Ŀ������������²�����ϣ��������Ϸ��죡", DateTime.Now);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
