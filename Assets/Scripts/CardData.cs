using UnityEngine;
using System.Collections.Generic; // Listを使うために必要

// 1. カードの種類を定義 (Enum)
public enum CardType
{
    Attack, // 攻撃
    Skill,  // スキル (防御やドローなど)
    Power   // パワー (永続効果など)
}

// 2. 効果の種類を定義 (Enum)
public enum EffectType
{
    Damage, // ダメージ
    Block,  // ブロック(防御)
    Draw    // カードを引く
    // 必要に応じて Energy (エナジー回復) などを追加
}
public enum TargetType
{
    Enemy,      // 敵単体
    AllEnemies, // 敵全体
    Self        // 自分自身
}

// 3. 効果の中身を定義するクラス
// [System.Serializable] をつけることで、Unityのインスペクタでリスト表示できるようになる
[System.Serializable]
public class CardEffect
{
    public EffectType effectType; // 何をするか
    public int value;             // どのくらいするか (ダメージ量やドロー枚数)
    public int duration;          // 効果の持続ターン数 (必要に応じて)
}

// 4. カードデータ本体 (ScriptableObject)
[CreateAssetMenu(fileName = "NewCard", menuName = "SlaveOfTheSpider/CardData")]
public class CardData : ScriptableObject
{
    [Header("Basic Info")]
    [SerializeField] private string cardName;      // カード名
    [SerializeField] private int cost;             // コスト
    [SerializeField] private Sprite icon;          // 絵
    [SerializeField] [TextArea] private string description; // 説明文
    [SerializeField] private CardType cardType;    // 種類
    [SerializeField] private TargetType targetType; // ターゲットタイプ

    [Header("Abilities")]
    [SerializeField] private List<CardEffect> effects; // 効果のリスト

    // --- データの取得用プロパティ (カプセル化) ---
    
    // Pythonの getter のようなもの。外部からは読むだけで、書き換えられない。
    public string CardName => cardName;
    public int Cost => cost;
    public Sprite Icon => icon;
    public string Description => description;
    public CardType Type => cardType;
    public TargetType Target => targetType;
    
    // 安全装置: リストが空(null)の場合に備えて、空のリストを返すかそのまま返す
    public List<CardEffect> Effects => effects ?? new List<CardEffect>();
}