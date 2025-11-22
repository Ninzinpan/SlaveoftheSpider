using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

// 状態の定義
public enum BattleState
{
    PlayerTurn,      // プレイヤーがカードを選べる状態
    Targeting,       // カードを選んで、対象を選択中の状態（矢印が出ている）
    Busy,            // アニメーション中や処理中（操作不能）
    EnemyTurn        // 敵のターン
}

public class BattleManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private DeckManager deckManager;
    // [SerializeField] private TargetingUI targetingUI; // 後で作るターゲット矢印UI

    // 現在の状態（インスペクタで確認用）
    [Header("State")]
    [SerializeField] private BattleState currentState;

    // 現在選択中のカード（ターゲット待ちのカード）
    private CardDisplay selectedDisplay;

    private void Start()
    {
        // 戦闘開始時はプレイヤーのターン
        currentState = BattleState.PlayerTurn;
    }

    private void Update()
    {
        // ターゲット選択中のキャンセル処理
        // (右クリック または ESCキー でキャンセル)
        if (currentState == BattleState.Targeting)
        {
            if (Mouse.current.rightButton.wasPressedThisFrame)
            
            {
                Debug.Log("BattleManager: 右クリックが押されました。ターゲット選択がキャンセルされます.");
                CancelTargeting();
            }
        }
    }

    // --- 1. カードがクリックされた時の処理 ---
    public void OnCardClicked(CardDisplay cardDisplay)
    {
        // 安全装置: プレイヤーのターン以外は無視
        if (currentState != BattleState.PlayerTurn) return;

        // 安全装置: データが無い、またはコスト不足なら無視
        if (cardDisplay == null || cardDisplay.CardData == null) return;
        
        // エナジーが足りなければ、ここで門前払いする
        if (playerManager.CurrentEnergy < cardDisplay.CardData.Cost)
        {
            Debug.Log("エナジーが足りません！");
            return;
        }
        // 状態を「ターゲット選択中」に移行
        selectedDisplay = cardDisplay;
        currentState = BattleState.Targeting;
        
        Debug.Log($"ターゲット選択開始: {cardDisplay.CardData.CardName}");
        
        // TODO: targetingUI.Show(startPosition, mousePosition);
    }

    // --- 2. ターゲット（敵）がクリックされた時の処理 ---
    // ※EnemyManagerスクリプトからこのメソッドを呼んでもらう想定
    public void OnTargetClicked(GameObject targetEnemy)
    {
        // 安全装置: ターゲット選択中以外なら無視
        if (currentState != BattleState.Targeting) return;

        // ターゲット選択完了！シーケンスを開始
        StartCoroutine(CardPlaySequence(selectedDisplay.CardData, targetEnemy));
        
        // 選択状態をリセット
        CancelTargeting(); 
    }

    // --- 3. ターゲット選択のキャンセル ---
    public void CancelTargeting()
    {
        selectedDisplay = null;
        currentState = BattleState.PlayerTurn;
        
        Debug.Log("ターゲット選択キャンセル");
        // TODO: targetingUI.Hide();
    }

    // --- 4. カード発動シーケンス (コルーチン) ---
    // ここに「演出」と「処理」を順番に書いていく
    private IEnumerator CardPlaySequence(CardData cardData, GameObject target)
    {
        // 状態を「処理中」にして、入力をブロック
        BattleState previousState = currentState;
        currentState = BattleState.Busy;

        Debug.Log($"カード発動シーケンス開始: {cardData.CardName}");

        // --- A. コストを支払う ---
        playerManager.ConsumeEnergy(cardData.Cost);
        Debug.Log($"カードコスト支払い: {cardData.Cost} 残りエナジー{playerManager.CurrentEnergy}");

        // --- B. ターゲットマーカーを消す ---
        // targetingUI.Hide();

        // --- C. 演出 (拡張ポイント) ---
        // カードが光る、キャラが動くなど
        // yield return StartCoroutine(PlayAttackAnimation()); 
        Debug.Log("演出中...(1秒待機)");
        yield return new WaitForSeconds(1.0f); // 仮の待機

        // --- D. 効果の実処理 ---

        // ターゲットのオブジェクトから EnemyController を取得
        EnemyController enemy = target.GetComponent<EnemyController>();

        if (enemy != null)
        {
            // カードに設定されている全効果をチェック
            foreach (var effect in cardData.Effects)
            {
                // 効果の種類が「ダメージ」なら実行
                if (effect.effectType == EffectType.Damage)
                {
                    // ★ポイント: 
                    // enemy.TakeDamage はコルーチンなので、StartCoroutineで起動し、
                    // yield return でその処理(演出含む)が終わるのを待つ
                    yield return StartCoroutine(enemy.TakeDamage(effect.value));
                }
                else
                {
                    Debug.LogWarning($"未対応の効果タイプ: {effect.effectType}");
                }
            }
        }
        // target.GetComponent<Enemy>().TakeDamage(...);

        // --- E. カードを捨てる ---
        deckManager.DiscardSpecificCard(selectedDisplay);
        // 参照を外しておく（安全のため）
        selectedDisplay = null;

        // --- F. 処理完了 ---
        // 勝利していなければプレイヤーターンに戻す
        currentState = BattleState.PlayerTurn;
    }
}