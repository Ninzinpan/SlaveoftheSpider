using UnityEngine;
using System.Collections.Generic;

// 1. 敵の行動の種類
public enum EnemyActionType
{
    Attack, // 攻撃
    Defend, // 防御
    Buff,   // 力アップなど
    Debuff  // プレイヤーを弱体化
}

// 2. 敵の行動データ (1ターン分の行動)
[System.Serializable]
public class EnemyAction
{
    public EnemyActionType actionType; // 何をするか
    public int value;                  // ダメージ量など
    public Sprite intentIcon;          // プレイヤーに見せる「予告アイコン」
}

// 3. エネミーデータ本体
[CreateAssetMenu(fileName = "NewEnemy", menuName = "SlaveOfTheSpider/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Stats")]
    [SerializeField] private string enemyName;
    [SerializeField] private int maxHP;
    [SerializeField] private Sprite sprite; // 敵の立ち絵画像

    [Header("Behavior")]
    [Tooltip("行動パターンのリスト。上から順に実行し、最後まで行ったら最初に戻るループを想定")]
    [SerializeField] private List<EnemyAction> actions;

    // --- データの取得用プロパティ (カプセル化) ---
    public string EnemyName => enemyName;
    public int MaxHP => maxHP;
    public Sprite Sprite => sprite;

    // 安全装置付きの行動リスト取得
    public List<EnemyAction> Actions => actions ?? new List<EnemyAction>();
}