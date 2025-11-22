using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewPlayerData", menuName = "SlaveOfTheSpider/PlayerBaseData")]
public class PlayerBaseData : ScriptableObject
{
    [Header("Status")]
    [SerializeField] private int maxHP = 80;       // 初期の最大HP
    [SerializeField] private int baseEnergy = 3;   // ターンごとの回復エナジー
    [SerializeField] private int baseDrawCount = 5; // ターンごとに引く枚数

    [Header("Visual")]
    [SerializeField] private Sprite playerSprite;  // プレイヤーの画像

    [Header("Deck")]
    [Tooltip("ゲーム開始時に持っているカードのリスト")]
    [SerializeField] private List<CardData> startingDeck;

    // --- データの取得用プロパティ (カプセル化) ---
    public int MaxHP => maxHP;
    public int BaseEnergy => baseEnergy;
    public int BaseDrawCount => baseDrawCount;
    public Sprite PlayerSprite => playerSprite;

    // 安全装置付きのデッキ取得
    public List<CardData> StartingDeck => startingDeck ?? new List<CardData>();
}