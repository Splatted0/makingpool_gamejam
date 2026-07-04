// 보스 패턴 한 조각의 계약. 주 패턴(레이저·탄막)도 주사위 패턴도 이 형태로 구현한다.
// Start로 시작 → 매 프레임 Tick → IsFinished가 되면 보스가 다음으로 넘어간다(1회성 사이클).
public interface IBossPattern
{
	void Start(Boss boss);
	void Tick(Boss boss, double delta);
	bool IsFinished { get; }
}
