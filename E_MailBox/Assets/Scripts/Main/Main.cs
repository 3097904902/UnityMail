using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    public MidPanelController midPanelController;
    private void Awake()
    {
        MailBox.Instance.LoadIdCount();
    }
    void Start()
    {
        if (midPanelController != null)
        {
            midPanelController.InitMidPanelView();
        }
    }
    private void OnEnable()
    {
        MailBox.Instance.LoadMailsFromXml("mails.xml");
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)) 
        {
            MailBox.Instance.InitMail();
        }
    }
    private void OnDestroy()
    {
        MailBox.Instance.SaveMailsToXml("mails.xml");
        MailBox.Instance.SaveIdCount();
    }
}
