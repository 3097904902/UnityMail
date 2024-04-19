using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MidPanelView : MonoBehaviour
{
    //�ؼ�
    public GameObject itemContent;//װitem��Content
    public GameObject MailContent;
    public GameObject scrollView;
    public Text contentTitle;
    public Text Time;
    public Text contentSendMessage;
    public Text contentBody;
    public GameObject attachment;
    public Button btnDeleteAlready;
    public Button btnRecieveAll;
    public Button btnDelete;
    public Button btnRecieve;
    public GameObject MailPrefab;
    public Image LeftNull;
    public Image RightNull;
    public Transform ScrollViewContent;
    public RectTransform ScrollViewTransform;
    public RectTransform MailitemPrefabSize;
    public List<GameObject> MailItemList = new List<GameObject>();
    public MidPanelController controller;
    private void Awake()
    {
        controller = GetComponent<MidPanelController>();
    }
    public void UpdateInfo(MailBox mailbox)//�����ʼ��б�����ʾ
    {
        controller.ReloadMailItems();
    }
}