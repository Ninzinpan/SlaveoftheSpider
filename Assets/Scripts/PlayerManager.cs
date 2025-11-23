using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI; // 【追加】Imageコンポーネントを扱うために必要
using UnityEngine.EventSystems;
using System.Collections;
using System.Linq.Expressions;
public class PlayerManager : MonoBehaviour,IPointerClickHandler
{
    [Header("Base Data")]
    [SerializeField] private PlayerBaseData baseData;
    [SerializeField] private PlayerInfoUI infoUI; // 【追加】UI参照
    [Header("Visuals")]
    [SerializeField] private Image playerImage; // 【追加】プレイヤーの立ち絵を表示する場所
    
    // ゲーム中に変動する「現在の所持デッキ」
    // private set なので、外部からは読み取り専用
    public List<CardData> MasterDeck {get; private set;} = new List<CardData>();
    public int CurrentEnergy { get; private set; } // 現在のエナジー
    public int MaxHP { get; private set; }    // 【追加】
    public int CurrentHP { get; private set; } // 【追加】
        public int CurrentBlock { get; private set; } // 【追加】
    private BattleManager battleManager;


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
            CurrentBlock = 0;
            Debug.Log($"エナジー初期化: {CurrentEnergy}");
            if (infoUI != null)
            {
                infoUI.SetupHP(MaxHP, CurrentHP);
                infoUI.UpdateEnergy(CurrentEnergy);
            }
            // 【追加】見た目の初期化（データを元に画像を表示）
            if (playerImage != null && baseData.PlayerSprite != null)
            {
                playerImage.sprite = baseData.PlayerSprite;
                playerImage.preserveAspect = true; // 縦横比を維持
                
                // もし画像が透明(alpha=0)だと見えないので、念のため色を白(不透明)にする
                playerImage.color = Color.white;
            }
        }
                battleManager = FindFirstObjectByType<BattleManager>();

    }
    // --- 【追加】ターン開始時の処理 ---
    public void OnTurnStart()
    {
        // エナジーを最大値まで回復
        if (baseData != null)
        {
            CurrentBlock = 0;

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
    public IEnumerator TakeDamage(int damage)
    {
  
        CurrentBlock -= damage;
        if (CurrentBlock < 0)
        {
            damage = -CurrentBlock;
            CurrentBlock = 0;
        }
        else
        {
            damage = 0;
        }
        if (damage < 0) damage = 0;
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

        yield return new WaitForSeconds(1.0f);
    }

    public IEnumerator GainBlock(int blockAmount)
    {
        CurrentBlock += blockAmount;
        Debug.Log($"プレイヤーにブロック付与: {blockAmount} (現在のブロック: {CurrentBlock})");
        yield return new WaitForSeconds(1.0f);
    }
        public void OnPointerClick(PointerEventData eventData)
    {
        if (battleManager != null)
        {
            battleManager.OnTargetClicked(this.gameObject);
        }
    }
}
