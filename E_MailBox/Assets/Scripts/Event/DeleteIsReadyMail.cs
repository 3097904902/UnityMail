using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeleteIsReadyMail : MonoBehaviour
{
    public Button btnDeleteIsReady;
    void Start()
    {
        if (btnDeleteIsReady != null)
        {
            btnDeleteIsReady.onClick.AddListener(DeleteIsReady);
        }
    }
    private void DeleteIsReady()
    {
        MailBox.Instance.DeleteIsReadyMail();
        
    }

    void Update()
    {
        
    }
}
