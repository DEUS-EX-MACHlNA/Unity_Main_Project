# 백엔드 API 연동 요청사항

Unity 클라이언트에서 REST API로 통신하기 위해 다음 정보가 필요합니다.

---

## 1. 서버 접속 정보

- [ ] **서버 주소** (아래 중 하나)
  - 공인 IP + 포트 (예: `http://203.123.45.67:8080`)
  - 도메인 URL (예: `https://api.example.com`)
  - 클라우드 URL (예: `https://my-app.aws.com`)

> ⚠️ 같은 네트워크가 아니므로 사설 IP(192.168.x.x)는 사용 불가

---

## 2. API 엔드포인트

- [ ] 엔드포인트 경로 (예: `/api/chat`, `/api/message`)
- [ ] HTTP 메서드 (`POST`, `GET` 등)

---

## 3. 요청(Request) 형식

- [ ] Content-Type (보통 `application/json`)
- [ ] JSON 필드명 및 구조

예시:
```json
{
  "message": "사용자 입력 텍스트"
}
```

---

## 4. 응답(Response) 형식

- [ ] 성공/실패 판단 기준
- [ ] JSON 필드명 및 구조

예시:
```json
{
  "success": true,
  "response": "서버 응답 메시지"
}
```

---

## 5. 인증(Authentication)

- [ ] 인증 방식 (API Key, Bearer Token, 없음 등)
- [ ] 헤더에 포함할 키/값

예시:
```
Authorization: Bearer {API_KEY}
```

---

## 6. 기타

- [ ] 테스트용 API 또는 Postman 컬렉션 공유 가능 여부
- [ ] 에러 코드 및 메시지 형식


