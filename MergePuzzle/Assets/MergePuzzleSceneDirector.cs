using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MergePuzzleSceneDirector : MonoBehaviour
{
	//アイテムのプレハブ
	[SerializeField] List<BubbleController> prefabBubbles;

	// UI
	[SerializeField] TextMeshProUGUI textScore;
	[SerializeField] Text textScoreResult;
	[SerializeField] GameObject panelResult;
	// Audio
	[SerializeField] AudioClip seDrop;
	[SerializeField] AudioClip seMerge;

	// スコア
	int score;
	// 現在のアイテム
	BubbleController currentBubble;
	// 生成位置
	const float SpawnItemY = 3.5f;
	// Audio再生位置
	AudioSource audioSource;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		// サウンド再生用
		audioSource = GetComponent<AudioSource>();

		// リザルト画面を非表示
		panelResult.SetActive(false);

		//　最初のアイテムを生成
		StartCoroutine(SpawnCurrentItem());
	}

	// Update is called once per frame
	void Update()
	{
		// アイテムがなければここから下の処理はしない
		if (!currentBubble) return;

		// ゲームオーバー関数の実行(デバッグ用)
		// GAMEOVER();

		// マウスポジション(スクリーン座標)からワールド座標に変換
		Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		// バブルの幅を取得
		float bubbleWidth = currentBubble.GetComponent<SpriteRenderer>().bounds.size.x;

		// 壁の範囲を計算（バブルの幅の半分を考慮）
		float leftLimit = -2.86f + bubbleWidth / 2;
		float rightLimit = 2.86f - bubbleWidth / 2;

		// x座標をマウスに合わせる（壁の範囲内に制限）
		worldPoint.x = Mathf.Clamp(worldPoint.x, leftLimit, rightLimit);

		Vector2 bubblePosition = new Vector2(worldPoint.x, SpawnItemY);
		currentBubble.transform.position = bubblePosition;

		// タッチ処理
		if (Input.GetMouseButtonUp(0))
		{
			// 重力をセットしてドロップ
			currentBubble.GetComponent<Rigidbody2D>().gravityScale = 1;
			// 主事アイテムリセット
			currentBubble = null;
			// 次のアイテム
			StartCoroutine(SpawnCurrentItem());
			// SE再生
			audioSource.PlayOneShot(seDrop);
		}
	}

	// アイテム生成
	BubbleController SpawnItem(Vector2 position, int colorType = -1)
	{
		// 色ランダム
		int index = Random.Range(0, prefabBubbles.Count / 2);

		// 色の指定があれば上書き
		if (0 < colorType)
		{
			index = colorType;
		}

		// 生成
		BubbleController bubble =
			Instantiate(prefabBubbles[index], position, Quaternion.identity);

		// 必須データセット
		bubble.SceneDirector = this;
		bubble.ColorType = index;

		return bubble;
	}

	// 所持アイテム生成
	IEnumerator SpawnCurrentItem()
	{
		// 指定された秒数を待つ
		yield return new WaitForSeconds(1.0f);
		// 生成されたアイテムを保持する
		currentBubble = SpawnItem(new Vector2(0, SpawnItemY));
		// 落ちないように重力を0にする
		currentBubble.GetComponent<Rigidbody2D>().gravityScale = 0;
	}

	// アイテムを合体させる
	public void Merge(BubbleController bubbleA, BubbleController bubbleB)
	{

		//　操作中のアイテムとぶつかったらゲームオーバー
		if (currentBubble == bubbleA || currentBubble == bubbleB)
		{
			// ゲームオーバー関数の実行
			GAMEOVER();
			return;
		}


		// マージ済みの場合は関数を終了
		if (bubbleA.IsMerged || bubbleB.IsMerged) return;

		// 違う色の場合は関数を終了
		if (bubbleA.ColorType != bubbleB.ColorType) return;

		// 次に生成する色が用意してあるリストの最大数を超える場合
		int nextColor = bubbleA.ColorType + 1;
		if (!(prefabBubbles.Count - 1 < nextColor))
		{
			// 2点間の中心の位置を取得
			Vector2 lerpPosition =
				Vector2.Lerp(bubbleA.transform.position, bubbleB.transform.position, 0.5f);

			// 新しいアイテムを生成
			BubbleController newBubble = SpawnItem(lerpPosition, nextColor);

			// 点数計算
			score += newBubble.ColorType * 10;
		}
		else
		{
			score += bubbleA.ColorType * 20;
		}

		// マージ済みフラグON
		bubbleA.IsMerged = true;
		bubbleB.IsMerged = true;

		// シーンからバブルを削除
		Destroy(bubbleA.gameObject);
		Destroy(bubbleB.gameObject);

		// 点数の表示更新
		textScore.text = score.ToString();
		textScoreResult.text = score.ToString() + " 点";

		// SE再生
		audioSource.PlayOneShot(seMerge);
	}

	// リトライボタン
	public void OnClickRetry()
	{
		SceneManager.LoadScene("MergePuzzleScene");
	}

	public void TitleButton()
	{
		// Load the title scene
		SceneManager.LoadScene("GameStartScene");
	}

	public void GAMEOVER()
	{
		// このUpdateに入らないようにする
		enabled = false;
		// リザルトパネル表示
		panelResult.SetActive(true);
		return;
	}
}
