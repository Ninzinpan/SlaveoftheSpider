using UnityEngine;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform handArea;

    [Header("Game State (View Only)")]
    // インスペクタで確認できるように SerializeField をつけていますが、外部からは触らせません
    [SerializeField] private List<CardData> drawPile = new List<CardData>();   // 山札
    [SerializeField] private List<CardDisplay> currentHand = new List<CardDisplay>(); // 手札 (表示オブジェクトを管理)
    [SerializeField] private List<CardData> discardPile = new List<CardData>(); // 捨て札

    private void Start()
    {
        // ゲーム開始時のセットアップフロー
        InitializeDeck();
        ShuffleDeck();
        DrawCard(5); // とりあえず5枚引いてみる
    }

    /// <summary>
    /// 1. デッキの初期化: プレイヤーデータからカードリストをコピーする
    /// </summary>
    private void InitializeDeck()
    {
        if (playerManager == null)
        {
            Debug.LogError("DeckManager: PlayerBaseDataがセットされていません！");
            return;
        }

        // ScriptableObjectのリストをそのまま使うと元のデータが書き換わる恐れがあるため、
        // 新しいリストとしてコピーを作成します (ディープコピーではないが、リストの構造は別になる)
        drawPile = new List<CardData>(playerManager.MasterDeck);
        
        discardPile.Clear();
        currentHand.Clear();
    }

    /// <summary>
    /// 2. シャッフル: 山札の順番をランダムにする
    /// </summary>
    public void ShuffleDeck()
    {
        // フィッシャー–イェーツのシャッフルアルゴリズム
        for (int i = 0; i < drawPile.Count; i++)
        {
            CardData temp = drawPile[i];
            int randomIndex = Random.Range(i, drawPile.Count);
            drawPile[i] = drawPile[randomIndex];
            drawPile[randomIndex] = temp;
        }
        Debug.Log("デッキをシャッフルしました");
    }

    /// <summary>
    /// 3. ドロー: 山札からカードを引き、画面に生成する
    /// </summary>
    public void DrawCard(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // 山札が空の場合の処理 (捨て札を戻してシャッフル)
            if (drawPile.Count == 0)
            {
                if (discardPile.Count > 0)
                {
                    ReshuffleDiscardPile();
                }
                else
                {
                    Debug.Log("引けるカードがありません！");
                    break;
                }
            }

            // --- データ上の移動 ---
            CardData drawnData = drawPile[0]; // 先頭（0番目）を引く
            drawPile.RemoveAt(0);

            // --- 画面上の生成 ---
            GameObject cardObj = Instantiate(cardPrefab, handArea);
            CardDisplay display = cardObj.GetComponent<CardDisplay>();
            
            if (display != null)
            {
                display.Setup(drawnData);
                currentHand.Add(display); // 手札リストに追加（後で捨てるときに使う）
            }
        }
    }

    /// <summary>
    /// 捨て札を山札に戻す処理
    /// </summary>
    private void ReshuffleDiscardPile()
    {
        Debug.Log("捨て札を山札に戻してシャッフルします");
        
        // 捨て札を全部山札に移す
        drawPile.AddRange(discardPile);
        discardPile.Clear();

        // シャッフル
        ShuffleDeck();
    }

    // --- デバッグ用機能 (ボタンなどで呼べるようにpublicにしておく) ---
    
    // 手札をすべて捨てる（ターン終了時の処理などで使う）
    public void DiscardHand()
    {
        foreach (var cardDisplay in currentHand)
        {
            // データだけ捨て札リストに移す
            discardPile.Add(cardDisplay.CardData);
            
            // 画面上のオブジェクトは破壊する
            Destroy(cardDisplay.gameObject);
        }
        currentHand.Clear();
    }

    public void DiscardSpecificCard(CardDisplay cardDisplay)
    {
        if (cardDisplay == null) return;

        // 1. データだけ捨て札リストに移す
        discardPile.Add(cardDisplay.CardData);

        // 2. 手札リスト（管理用）から除外する
        if (currentHand.Contains(cardDisplay))
        {
            currentHand.Remove(cardDisplay);
        }

        // 3. 画面上のオブジェクトを破壊する
        Destroy(cardDisplay.gameObject);

        Debug.Log("カードを捨て札に送りました");
    }
    // ... (既存のコードの続き)

    // --- ボタンから呼び出すためのメソッド ---

    // 「1枚引く」ボタン用
    public void OnDrawButtonClicked()
    {
        DrawCard(1);
    }

    // 「全て捨てる」ボタン用
    public void OnDiscardButtonClicked()
    {
        DiscardHand();
    }

}