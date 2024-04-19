using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TopPanelView : MonoBehaviour
{
    public Text MailCountText;
    public void updateInfo() 
    {
        MailCountText.text = MailBox.Instance.mailList.Count.ToString() + "/" + MailBox.Instance.MailMaxCount.ToString();
        Debug.Log(MailBox.Instance.mailList.Count);
    }
}
