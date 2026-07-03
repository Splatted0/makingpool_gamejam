using System;
using System.Collections.Generic;

// 몬스터가 하나씩 들고 있는 디버프 관리자. 타입당 하나만 유지한다.
// 같은 종류 재적용 시 스택++(최대 3), 지속시간은 더 긴 쪽으로 갱신.
public class DebuffController
{
	private const int MAX_STACKS = 3;

	private readonly Monster _owner;
	private readonly Dictionary<Type, IDebuff> _active = new();

	public DebuffController(Monster owner) // 생성자
	{
		_owner = owner;
	}

	// 각 Elemental에 대응하는 디버프 생성
	public static IDebuff Create(Elemental element)
	{
		switch (element)
		{
			case Elemental.Fire:
				return new FireEffect();
			case Elemental.Ice:
				return new IceEffect();
			case Elemental.Earth:
				return new EarthEffect();
			default:
				return null;
		}
	}

	public void Apply(IDebuff debuff)
	{
		Type key = debuff.GetType();

		if (_active.TryGetValue(key, out IDebuff existing))
		{
			// 이미 걸려있는 디버프라면 스택 추가
			if (existing.Stacks < MAX_STACKS) {
				existing.Stacks += 1;
			}

			// 지속시간은 더 긴 쪽으로 갱신
			if (debuff.Duration > existing.Duration) {
				existing.Duration = debuff.Duration;
			}

			existing.OnApply(_owner);   // 이미 걸려있던 디버프 갱신 시에도 OnApply 호출(재적용 효과)
			return;
		}

		_active[key] = debuff; // existing이 아니라면 새 디버프로 등록
		debuff.OnApply(_owner); // 새 디버프 적용 시 OnApply 호출
	}

	// Monster.cs에서 매 프레임마다 호출. 지속시간 감소, 틱데미지 적용, 만료 처리.
	public void Tick(float delta)
	{
		List<Type> expired = null;

		foreach (KeyValuePair<Type, IDebuff> pair in _active) {
			pair.Value.Duration -= delta;		// 지속시간 감소
			pair.Value.OnTick(_owner, delta);	// 틱당 효과 적용

			if (pair.Value.Duration <= 0f) {	// 만료된 디버프는 나중에 제거
				if (expired == null) {
					expired = new List<Type>();
				}
				expired.Add(pair.Key);
			}
		}

		if (expired == null) {
			return;
		}

		foreach (Type key in expired) {
			_active[key].OnExpire(_owner);
			_active.Remove(key);
		}
	}
	// TMI?
	// C#에서 foreach로 딕셔너리를 도는 중에 그 딕셔너리에서 Remove하면 "컬렉션이 수정됐다"고 예외가 터진다고 함. 
	// 그래서 도는 동안은 "지울 것들"만 따로 적어두고, 루프가 끝난 뒤 별도로 지우는 패턴 (claude 왈)
}
