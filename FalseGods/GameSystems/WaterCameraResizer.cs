using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class WaterCameraResizer : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera cmCamera;
    [SerializeField] private Camera waterCamera;
    [SerializeField] private Transform waterSpriteTrns;

    [SerializeField] private Vector2Int spriteDimensions = new Vector2Int(256, 256);

    [SerializeField] private int offset = 5;

    private float scaleMultiplierX;
    private float scaleMultiplierY;

    private void LateUpdate()
    {
        waterCamera.orthographicSize = Camera.main.orthographicSize;

        Vector3 lowerLeft = Camera.main.ScreenToWorldPoint(new Vector3(-offset, -offset, 0f));
        Vector3 upperRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width + offset, Screen.height + offset, 0f));

        scaleMultiplierX = upperRight.x - lowerLeft.x;
        scaleMultiplierY = upperRight.y - lowerLeft.y;

        waterSpriteTrns.localScale = new Vector3(scaleMultiplierX, scaleMultiplierY, 1f);
    }

    
}
