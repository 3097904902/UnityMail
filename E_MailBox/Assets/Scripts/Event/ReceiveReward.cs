using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReceiveReward : MonoBehaviour
{
    public Button btnReceiveReward;
    public int MailID;
    void Start()
    {
        if (btnReceiveReward != null)
        {
            btnReceiveReward.onClick.AddListener(ReceiveClicked);
        }
    }
    public void ReceiveClicked() 
    {
        if (btnReceiveReward != null) 
        {
            MailBox.Instance.ReceiveReward(MailID);
        }
    }
}
