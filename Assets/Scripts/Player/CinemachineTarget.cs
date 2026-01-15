using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CinemachineTarget : MonoBehaviour
{
    private CinemachineTargetGroup cinemachineTargetGroup;
    [SerializeField] private Transform cursorTarget;

    private void Awake()
    {
        cinemachineTargetGroup = GetComponent<CinemachineTargetGroup>();
    }

    private void Start()
    {
        SetCinemachineTargetGroup();
    }

    private void Update()
    {
        cursorTarget.position = HelperUtilities.GetMouseWorldPosition();
    }

    private void SetCinemachineTargetGroup()
    {
        CinemachineTargetGroup.Target cinemachineGroupTarget_player = new() { Weight = 1f, Radius = 2.5f, Object = GameManager.Instance.GetPlayer().transform };

        CinemachineTargetGroup.Target cinemachineGroupTarget_cursor = new() { Weight = 1f, Radius = 1f, Object = cursorTarget };

        List<CinemachineTargetGroup.Target> cinemachineTargetList = new List<CinemachineTargetGroup.Target>();

        cinemachineTargetList.Add(cinemachineGroupTarget_player);

        cinemachineTargetList.Add(cinemachineGroupTarget_cursor);

        cinemachineTargetGroup.Targets = cinemachineTargetList;
    }
}