using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReMoveMail : MonoBehaviour
{
    public Button btndeleteMail;
    public int MailID;
    void Start()
    {
        if (btndeleteMail != null)
        {
            btndeleteMail.onClick.AddListener(MyButtonClicked);
        }
    }
    private void MyButtonClicked()
    {
        MailBox.Instance.RemoveMail(MailID);
    }
    void Update()
    {
        
    }
}
