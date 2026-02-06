# REST API 통신 기능 구현 완료

**작성일:** 2026-02-06

---

## 변경 사항 요약

사용자 입력을 백엔드 서버로 전송하는 REST API 통신 기능을 구현했습니다.

---

## 생성된 파일

### `Assets/scripts/Ryu/ApiClient.cs`

REST API 통신을 담당하는 클래스입니다.

**주요 기능:**
- UnityWebRequest를 사용한 POST 요청
- JSON 직렬화/역직렬화
- 비동기 통신 (Coroutine)
- 성공/실패 콜백 지원

**Inspector 설정:**
- `Server Url`: 백엔드 서버 주소 (기본값: `http://localhost:8080/api/chat`)

---

## 수정된 파일

### `Assets/scripts/Ryu/InputHandler.cs`

**추가된 내용:**
- `ApiClient apiClient` 필드 추가
- 서버 통신 로직 연동
- 응답 처리 콜백 (`OnApiSuccess`, `OnApiError`)

**동작 흐름:**
1. 사용자가 텍스트 입력 후 엔터
2. "전송 중..." 표시
3. ApiClient를 통해 서버로 전송
4. 응답을 resultText에 표시

---

## API 스펙

### 요청 (Request)

- **Method:** POST
- **Content-Type:** application/json

```json
{
  "message": "사용자 입력 텍스트"
}
```

### 응답 (Response)

```json
{
  "success": true,
  "response": "서버 응답 메시지"
}
```

---

## Unity 씬 설정

**GameManager 오브젝트:**
- InputHandler 컴포넌트
- ApiClient 컴포넌트 (새로 추가됨)
- InputHandler.apiClient → ApiClient 연결 완료

---

## 사용 방법

1. Unity Inspector에서 **GameManager** 선택
2. **ApiClient** 컴포넌트의 `Server Url`을 실제 백엔드 URL로 변경
3. 플레이 모드에서 테스트

---

## 백엔드 담당자 요청 사항

위 API 스펙에 맞춰 엔드포인트를 구현해주세요.

**필요한 정보:**
- [x] API 스펙 (위 형식으로 결정됨)
- [ ] **서버 URL** (백엔드에서 제공 필요)

