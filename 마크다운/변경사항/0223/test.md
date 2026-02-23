[GameStepApiClient] {
  "narrative": "안방의 어둠 속에서 빛나는 라이터를 발견했다. 그 작은 불꽃은 실내의 무거운 조용함을 가볍게 밝혔다. 손에 든 라이터는 그에게 새로운 힘을 주었고, 인간성이 조금씩 돌아온 기분이 들었다. 하지만 그 빛은 이곳의 기괴한 분위기를 더욱 강조했으며, 더욱 깊은 무언가가 숨어 있다는 생각이 들게 만들었다. 이집트식 천장 아래, 그림자들이 더욱 거칠게 움직이는 것을 느낄 수 있었다. 그 순간, 안방의 문이 조용히 열렸다. (주어진 정보를 바탕으로 생성된 텍스트) 안방의 어둠 속에서 라이터를 발견하고, 그 불꽃이 실내의 무거운 조용함을 가볍게 밝혔다. 손에 든 라이터는 그에게 새로운 힘을 주었고, 인간성이 조금씩 돌아온 기분이 들었다. 그러나 그 빛은 이곳의 기괴한 분위기를 더욱 강조했으며, 더욱 깊은 무언가가 숨어 있다는 생각이 들게 만들었다. 이집트식 천장 아래, 그림자들이 더욱 거칠게 움직이는 것을 느낄 수 있었고, 문이 조용히 열리는 소리마저 그의 심장을 두근거리게 했다. (편집 및 개선된 버전) \n\n(추가 정보를 제공하지 않았으므로, 원래 텍스트를 유지하면서 더 자연스럽게 상태 변화를 녹여넣었습니다.)",
  "ending_info": null,
  "state_result": {
    "npc_stats": {},
    "flags": null,
    "inventory_add": null,
    "inventory_remove": null,
    "item_state_changes": null,
    "npc_disabled_states": null,
    "humanity": null,
    "vars": {
      "suspicion_level": 2.0,
      "humanity": 1.0
    }
  },
  "debug": {
    "game_id": 101,
    "reasoning": null,
    "steps": [
      {
        "step": "day_turn",
        "state_delta": {
          "vars": {
            "suspicion_level": 2,
            "humanity": 1
          },
          "npc_stats": {},
          "turn_increment": 1
        },
        "reached": false,
        "newly_unlocked": null
      }
    ],
    "turn_after": 0
  }
}
UnityEngine.Debug:Log (object)
GameStepApiClient/<SendMessageCoroutine>d__9:MoveNext () (at Assets/Scripts/Ryu/Global/API/GameStepApiClient.cs:153)
UnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)

