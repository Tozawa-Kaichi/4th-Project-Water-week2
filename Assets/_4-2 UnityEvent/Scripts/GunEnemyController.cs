using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 敵を制御するコンポーネント。敵は独立して動いているため、プレハブを増やせば敵を増やすことができる。
/// ただし、_onShoot だけはプレハブを置いた後に設定しなければならないため、動的に敵を生成する場合は _onShoot を設定する処理をすること。
/// </summary>
public class GunEnemyController : MonoBehaviour
{
    /// <summary>Idle 状態から Ready 状態に移る時間の最小値（単位: 秒）</summary>
    [SerializeField] float _minInterval = 1f;
    /// <summary>Idle 状態から Ready 状態に移る時間の最大値（単位: 秒）</summary>
    [SerializeField] float _maxInterval = 5f;
    /// <summary>Ready 状態から攻撃までの時間の最小値（単位: 秒）</summary>
    [SerializeField] float _minFireTime = 0.1f;
    /// <summary>Ready 状態から攻撃までの時間の最大値（単位: 秒）</summary>
    [SerializeField] float _maxFireTime = 0.3f;
    /// <summary>キャラクターアニメーションのための Animator</summary>
    [SerializeField] Animator _animator = null;
    /// <summary>攻撃が当たった時に加算される点</summary>
    [SerializeField] int _score = 100;
    /// <summary>キャラクターの状態</summary>
    GunEnemyStatus _status = GunEnemyStatus.Idle;
    /// <summary>タイマー</summary>
    float _timer;
    /// <summary>Idle 状態から Ready 状態に移るまでの時間（単位: 秒）</summary>
    float _interval;
    /// <summary>Ready 状態から攻撃までの時間（単位: 秒）</summary>
    float _fireTime;
    /// <summary>攻撃判定のためのコライダー</summary>
    Collider _collider = null;  // Collider は BoxCollider, SphereCollider などの基底クラスである
    /// <summary>敵が銃を撃った時に実行される処理</summary>
    [SerializeField] UnityEngine.Events.UnityEvent _onShoot = null;
    /// <summary>ゲームの総得点</summary>
    int _totalScore = 0;
    /// <summary>スコアを表示するための Text(UI)</summary>
    [SerializeField] Text _scoreText = null;

    /// <summary>
    /// 初期化処理
    /// </summary>
    void OnEnable()
    {
        this.transform.LookAt(Camera.main.transform.position);
        _collider = GetComponent<Collider>();
        _collider.enabled = false;
        _status = GunEnemyStatus.Idle;
        ResetTimer();
        _scoreText.text = string.Format("{0:0000000000}", _totalScore);
    }

    void Update()
    {
        _timer += Time.deltaTime;

        switch (_status)
        {
            case GunEnemyStatus.Idle:
                // 時間が経ったら Idle -> Ready に移行する
                if (_timer > _interval)
                {
                    ResetTimer();
                    _status = GunEnemyStatus.Ready;
                    _animator.SetTrigger("Ready");
                    _collider.enabled = true;
                }
                break;
            case GunEnemyStatus.Ready:
                // 時間が経ったら発砲し、Idle 状態に移行する
                if (_timer > _fireTime)
                {
                    ResetTimer();
                    _status = GunEnemyStatus.Idle;
                    _animator.SetTrigger("Fire");
                    _collider.enabled = false;
                    
                    if (_onShoot.GetPersistentEventCount() > 0)
                    {
                        _onShoot.Invoke();
                    }
                    else
                    {
                        Debug.LogWarning("発砲時に呼ばれる処理が指定されていません。");
                    }
                }
                break;
        }
    }

    /// <summary>
    /// タイマーをリセットする
    /// </summary>
    void ResetTimer()
    {
        _timer = 0;
        _fireTime = Random.Range(_minFireTime, _maxFireTime);    // Idle -> Ready までの時間をランダムに決める
        _interval = Random.Range(_minInterval, _maxInterval);    // Ready -> 攻撃までの時間をランダムに決める
    }

    /// <summary>
    /// プレイヤーの攻撃が当たった時に呼ぶ
    /// Idle 状態（初期状態）に戻す
    /// </summary>
    /// <returns></returns>
    public int Hit()
    {
        ResetTimer();
        _status = GunEnemyStatus.Idle;
        _animator.SetTrigger("Hit");
        _collider.enabled = false;
        _totalScore += _score;
        Debug.Log($"{name} got hit. Score: {_totalScore}");
        _scoreText.text = string.Format("{0:0000000000}", _totalScore);
        return _score;
    }
}

/// <summary>
/// 敵の状態を表す
/// </summary>
enum GunEnemyStatus
{
    /// <summary>壁の裏に隠れて待機している状態。こちらの攻撃は当たらない
    /// </summary>
    Idle,
    /// <summary>銃を構えてこちらを狙っている状態。この時にこちらの攻撃が当たる。</summary>
    Ready,
}
