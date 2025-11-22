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

    // --- ゲーム中に変動するパラメータ (Runtime State) ---
    private int currentHP;
    private int maxHP;

    private BattleManager battleManager;

    private void Start()
    {
        battleManager = FindFirstObjectByType<BattleManager>();
        
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

        // 2. 見た目の初期化
        // Imageコンポーネントがあり、かつデータに画像が設定されていれば反映
        if (enemyImage != null && enemyData.Sprite != null)
        {
            enemyImage.sprite = enemyData.Sprite;
            Debug.Log("EnemyController: 敵の画像を設定しました.");
            
            // 画像の縦横比を維持してサイズ調整
            enemyImage.preserveAspect = true; 
        }

        Debug.Log($"敵が出現: {enemyData.EnemyName} (HP: {currentHP})");
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