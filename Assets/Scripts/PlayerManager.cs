using UnityEngine;
using System.Collections.Generic;


public class PlayerManager : MonoBehaviour
{
    [Header("Base Data")]
    [SerializeField] private PlayerBaseData baseData;
    [SerializeField] private PlayerInfoUI infoUI; // 【追加】UI参照
    // ゲーム中に変動する「現在の所持デッキ」
    // private set なので、外部からは読み取り専用
    public List<CardData> MasterDeck {get; private set;} = new List<CardData>();
    public int CurrentEnergy { get; private set; } // 現在のエナジー
    public int MaxHP { get; private set; }    // 【追加】
    public int CurrentHP { get; private set; } // 【追加】

    private void Awake()
    {
        if (baseData != null)
        {
            MasterDeck = new List<CardData>(baseData.StartingDeck);

        }
    }
     // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 戦闘開始時、最大値まで回復
        if (baseData != null)
        {
            // 【追加】HPの初期化
            MaxHP = baseData.MaxHP;
            CurrentHP = MaxHP;
            CurrentEnergy = baseData.BaseEnergy;
            Debug.Log($"エナジー初期化: {CurrentEnergy}");
            if (infoUI != null)
            {
                infoUI.SetupHP(MaxHP, CurrentHP);
                infoUI.UpdateEnergy(CurrentEnergy);
            }
        }
    }
    // --- 【追加】ターン開始時の処理 ---
    public void OnTurnStart()
    {
        // エナジーを最大値まで回復
        if (baseData != null)
        {
            CurrentEnergy = baseData.BaseEnergy;
            
            // UI更新
            if (infoUI != null) infoUI.UpdateEnergy(CurrentEnergy);
        }
        
        Debug.Log("プレイヤーターン開始: エナジー回復");
    }
    public void ConsumeEnergy(int cost)
    {
        CurrentEnergy -= cost;
        if (CurrentEnergy < 0) CurrentEnergy = 0;
        // 【追加】UI更新
        if (infoUI != null) infoUI.UpdateEnergy(CurrentEnergy);
        Debug.Log($"エナジー消費: {cost} (残り: {CurrentEnergy})");
    }

// --- 【追加】ダメージを受ける処理 ---
    public void TakeDamage(int damage)
    {
        CurrentHP -= damage;
        if (CurrentHP < 0) CurrentHP = 0;

        // UI更新
        if (infoUI != null)
        {
            infoUI.UpdateHP(CurrentHP);
        }

        Debug.Log($"プレイヤー被弾: {damage} (残りHP: {CurrentHP})");

        // 死亡判定
        if (CurrentHP <= 0)
        {
            Debug.Log("GAME OVER...");
            // ここにゲームオーバー処理を追加する
        }
    }
}
