# Project-RA
**Roguelike Action Game** - Unity 게임 개발 프로젝트

## 📋 프로젝트 개요

Project-RA는 하데스(Hades) 스타일의 로그라이크 액션 게임을 목표로 하는 Unity 프로젝트입니다.
4인 팀으로 3개월 프로토타입 개발을 진행하며, 로그라이크 시스템과 액션 전투를 결합한 게임을 제작합니다.

### 🎮 게임 특징
- **장르**: 로그라이크 액션 RPG
- **시점**: 2.5D 탑다운 뷰
- **플랫폼**: PC (Windows/Mac), 모바일 확장 고려
- **개발 기간**: 3개월 프로토타입

## 🛠️ 기술 스택

- **엔진**: Unity 6000.0.58f2 (Unity 6)
- **렌더 파이프라인**: Universal Render Pipeline (URP)
- **입력 시스템**: New Input System
- **버전 관리**: Git with LFS
- **IDE**: JetBrains Rider, Visual Studio

## 🏗️ 프로젝트 구조

```
Assets/
├── 00_Imported/          # 외부 에셋 및 패키지
├── 01_Scripts/           # C# 스크립트
│   ├── Player/           # 플레이어 관련
│   ├── Enemy/            # 적 AI 및 행동
│   ├── Items/            # 아이템 시스템
│   ├── Dungeon/          # 던전 생성 및 관리
│   ├── Combat/           # 전투 시스템
│   ├── UI/               # 사용자 인터페이스
│   ├── Managers/         # 게임 매니저들
│   └── Utils/            # 유틸리티 스크립트
├── 02_Prefabs/           # 프리팹
├── 03_Art/               # 아트 에셋
│   ├── Materials/        # 머티리얼
│   ├── Textures/         # 텍스처
│   ├── Models/           # 3D 모델
│   └── Animations/       # 애니메이션
├── 04_Audio/             # 오디오 에셋
├── 05_Scenes/            # 씬 파일들
├── 06_Data/              # ScriptableObject 데이터
└── Settings/             # URP 및 프로젝트 설정
```

## 🎯 핵심 시스템

### 플레이어 컨트롤
- **이동**: WASD / 게임패드 스틱
- **시점**: 마우스 / 우측 스틱
- **공격**: 마우스 좌클릭 / X버튼
- **상호작용**: E / Y버튼 (Hold)
- **점프**: Space / A버튼
- **달리기**: Shift / 좌스틱 클릭

### 렌더링 설정
- **PC 버전**: 고품질 렌더링 (`PC_RPAsset`)
- **모바일 버전**: 최적화된 렌더링 (`Mobile_RPAsset`)
- **포스트 프로세싱**: Volume Profile 적용

## 🚀 시작하기

### 필수 요구사항
- Unity Hub
- Unity 6000.0.58f2 또는 최신 Unity 6 LTS
- Git (LFS 지원)
- Visual Studio 2022 또는 JetBrains Rider

### 설치 및 실행
1. **저장소 클론**:
   ```bash
   git clone https://github.com/kebap-studio/Project-RA.git
   cd Project-RA
   ```

2. **Unity Hub에서 프로젝트 열기**:
    - Unity Hub → Add → 프로젝트 폴더 선택
    - Unity 6000.0.58f2로 열기

3. **첫 실행**:
    - `Assets/Scenes/SampleScene.unity` 열기
    - Play 버튼으로 테스트

### 빌드 방법
1. **File → Build Settings**
2. **플랫폼 선택** (PC/Mac/Mobile)
3. **Player Settings에서 URP 에셋 확인**:
    - PC: `PC_RPAsset`
    - Mobile: `Mobile_RPAsset`
4. **Build** 실행

## 👥 팀 협업 가이드

### Git 워크플로우
```bash
# 새 기능 개발
git checkout -b feature/player-movement
git add .
git commit -m "feat: 플레이어 이동 시스템 구현"
git push origin feature/player-movement

# Pull Request 생성 후 병합
```

### 커밋 메시지 컨벤션
- `feat:` 새로운 기능
- `fix:` 버그 수정
- `docs:` 문서 수정
- `style:` 코드 포맷팅
- `refactor:` 코드 리팩토링
- `test:` 테스트 추가/수정

### 씬 작업 시 주의사항
1. **씬 병합 충돌 방지**:
    - 작업 전 최신 버전 pull
    - 가능한 한 서로 다른 씬에서 작업
    - 프리팹 활용으로 씬 의존성 최소화

2. **대용량 에셋 관리**:
    - 모든 아트 에셋은 Git LFS 사용
    - `.meta` 파일 포함하여 커밋

## 📁 코딩 컨벤션

### C# 스타일 가이드
```csharp
// 클래스명: PascalCase
public class PlayerController : MonoBehaviour
{
    // public 필드: PascalCase
    public float MoveSpeed = 5f;
    
    // private 필드: camelCase with underscore
    private Rigidbody _rigidbody;
    
    // 메서드: PascalCase
    private void HandleMovement()
    {
        // 지역변수: camelCase
        Vector3 moveDirection = Vector3.zero;
    }
}
```

### 폴더 네이밍
- **Scripts**: 기능별 분류 (Player, Enemy, UI 등)
- **Prefabs**: 용도별 분류 (Characters, Environment, UI 등)
- **Materials**: 용도_이름 형식 (Player_Body, Environment_Rock 등)

## 🔧 개발 도구 및 에셋

### 추천 Unity 패키지
- **Addressables**: 동적 콘텐츠 로딩
- **Cinemachine**: 카메라 시스템
- **Timeline**: 시네마틱 제작
- **TextMeshPro**: 텍스트 렌더링

### 외부 도구
- **Git LFS**: 대용량 파일 관리
- **Unity Cloud Build**: CI/CD (추후 고려)
- **Plastic SCM**: 대안 버전 관리 (필요시)

## 🐛 문제 해결

### 자주 발생하는 문제들

**1. Library 폴더 문제**
```bash
# Unity 프로젝트 재임포트
rm -rf Library/
# Unity에서 프로젝트 다시 열기
```

**2. Input System 인식 안됨**
- Window → Package Manager → Input System 설치 확인
- Player Settings → Active Input Handling → Input System Package

**3. URP 렌더링 문제**
- Graphics Settings에서 Render Pipeline Asset 확인
- Quality Settings에서 각 레벨별 설정 확인

## 📚 참고 자료

- [Unity 6 Documentation](https://docs.unity3d.com/6000.0/Documentation/Manual/)
- [URP Documentation](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest/)
- [New Input System Guide](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest/)
- [Git LFS Tutorial](https://git-lfs.github.io/)

## 📝 개발 로그

### 2025.10.07
- ✅ 초기 프로젝트 설정 완료
- ✅ URP 파이프라인 구성 (PC/Mobile)
- ✅ New Input System 설정
- ✅ Git 저장소 및 LFS 설정

### 다음 마일스톤
- [ ] 기본 플레이어 컨트롤러 구현
- [ ] 던전 생성 시스템 프로토타입
- [ ] 기본 전투 시스템
- [ ] UI 시스템 기반 구축

## 📞 연락처

**개발팀**: kebap-studio  
**저장소**: [https://github.com/kebap-studio/Project-RA](https://github.com/kebap-studio/Project-RA)

---

**마지막 업데이트**: 2025.10.08  
**Unity 버전**: 6000.0.58f2  
**프로젝트 상태**: 초기 설정 완료