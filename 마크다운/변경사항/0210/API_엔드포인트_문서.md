# API 엔드포인트 문서

## 개요
백엔드 API 엔드포인트 스펙 문서입니다. Swagger UI를 기반으로 작성되었습니다.

## 작업일: 2026-02-10

---

## 1. 시나리오 API

### 1.1 사용 가능한 시나리오 목록 조회

**엔드포인트:** `GET /api/v1/scenario/`

**설명:** 사용 가능한 시나리오 목록을 조회합니다.

**요청:**
- HTTP Method: `GET`
- Path: `/api/v1/scenario/`
- 파라미터: 없음

**응답:**
- (응답 형식은 이미지에서 확인되지 않음)

---

## 2. 게임 API

### 2.1 게임 대화 요청

**엔드포인트:** `POST /api/v1/game/{game_id}/step`

**설명:** 게임 내 대화를 진행합니다.

**요청:**
- HTTP Method: `POST`
- Path: `/api/v1/game/{game_id}/step`
- Content-Type: `application/json`

**파라미터:**

| 이름 | 타입 | 위치 | 필수 | 설명 | 예시 값 |
|------|------|------|------|------|---------|
| `game_id` | integer | path | ✅ 필수 | 게임 ID | `12` |

**Request Body:**

```json
{
  "chat_input": "string",
  "npc_name": "string",
  "item_name": "string"
}
```

**Request Body 필드 설명:**

| 필드명 | 타입 | 필수 | 설명 |
|--------|------|------|------|
| `chat_input` | string | ✅ 필수 | 사용자의 대화 입력 |
| `npc_name` | string | ✅ 필수 | NPC 이름 |
| `item_name` | string | ✅ 필수 | 아이템 이름 |

**요청 예시:**
```
POST /api/v1/game/12/step
Content-Type: application/json

{
  "chat_input": "안녕하세요",
  "npc_name": "계모",
  "item_name": "빵"
}
```

**응답:**
- (응답 형식은 이미지에서 확인되지 않음)

---

## 3. 참고 사항

- 모든 API는 RESTful 방식으로 설계되었습니다.
- 필수 파라미터는 `* required`로 표시되어 있습니다.
- Path 파라미터는 URL 경로에 포함되며, Query 파라미터는 URL 쿼리 스트링에 포함됩니다.
- Request Body는 JSON 형식으로 전송됩니다.

---

## 4. 관련 문서

- [백엔드_API_형식_변경_대응_계획.md](./백엔드_API_형식_변경_대응_계획.md)
- [Phase1_데이터_모델_정의.md](./Phase1_데이터_모델_정의.md)
- [Phase2_NPC_시스템_구현.md](./Phase2_NPC_시스템_구현.md)

