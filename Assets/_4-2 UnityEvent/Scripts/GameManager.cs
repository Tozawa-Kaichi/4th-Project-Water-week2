using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// ゲームを管理する。適当なオブジェクトにアタッチし、各種設定をすれば動作する。
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>Windows のマウスカーソルをゲーム中に消すかどうかの設定</summary>
    [SerializeField] bool _hideSystemMouseCursor = false;
    /// <summary>敵オブジェクトがいるレイヤー</summary>
    [SerializeField] LayerMask _enemyLayer;
    /// <summary>照準の Image (UI)</summary>
    [SerializeField] Image _crosshairImage = null;
    /// <summary>照準に敵が入っていない時の色</summary>
    [SerializeField] Color _colorNormal = Color.white;
    /// <summary>照準に敵が入っている時の色</summary>
    [SerializeField] Color _colorFocus = Color.red;
    /// <summary>銃のオブジェクト</summary>
    [SerializeField] GameObject _gunObject = null;
    /// <summary>銃の操作のために Ray を飛ばす距離</summary>
    [SerializeField] float _rangeDistance = 100f;
    /// <summary>スコアを表示するための Text (UI)</summary>
    [SerializeField] Text _scoreText = null;
    /// <summary>ライフの初期値</summary>
    [SerializeField] int _initialLife = 500;
    /// <summary>ライフを表示するための Text (UI)</summary>
    [SerializeField] Text _lifeText = null;
    /// <summary>弾を撃った時に呼び出す処理</summary>
    [SerializeField] UnityEngine.Events.UnityEvent _onShoot = null;
    /// <summary>ゲームスタート時に呼び出す処理</summary>
    [SerializeField] UnityEngine.Events.UnityEvent _onGameStart = null;
    /// <summary>ゲームオーバー時に呼び出す処理</summary>
    [SerializeField] UnityEngine.Events.UnityEvent _onGameOver = null;
    /// <summary>ライフの現在値</summary>
    int _life;
    /// <summary>ゲームのスコア</summary>
    int _score = 0;
    /// <summary>全ての敵オブジェクトを入れておくための List</summary>
    List<GunEnemyController> _enemies = null;
    /// <summary>現在照準で狙われている敵</summary>
    GunEnemyController _currentTargetEnemy = null;
    /// <summary>ライフを表示するための GameObject</summary>
    GameObject _lifeObject;

    /// <summary>
    /// ゲームを初期化する
    /// </summary>
    void Start()
    {
        _onGameStart.Invoke();
        _life = _initialLife;
        _enemies = GameObject.FindObjectsOfType<GunEnemyController>().ToList();
        _lifeObject = GameObject.Find("LifeText");
        _lifeText = _lifeObject.GetComponent<Text>();
        _lifeText.text = string.Format("{0:000}", _life);

        if (_hideSystemMouseCursor)
        {
            Cursor.visible = false;
        }
    }

    /// <summary>
    /// ゲームをやり直す
    /// </summary>
    public void Restart()
    {
        Debug.Log("Restart");
        _enemies.ForEach(enemy => enemy.gameObject.SetActive(true));
        Start();
    }

    /// <summary>
    /// ゲームオーバー
    /// </summary>
    void Gameover()
    {
        Debug.Log("Gameover");
        _enemies.ForEach(enemy => enemy.gameObject.SetActive(false));
        _onGameOver.Invoke();
    }

    void Update()
    {
        // 照準を処理する
        _crosshairImage.rectTransform.position = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, _rangeDistance))
        {
            _gunObject.transform.LookAt(hit.point);    // 銃の方向を変えている
        }

        // 敵が照準に入っているか調べる
        bool isEnemyTargeted = Physics.Raycast(ray, out hit, _rangeDistance, _enemyLayer);
        _crosshairImage.color = isEnemyTargeted ? _colorFocus : _colorNormal;    // 三項演算子 ? を使っている
        _currentTargetEnemy = isEnemyTargeted ? hit.collider.gameObject.GetComponent<GunEnemyController>() : null;    // 三項演算子 ? を使っている

        // 左クリック入力時の処理
        if (Input.GetButtonDown("Fire1"))
        {
            _onShoot.Invoke();

            // 敵に当たったら得点を足して表示を更新する
            if (_currentTargetEnemy)
            {
                _currentTargetEnemy.Hit();
            }
        }
    }

    private void OnApplicationQuit()
    {
        Cursor.visible = true;
    }

    /// <summary>
    /// 攻撃を食らった時に呼ぶ
    /// </summary>
    public void Hit()
    {
        // ライフを減らして表示を更新する。
        _life -= 1;
        Debug.Log($"Hit by enemy. Life: {_life}");
        _lifeText.text = string.Format("{0:000}", _life);
        
        if (_life < 1)
        {
            Gameover();
        }
    }
}
