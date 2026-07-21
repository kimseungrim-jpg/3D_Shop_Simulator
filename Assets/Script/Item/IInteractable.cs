using UnityEngine;

/// <summary>
/// 플레이어가 상호작용할 수 있는 오브젝트가 구현해야 하는 곹옽 기능을 정의
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// 오브젝트가 담당하는 상호작용 행동을 실행
    /// 플레이어가 상호작용을 입력 했을 때 상호작용 감지 시스템에서 호출
    /// </summary>
    void Interact();

    /// <summary>
    /// 현재 오브젝트와 수행할 수 있는 상호작용 안내 문구를 반환
    /// 플레이어가 오브젝트를 바라보거나 상호작용 범위에 들어왔을 때 UI 갱신에 사용
    /// </summary>
    string GetInteractText();
}
