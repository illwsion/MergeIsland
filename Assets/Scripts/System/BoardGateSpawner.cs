// BoardGateSpawner.cs
using UnityEngine;
using System.Collections.Generic;

public class BoardGateSpawner : MonoBehaviour
{
    public GameObject gatePrefab; // ȸ�� ������ ������
    public Transform gateParent; // ���� ��ġ (ex: BoardGates ������Ʈ)
    public float offset = 1.1f; // ����Ʈ ��ġ ������

    public static BoardGateSpawner Instance;

    void Awake()
    {
        Instance = this;
    }

    public void SpawnGates(string boardKey, Vector3 gridOrigin, float cellSize, int width, int height)
    {
        //���� ����Ʈ ����
        foreach (Transform child in gateParent)
        {
            Destroy(child.gameObject);
        }

        float boardWidth = cellSize * width;
        float boardHeight = cellSize * height;
        float margin = cellSize / 4;

        foreach (var gateData in BoardGateManager.Instance.GetGatesForBoard(boardKey))
        {
            Vector3 localOffset = GetOffset(gateData.direction, boardWidth, boardHeight, margin);

            GameObject gate = Instantiate(gatePrefab, gateParent);
            gate.transform.localPosition = localOffset;

            gate.transform.rotation = GetGateRotation(gateData.direction);

            BoardGate gateComponent = gate.GetComponent<BoardGate>();
            gateComponent.Initialize(gateData);
        }
    }

    private Vector3 GetOffset(BoardGateData.Direction direction, float boardWidth, float boardHeight, float margin)
    {
        switch (direction)
        {
            case BoardGateData.Direction.Top: return new Vector3(0, boardHeight / 2f + margin, 0);
            case BoardGateData.Direction.Bottom: return new Vector3(0, -boardHeight / 2f - margin, 0);
            case BoardGateData.Direction.Right: return new Vector3(boardWidth / 2f + margin, 0, 0);
            case BoardGateData.Direction.Left: return new Vector3(-boardWidth / 2f - margin, 0, 0);
            default: return Vector3.zero;
        }
    }

    private Quaternion GetGateRotation(BoardGateData.Direction direction)
    {
        switch (direction)
        {
            case BoardGateData.Direction.Top: return Quaternion.Euler(0, 0, 0);
            case BoardGateData.Direction.Right: return Quaternion.Euler(0, 0, -90);
            case BoardGateData.Direction.Bottom: return Quaternion.Euler(0, 0, 180);
            case BoardGateData.Direction.Left: return Quaternion.Euler(0, 0, 90);
            default: return Quaternion.identity;
        }
    }
}
