// 보스 패턴 한 조각의 계약. 주 패턴(레이저·탄막)도 주사위 패턴도 이 형태로 구현한다.
// Start로 시작 → 매 프레임 Tick → IsFinished가 되면 보스가 다음으로 넘어간다(1회성 사이클).
public interface IBossPattern
{
	void Start(Boss boss);
	void Tick(Boss boss, double delta);
	bool IsFinished { get; }

	// 조건 미충족 등으로 아무 효과 없이 취소됐는지. true면 컨트롤러가 다음 주사위 쿨다운을 스킵한다.
	bool WasCancelled { get; }

	// 보스 사망 등으로 패턴이 도중에 끊길 때, 걸어둔 상태(속박·쿨다운 배율 등)를 되돌린다.
	// 기본은 정리할 것 없음 — 지속 상태를 거는 패턴만 오버라이드한다.
	void Cancel(Boss boss) { }
}
