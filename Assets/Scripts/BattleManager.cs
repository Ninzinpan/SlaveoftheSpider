using UnityEngine;
using System.Collections;
using System.Collections.Generic; // ← 【重要】これを追加してください！
using UnityEngine.InputSystem;
using UnityEngine.UI; // Buttonを操作するために必要

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
    [SerializeField] private TargetingUI targetingUI; // 後で作るターゲット矢印UI
    [SerializeField] private Button endTurnButton; // 【追加】ターン終了ボタン

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
        if (targetingUI != null) targetingUI.Show();
        
        // TODO: targetingUI.Show(startPosition, mousePosition);
    }

    // --- 2. ターゲット（敵）がクリックされた時の処理 ---
    // ※EnemyManagerスクリプトからこのメソッドを呼んでもらう想定
// --- 2. ターゲット（敵や自分）がクリックされた時の処理 ---
    public void OnTargetClicked(GameObject targetObj)
    {
        // 安全装置
        if (currentState != BattleState.Targeting) return;

        // 判定用のコンポーネント取得
        EnemyController enemy = targetObj.GetComponent<EnemyController>();
        PlayerManager player = targetObj.GetComponent<PlayerManager>();
        TargetType targetType = selectedDisplay.CardData.Target;

        bool isValid = false;

        // ターゲットタイプ別の判定ロジック
        switch (targetType)
        {
            case TargetType.Enemy:
            case TargetType.AllEnemies: // 全体攻撃も、とりあえず敵をクリックすればOKとする
                if (enemy != null) isValid = true;
                break;

            case TargetType.Self:
                if (player != null) isValid = true;
                break;
        }

        // 不正なターゲットならキャンセル
        if (!isValid)
        {
            Debug.LogWarning($"ターゲット不一致: {targetType} に対して {targetObj.name} を選択しました");
            CancelTargeting();
            return;
        }

        // 適合したので発動シーケンスへ
        StartCoroutine(CardPlaySequence(selectedDisplay, targetObj));
        CancelTargeting();
    }
    // --- 3. ターゲット選択のキャンセル ---
    public void CancelTargeting()
    {
        selectedDisplay = null;
        currentState = BattleState.PlayerTurn;
        
        Debug.Log("ターゲット選択キャンセル");
if (targetingUI != null) targetingUI.Hide();
    }


    // --- 4. カード発動シーケンス (コルーチン) ---
    // ここに「演出」と「処理」を順番に書いていく
   // --- 4. カード発動シーケンス (コルーチン) ---
    private IEnumerator CardPlaySequence(CardDisplay display, GameObject targetObj)
    {
        currentState = BattleState.Busy;
        CardData cardData = display.CardData;

        Debug.Log($"カード発動: {cardData.CardName} (Target: {cardData.Target})");

        // --- A. コスト支払い ---
        playerManager.ConsumeEnergy(cardData.Cost);

        // --- B. ターゲットUI消去 (省略) ---

        // --- C. 演出 ---
        yield return new WaitForSeconds(0.5f); // 演出待ち

        // --- D. 効果の実処理 (ターゲット振り分け) ---

        // 1. 「本当の効果対象」のリストを作る
        List<object> finalTargets = new List<object>(); // EnemyController または PlayerManager が入る

        switch (cardData.Target)
        {
            case TargetType.Enemy:
                // クリックした敵単体
                finalTargets.Add(targetObj.GetComponent<EnemyController>());
                break;

            case TargetType.AllEnemies:
                // シーン上の全ての敵
                var allEnemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
                foreach (var enemy in allEnemies)
                {
                    // 死んでいる敵は除外しても良い
                    if (enemy.gameObject.activeSelf) finalTargets.Add(enemy);
                }
                break;

            case TargetType.Self:
                // クリックしたプレイヤー自身
                finalTargets.Add(targetObj.GetComponent<PlayerManager>());
                break;
        }

        // 2. 対象リスト全員に効果を適用
        foreach (var effect in cardData.Effects)
        {
            foreach (var target in finalTargets)
            {
                // 対象が「敵」の場合
                if (target is EnemyController enemy)
                {
                    switch (effect.effectType)
                    {
                        case EffectType.Damage:
                            // 敵へのダメージはコルーチン(演出待ちあり)
                            yield return StartCoroutine(enemy.TakeDamage(effect.value));
                            break;
                        case EffectType.Block:
                            // 敵がブロックを得る場合(実装していれば)
                            Debug.Log($"{enemy.name} にブロック {effect.value}");
                            break;
                    }
                }
                // 対象が「プレイヤー」の場合
                else if (target is PlayerManager player)
                {
                    switch (effect.effectType)
                    {
                        case EffectType.Damage:
                            // 自傷ダメージ 
                            StartCoroutine(player.TakeDamage(effect.value));
                            break;
                        case EffectType.Block:
                            // ブロック獲得
                            StartCoroutine(player.GainBlock(effect.value));
                            break;
                        case EffectType.Draw:
                            // ドロー効果
                            deckManager.DrawCard(effect.value);
                            break;
                    }
                }
            }
        }

        // --- E. カード破棄 ---
        deckManager.DiscardSpecificCard(display);
        selectedDisplay = null;

        // --- F. 完了 ---
        currentState = BattleState.PlayerTurn;
    }
    public void OnEndTurnButton()
    {
        // プレイヤーのターンでなければ押せない
        if (currentState != BattleState.PlayerTurn) return;

        // 敵ターンへの遷移を開始
        StartCoroutine(EnemyTurnSequence());
    }
    private IEnumerator EnemyTurnSequence()
    {
        // 1. 状態変更 & ボタン無効化
        currentState = BattleState.EnemyTurn;
        if (endTurnButton != null) endTurnButton.interactable = false;

        Debug.Log("--- 敵ターン開始 ---");

        // 2. 手札を捨てる
        deckManager.DiscardHand();

        // ターゲット選択中だったら解除しておく
        if (selectedDisplay != null) CancelTargeting();

        // 3. 全ての敵に行動させる
        // (FindObjectsByType でシーン上の敵を全部見つける)
        var enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);

        foreach (var enemy in enemies)
        {
            // 生きている敵だけ行動
            if (enemy.gameObject.activeSelf)
            {
                // 敵の行動コルーチンを呼び、終わるのを待つ (Wait)
                yield return StartCoroutine(enemy.PerformTurn());
            }
        }

        Debug.Log("--- 敵ターン終了 ---");

        // 4. プレイヤーのターンを開始する
        StartPlayerTurn();
    }

    // --- 【追加】プレイヤーターンの開始処理 ---
    private void StartPlayerTurn()
    {
        currentState = BattleState.PlayerTurn;
        
        // UI操作を許可
        if (endTurnButton != null) endTurnButton.interactable = true;

        // エナジー回復
        playerManager.OnTurnStart();

        // カードを引く (例えば5枚)
        deckManager.DrawCard(5);
        
        // 敵の次の行動(Intent)を更新してもらう
        // (EnemyController側で管理しているなら不要だが、念のため再取得指示などがここに入りうる)
        
        Debug.Log("--- プレイヤーターン開始 ---");
    }
}