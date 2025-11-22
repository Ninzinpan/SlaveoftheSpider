using UnityEngine;
using System.Collections.Generic;

// 敵の「1回分の行動」定義
// カードと同じく、複数の効果(CardEffect)を持てるようにする
[System.Serializable]
public class EnemyAction
{
    public string actionName;        // 行動名 (例: "強酸の唾")
    public Sprite intentIcon;        // プレイヤーに見せる予告アイコン
    public List<CardEffect> effects; // 効果のリスト (例: 7ダメージ + 2毒)
}

[CreateAssetMenu(fileName = "NewEnemy", menuName = "SlaveOfTheSpider/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Stats")]
    [SerializeField] private string enemyName;
    [SerializeField] private int maxHP;
    [SerializeField] private Sprite sprite;

    [Header("Behavior Deck")]
    [Tooltip("行動パターンのリスト。上から順に実行し、最後まで行ったら最初に戻る")]
    [SerializeField] private List<EnemyAction> actionPattern;

    // --- 外部公開用プロパティ ---
    public string EnemyName => enemyName;
    public int MaxHP => maxHP;
    public Sprite Sprite => sprite;

    // 行動パターンを取得
    public List<EnemyAction> ActionPattern => actionPattern ?? new List<EnemyAction>();
}