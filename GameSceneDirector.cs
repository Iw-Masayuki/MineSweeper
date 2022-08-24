using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSceneDirector : MonoBehaviour
{
    const int CELL_X = 18;
    const int CELL_Y = 13;
    int mineCount = 30;

    public GameObject prefabCell;
    public GameObject prefabFlag;
    public GameObject result;

    public GameObject prefabOpenEx;
    public GameObject prefabMineEx;

    public Text txtTimer;
    public Text txtFlag;

    Text txtResult;

    CellController[,] cells;

    int flagCount;
    float timer;

    // Start is called before the first frame update
    void Start()
    {
        //子オブジェクト取得
        GameObject child = result.transform.Find("TextResult").gameObject;
        txtResult = child.GetComponent<Text>();

        //セルの設定
        cells = new CellController[CELL_X, CELL_Y];

        for (int i = 0; i < CELL_X; i++)
        {
            for (int j = 0; j < CELL_Y; j++)
            {
                float x = i - (CELL_X / 2);
                float y = j - (CELL_Y / 2);

                Vector3 pos = new Vector3(x, 0, y);
                GameObject obj = Instantiate(prefabCell, pos, Quaternion.identity);

                cells[i, j] = obj.GetComponent<CellController>();
                cells[i, j].Pos = new Vector2Int(i, j);
            }
        }

        //爆弾の設置
        int cnt = mineCount;
        while (0 < cnt)
        {
            int x = Random.Range(0, cells.GetLength(0));
            int y = Random.Range(0, cells.GetLength(1));

            //爆弾がセットされていなければ
            if (!cells[x, y].IsMine)
            {
                cells[x, y].IsMine = true;

                //周りのセルのカウントを入力
                foreach (var v in GetRoundCell(cells[x, y]))
                {
                    v.MineCount++;
                }
            }

            cnt--;

        }

        flagCount = mineCount;
        txtFlag.text = "" + flagCount;

    }

    // Update is called once per frame
    void Update()
    {
        //ゲームセット後は処理しない
        if (result.activeSelf) return;

        //ゲームクリア判定
        if (isClear())
        {
            txtResult.text = "Game Clear!!";
            result.SetActive(true);
            return;
        }

        //タイマー
        timer += Time.deltaTime;
        if (999 < timer) timer = 999;
        txtTimer.text = "" + (int)timer;

        //選択
        CellController cell = null;
        bool flag = false;

        //クリック処理
        if (Input.GetMouseButtonDown(0)
            || Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                //配列番号に変換
                GameObject obj = hit.collider.gameObject;
                int x = (int)obj.transform.position.x + (CELL_X / 2);
                int y = (int)obj.transform.position.z + (CELL_Y / 2);

                cell = cells[x, y];

                //フラグ
                if (Input.GetMouseButton(1))
                {
                    flag = true;
                }

            }

        }

        //フラグセット
        if (flag)
        {
            //既にフラグが立っていたら削除
            if (null != cell.objFlag)
            {
                Destroy(cell.objFlag);
                flagCount++;
                txtFlag.text = "" + flagCount;
                return;
            }

            //新しいフラグの設置
            if (1 > flagCount) return;

            Vector3 pos = cell.gameObject.transform.position;
            pos.y += 1.0f;

            cell.objFlag = Instantiate(prefabFlag, pos, Quaternion.identity);
            flagCount--;
            txtFlag.text = "" + flagCount;
        }

        //セルをオープン
        else if (null != cell)
        {
            //オープン済
            if (!cell.Open()) return;

            //地雷
            if (cell.IsMine)
            {
                foreach (var v in cells)
                {
                    Vector3 force = new Vector3(
                        Random.Range(-1000, 1000),
                        Random.Range(-1000, 1000),
                        Random.Range(-1000, 1000));

                    v.gameObject.AddComponent<Rigidbody>().AddForce(force);
                    if (v.objFlag != null)
                    {
                        v.objFlag.AddComponent<Rigidbody>().AddForce(force);
                    }

                    //演出
                    //GameObject obj =
                    //    Instantiate(prefabMineEx, v.gameObject.transform.position, Quaternion.identity);
                    //Destroy(obj, 1.95f);


                    txtResult.text = "Game Over!!";
                    result.SetActive(true);
                }
            }

            //数字が0以上なら終了
            if (0 < cell.MineCount) return;

            //周りの空セルもオープン
            List<CellController> roundcells = GetRoundCell(cell);

            while (roundcells.Count > 0)
            {
                CellController tgt = roundcells[0];

                if (tgt.Open())
                {
                    //0ならその周りのセルも追加
                    if (tgt.MineCount == 0)
                    {
                        roundcells.AddRange(GetRoundCell(tgt));
                    }

                    //演出
                    Vector3 pos = tgt.gameObject.transform.position;
                    pos.y = 1.0f;
                    GameObject obj =
                        Instantiate(prefabOpenEx, pos, Quaternion.identity);
                    Destroy(obj, 1.5f);

                }
                roundcells.Remove(tgt);
            }
        }

    }

    //クリア判定
    bool isClear()
    {
        //オープンされていないセルがあるか
        foreach (var v in cells)
        {
            if (!v.IsOpen && !v.IsMine) return false;
        }

        return true;
    }

    List<CellController> GetRoundCell(CellController center)
    {
        List<CellController> ret = new List<CellController>();

        //方向
        List<Vector2Int> dir = new List<Vector2Int>()
        {
            new Vector2Int( -1,  0 ),
            new Vector2Int( -1, +1 ),
            new Vector2Int(  0, +1 ),
            new Vector2Int( +1, +1 ),
            new Vector2Int( +1,  0 ),
            new Vector2Int( +1, -1 ),
            new Vector2Int(  0, -1 ),
            new Vector2Int( -1, -1 ),
        };

        int x = center.Pos.x;
        int y = center.Pos.y;

        foreach (var v in dir)
        {
            CellController cell = getCell(x + v.x, y + v.y);

            if (null == cell) continue;

            ret.Add(cell);
        }

        return ret;
    }

    //指定の場所のセルを返す
    CellController getCell(int x, int y)
    {
        //配列オーバーのチェック
        if (x < 0 || cells.GetLength(0) <= x
            || y < 0 || cells.GetLength(1) <= y)
        {
            return null;
        }

        return cells[x, y];
    }

    //リトライ
    public void Retry()
    {
        SceneManager.LoadScene("GameScene");
    }
}
