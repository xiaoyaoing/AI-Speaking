using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MessageQueueManager : MonoBehaviour
{
    public GameObject messagePrefab; // 拖拽TextMeshPro Prefab到这里
    public int maxMessages = 3; // 最大消息数
    float ySpacing = 220f; // 消息间的垂直间隔

    private Queue<GameObject> messages = new Queue<GameObject>();
    private string[] messageOptions = new string[] {
        "good",
        "take it easy",
        "be confident",
        "perfect",
        "you're doing great",
        "be clear",
        "don't mind making mistakes",
        "introduce your work briefly and focus on your insights",
        "try your best"
    };

    void Start()
    {
        StartCoroutine(SendRandomMessage());
    }

    public void SendMessage(string text)
    {
        if (messages.Count >= maxMessages)
        {
            Destroy(messages.Dequeue()); // 移除旧消息
        }
        
        foreach (GameObject msg in messages)
        {
            msg.transform.localPosition += new Vector3(0, ySpacing, 0); // 向上移动每个消息
        }

        GameObject newMessage = Instantiate(messagePrefab, transform);
        StartCoroutine(TypeText(newMessage.GetComponentInChildren<TextMeshProUGUI>(), text));
        newMessage.transform.SetAsFirstSibling(); // 确保新消息总是在顶部

        messages.Enqueue(newMessage);
    }
    
    IEnumerator TypeText(TextMeshProUGUI textComponent, string text)
    {
        textComponent.text = ""; // 清空文本
        foreach (char c in text)
        {
            textComponent.text += c; // 逐字符添加文本
            yield return new WaitForSeconds(0.1f); // 等待一定时间后添加下一个字符
        }
    }

    IEnumerator SendRandomMessage()
    {
        while (true) // 无限循环
        {
            string randomMessage = messageOptions[Random.Range(0, messageOptions.Length)]; // 随机选择一个消息
            SendMessage(randomMessage); // 调用函数发送消息
            yield return new WaitForSeconds(10f); // 等待10秒
        }
    }
}