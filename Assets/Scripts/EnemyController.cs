using UnityEngine;
using UnityEngine.UI; // Imageを扱うために必要
using UnityEngine.EventSystems;
using System.Collections;

public class EnemyController : MonoBehaviour, IPointerClickHandler
{
    [Header("Data Source")]
    [SerializeField] private EnemyData enemyData; // インスペクタでセットする敵データ (.asset)

    [Header("UI References")]
    [SerializeField] private Image enemyImage;    // 見た目を変えるためのImageコンポーネント
    [SerializeField] private Slider hpSlider; // 【追加】HPバー

    // --- ゲーム中に変動するパラメータ (Runtime State) ---
    private int currentHP;
    private int maxHP;
// 【追加】現在の行動が、リストの何番目か
    private int actionIndex = 0;
    private BattleManager battleManager;
    private PlayerManager playerManager; // 攻撃対象として必要

    private void Start()
    {
        battleManager = FindFirstObjectByType<BattleManager>();
        playerManager = FindFirstObjectByType<PlayerManager>(); // プレイヤーを探しておく
        
        
        // ゲーム開始時にデータを読み込んで初期化
        SetupEnemy();
    }

    /// <summary>
    /// データからステータスと見た目を反映させる
    /// </summary>
    private void SetupEnemy()
    {
        if (enemyData == null)
        {
            Debug.LogError("EnemyController: EnemyDataがセットされていません");
            return;
        }

        // 1. ステータスの初期化
        maxHP = enemyData.MaxHP;
        currentHP = maxHP;
        actionIndex = 0; // 最初は0番目から

        // 2. 見た目の初期化
        // Imageコンポーネントがあり、かつデータに画像が設定されていれば反映
        if (enemyImage != null && enemyData.Sprite != null)
        {
            enemyImage.sprite = enemyData.Sprite;
            Debug.Log("EnemyController: 敵の画像を設定しました.");
            
            // 画像の縦横比を維持してサイズ調整
            enemyImage.preserveAspect = true; 
        }
        // 【追加】HPバーの初期化
            if (hpSlider != null)
            {
                hpSlider.maxValue = maxHP;
                hpSlider.value = currentHP;
            }

        Debug.Log($"敵が出現: {enemyData.EnemyName} (HP: {currentHP})");
        ShowNextIntent();
    }
    public void ShowNextIntent()
    {
        if (enemyData.ActionPattern.Count == 0) return;

        var nextAction = enemyData.ActionPattern[actionIndex];
        Debug.Log($"敵の次の行動: {nextAction.actionName} (Index: {actionIndex})");
        
        // TODO: ここで頭上にアイコンを出す (targetingUI.ShowIntent(nextAction.intentIcon))
    }

    // --- 【追加】ターン行動の実行 (コルーチン) ---
    // BattleManagerから呼ばれる
    public IEnumerator PerformTurn()
    {
        // 安全装置: 行動パターンがない敵は何もしない
        if (enemyData.ActionPattern.Count == 0) yield break;

        // 1. 現在の行動データを取得
        EnemyAction action = enemyData.ActionPattern[actionIndex];
        Debug.Log($"敵の行動開始: {action.actionName}");

        // 2. 攻撃アニメーションなどの演出（仮）
        yield return new WaitForSeconds(0.5f);

        // 3. 効果リストを順に実行
        foreach (var effect in action.effects)
        {
            // プレイヤーに攻撃
            if (effect.effectType == EffectType.Damage)
            {
                // まだPlayerManagerにTakeDamageがないので、一旦ログだけ
                Debug.Log($"プレイヤーに {effect.value} の攻撃！");
                
                // 次のステップで以下のコメントアウトを外します
                if (playerManager != null)
                {
                    playerManager.TakeDamage(effect.value); 

                }
            }
            else if (effect.effectType == EffectType.Block)
            {
                Debug.Log($"敵は {effect.value} のブロックを得た！");
                // GainBlock(effect.value);
            }
        }

        // 4. 演出待ち
        yield return new WaitForSeconds(0.5f);

        // 5. 次の行動へインデックスを進める (ループ処理)
        // (0 -> 1 -> 2 -> 0 -> 1...)
        actionIndex++;
        if (actionIndex >= enemyData.ActionPattern.Count)
        {
            actionIndex = 0;
        }
        
        // 次のターンの予告をしておく
        ShowNextIntent();
    }

    // クリック検知 (変更なし)
    public void OnPointerClick(PointerEventData eventData)
    {
        if (battleManager != null)
        {
            battleManager.OnTargetClicked(this.gameObject);
        }
    }

    // ダメージを受ける処理
    public IEnumerator TakeDamage(int damage)
    {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;
        // 【追加】HPバーの更新
        if (hpSlider != null)
        {
            hpSlider.value = currentHP;
        }

        Debug.Log($"{enemyData.EnemyName} に {damage} ダメージ！ 残りHP: {currentHP}/{maxHP}");

        // 【ポイント】ダメージ演出（点滅や振動など）
        // ここで時間を稼ぐと、BattleManagerもその分待ってくれる
        yield return new WaitForSeconds(0.5f); 

        // 死亡判定
        if (currentHP <= 0)
        {
            // 死亡処理を実行し、それが終わるのをさらに待つ
            yield return StartCoroutine(Die());
        }
    }

    // --- 【変更】こちらも IEnumerator に変更 ---
    private IEnumerator Die()
    {
        Debug.Log($"{enemyData.EnemyName} は倒れた...");

        // 【ポイント】死亡演出（フェードアウトや爆発エフェクト）
        // この時間が終わるまで、BattleManagerは次の処理に行かない
        yield return new WaitForSeconds(1.0f);

        gameObject.SetActive(false);
    }


}