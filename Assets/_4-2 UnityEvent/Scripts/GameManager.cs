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
    public int _score = 0;
    /// <summary>全ての敵オブジェクトを入れておくための List</summary>
    List<GunEnemyController> _enemies = null;
    /// <summary>現在照準で狙われている敵</summary>
    GunEnemyController _currentTargetEnemy = null;
    /// <summary>ライフを表示するための GameObject</summary>
    GameObject _lifeObject;
    int _fscore;
    [SerializeField]int _lifeborder=5000;
    bool started = false;
    [SerializeField] UnityEngine.Events.UnityEvent _ontrueStart;
    [SerializeField] UnityEngine.Events.UnityEvent _upsound;
    [SerializeField] UnityEngine.Events.UnityEvent _ee;
    /// <summary>
    ///
    /// アプリケーションを初期化する
    /// </summary>
    void Start()
    {
        started = false;
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
    /// ゲーム初期化する
    /// </summary>
    /// 


    /// <summary>
    /// ゲームをやり直す
    /// </summary>
    public void Restart()
    {
        Debug.Log("Restart");
        _score = 0;
        _enemies.ForEach(enemy => enemy.gameObject.SetActive(true));
        Addscore(0);
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
        _ee.Invoke();
        
    }

    void Update()
    {
        if(Input.GetButtonDown("Fire1"))
        {
            started = true;
            _ontrueStart.Invoke();
        }
        if (started)
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
                    Addscore(_currentTargetEnemy.Hit());
                }
            }
        }
    }
    public void Addscore(int _s)
    {
        _score += _s;
        _fscore += _s;
        _scoreText.text = _score.ToString("D8");
        if (_fscore>=_lifeborder)
        {
            _lifeborder+=_lifeborder;
            _life += 1;
            _upsound.Invoke();
            Debug.Log($"Hit by enemy. Life: {_life}");
            _lifeText.text = string.Format("{0:000}", _life);
            
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
