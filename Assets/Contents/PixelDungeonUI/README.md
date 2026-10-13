# Pixel Dungeon UI

텍스트 던전 게임용 픽셀 UI 리소스입니다. 모든 PNG는 16px 기준 원본을 4배(nearest)로 키운 것이고, 배경만 480x270 원본을 4배(1920x1080)로 키웠습니다.

## 폴더 구성

| 폴더 | 파일 | 용도 |
|---|---|---|
| Background | bg_dungeon.png | 전체 배경 (1920x1080) |
| Panels | panel_stone.png | 플레이어 정보 패널 |
| Panels | panel_parchment.png | 방 정보 패널 (양피지, 글자는 어두운 색 사용) |
| Panels | panel_wood.png | 메시지 패널 |
| Panels | tab_title.png | 패널 위 제목 탭 (PLAYER, ROOM) |
| Panels | bar_frame.png, bar_fill_hp.png, bar_fill_gold.png | HP/골드 게이지 |
| Buttons | button_normal / hover / pressed / disabled.png | 버튼 상태 4종 |
| Icons | icon_attack, chest, rest, enter, next, return | 버튼 6개용 아이콘 (64x64) |
| Icons | icon_player, hp, gold, room, monster, message | 패널용 아이콘 (64x64) |
| Fonts | Galmuri11.ttf, Galmuri9.ttf | 한글 픽셀 폰트 (SIL OFL, 라이선스 동봉) |

## Unity 임포트 설정 (전체 공통)

- Texture Type: **Sprite (2D and UI)**
- Filter Mode: **Point (no filter)**
- Compression: **None**
- Generate Mip Maps: 끔

## 9-Slice Border (Sprite Editor > Border, L/T/R/B 모두 같은 값)

| 스프라이트 | 크기 | Border |
|---|---|---|
| panel_stone / parchment / wood | 96x96 | 24 |
| button_* | 128x64 | 20 |
| tab_title | 96x44 | 16 |
| bar_frame | 128x32 | 12 |

Image 컴포넌트는 Image Type을 **Sliced**로 두세요. 테두리가 너무 두꺼우면 Image의 **Pixels Per Unit Multiplier**를 2로 올리면 절반 두께가 됩니다(픽셀 크기를 화면 전체에서 통일하는 게 중요).

## 버튼 설정

- Image Source: button_normal
- Transition: **Sprite Swap**
  - Highlighted: button_hover
  - Pressed: button_pressed
  - Selected: button_normal
  - Disabled: button_disabled
- 자식 Image에 아이콘을 넣고 Preserve Aspect를 켜세요. 비활성 상태에서는 아이콘 색을 회색(예: #6B6478)으로 바꾸면 미리보기처럼 보입니다.

## 폰트 (TextMeshPro)

Window > TextMeshPro > Font Asset Creator에서 Galmuri11.ttf를 고르고 Render Mode를 **RASTER_HINTED**(SDF 말고)로, Sampling Point Size를 11 또는 12로 지정하세요. 한글은 Character Set을 Custom Range로 `AC00-D7A3`을 추가하거나, 실제 쓰는 문자만 넣어 아틀라스 크기를 줄일 수 있습니다.

## 권장 색

| 용도 | 색 |
|---|---|
| 기본 글자 | #E8E4F0 |
| 강조/골드 | #F2C14E |
| HP | #FF8A80 |
| 양피지 위 글자 | #3B3346 |
| 양피지 위 경고 | #7A1F1F |
