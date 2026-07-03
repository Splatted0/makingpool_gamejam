// 디버프 계약. 남은 지속시간·스택을 갖고, 발동/틱/종료 시점에 몬스터 상태를 바꾼다.
// 실제 효과 로직은 각 원소 클래스(FireEffect/IceEffect/EarthEffect)가 구현한다.
public interface IDebuff
{
	float Duration { get; set; }     // 남은 지속시간(초). 컨트롤러가 매 프레임 감소시킨다
	int Stacks { get; set; }         // 중첩 수(1~3). 화상·슬로우는 스택이 효과수치를 키운다

	void OnApply(Monster monster);             // 발동·재적용 시. 현재 스택 기준으로 효과 재계산(멱등)
	void OnTick(Monster monster, float delta); // 매 프레임. 화상 틱데미지 등
	void OnExpire(Monster monster);            // 만료 시. 효과 해제
}
