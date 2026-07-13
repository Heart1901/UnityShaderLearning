using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class HandCardLayout : MonoBehaviour
{
    // 扇形参数
    public float fanRadius = 3f;
    public float totalAngle = 120f;
    public GameObject cardPrefab;
    private List<GameObject> cardsInHand = new List<GameObject>();

    // 卡牌限制
    public int maxCards = 7;

    private void Update()
    {
        // 检测键盘输入
        HandleKeyboardInput();
    }

    // 处理键盘输入
    private void HandleKeyboardInput()
    {
        // 按 A 键添加卡牌
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (cardsInHand.Count < maxCards)
            {
                AddCardToHand();

            }
            else
            {
                
            }
        }

        // 按 S 键删除卡牌（删除最右侧的卡牌，即列表最后一张）
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (cardsInHand.Count > 0)
            {
                RemoveCardFromHand(cardsInHand[cardsInHand.Count - 1]);
                //Debug.Log("删除卡牌成功！当前手牌数量：" + cardsInHand.Count);
            }
            else
            {
                //Debug.Log("没有卡牌可删除！");
            }
        }
    }

    // 排版核心逻辑
    // 排版核心逻辑
    public void LayoutCards()
    {
        
        int cardCount = cardsInHand.Count;
        if (cardCount == 0) return;
        UnityEngine.Debug.Log(cardCount);
    /*    if (cardCount == 1) {
            totalAngle = 90;
            fanRadius = 80;
        }
        else if (cardCount == 2) 
        {
            totalAngle = 50;
            fanRadius = 30;
         }
        else if (cardCount == 3)
        {
            totalAngle = 50;
            fanRadius = 40;
        }
        else if (cardCount == 4)
        {
            totalAngle = 90;
            fanRadius = 80;
        }
        else if (cardCount == 5)
        {
            totalAngle = 90;
            fanRadius = 80;
        }
        else if (cardCount == 6)
        {
            totalAngle = 90;
            fanRadius = 80;
        }
        else if (cardCount == 7)
        {
            totalAngle = 90;
            fanRadius = 80;
        }*/



        float anglePerCard = totalAngle / (cardCount - 1);
        float startAngle = -totalAngle / 2;

        for (int i = 0; i < cardCount; i++)
        {
            GameObject card = cardsInHand[i];
            if (card == null) continue;

            float currentAngle = startAngle + anglePerCard * i;
            float radian = currentAngle * Mathf.Deg2Rad;

            float xPos = fanRadius * Mathf.Sin(radian);
            float yPos = fanRadius * Mathf.Cos(radian);
            card.transform.localPosition = new Vector3(xPos, yPos, 0);

            // 修改旋转计算：只绕 Z 轴旋转，使用当前卡牌的角度
            card.transform.rotation = Quaternion.Euler(0, 0, -currentAngle);
        }
    }

    // 添加卡牌
    public void AddCardToHand()
    {
        GameObject newCard = Instantiate(cardPrefab, transform);
        cardsInHand.Add(newCard);
        LayoutCards();
    }

    // 移除卡牌
    public void RemoveCardFromHand(GameObject card)
    {
        cardsInHand.Remove(card);
        Destroy(card);
        LayoutCards();
    }
}