using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReceiveAllReward : MonoBehaviour
{
    public Button btnReceiveAll;
    void Start()
    {
        if (btnReceiveAll != null)
        {
            btnReceiveAll.onClick.AddListener(MyButtonClicked);
        }
    }

    private void MyButtonClicked()
    {
        MailBox.Instance.ReceiveAllReward();
    }

}
