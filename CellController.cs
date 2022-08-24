using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CellController : MonoBehaviour
{
    public GameObject text;
    public GameObject cube;

    public GameObject objFlag;

    public int MineCount;
    public bool IsMine;
    public bool IsOpen
    {
        get { return text.activeSelf; }
    }

    public Vector2Int Pos;

    // Start is called before the first frame update
    void Start()
    {
        text.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool Open()
    {
        //���ɊJ���Ă��� or �t���O�������Ă���
        if (text.activeSelf || null != objFlag) return false;

        //�e�L�X�g�\��
        text.SetActive(true);

        //�J���[�ݒ�
        cube.GetComponent<Renderer>().material.color =
            new Color32(225, 200, 175, 255);

        //���e�@
        if (IsMine)
        {
            cube.GetComponent<Renderer>().material.color = Color.red;
        }
        else if (0 < MineCount)
        {
            //�����\��
            text.GetComponent<TextMeshPro>().text = "" + MineCount;
            text.GetComponent<TextMeshPro>().color = Color.blue;
            if (3 < MineCount)
            {
                text.GetComponent<TextMeshPro>().color = new Color32(80, 0, 140, 255);
            }

            else if (2 < MineCount)
            {
                text.GetComponent<TextMeshPro>().color = Color.red;
            }
            else if (1 < MineCount)
            {
                text.GetComponent<TextMeshPro>().color = new Color32(10, 130, 0, 255);
            }
        }

        return true;
    }
}
