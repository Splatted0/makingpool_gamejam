// 보스 애니메이션 재생 담당. MonsterAnimator와 달리 idle 하나만 반복이고 나머지(attack/buff/
// debuff/diceroll/lucky)는 전부 단발이라, 패턴 이벤트가 트면 단발을 튼 뒤 끝나면 자동으로 idle로
// 돌아온다. die는 이미 상속받은 MonsterAnimator(부모 Monster._animator)가 처리하므로 여기서는
// 무시한다(사망 시 QueueFree까지 그쪽이 담당).
public class BossAnimator
{
	private const string IDLE = "idle";
	private const string ATTACK = "attack";
	private const string BUFF = "buff";
	private const string DEBUFF = "debuff";
	private const string DICEROLL = "diceroll";
	private const string DIE = "die";
	private const string LUCKY = "lucky";

	private readonly AnimatedSprite2D _sprite;
	private bool _locked;

	public BossAnimator(AnimatedSprite2D sprite)
	{
		_sprite = sprite;
		if (_sprite != null)
			_sprite.AnimationFinished += OnAnimationFinished;
	}

	public void PlayIdle() => PlayLoop(IDLE);
	public void PlayAttack() => PlayOneShot(ATTACK);
	public void PlayBuff() => PlayOneShot(BUFF);
	public void PlayDebuff() => PlayOneShot(DEBUFF);
	public void PlayDiceRoll() => PlayOneShot(DICEROLL);
	public void PlayLucky() => PlayOneShot(LUCKY);

	// Animation 문자열이 이미 name과 같아도 Play()를 부른 적 없으면 안 도는 상태일 수 있어서
	// (씬 로드 직후가 그 경우) IsPlaying()까지 같이 봐야 진짜 재생 중인지 판단된다.
	private void PlayLoop(string name)
	{
		if (_sprite == null || _locked || !HasAnimation(name))
			return;
		if (_sprite.Animation == name && _sprite.IsPlaying())
			return;

		_sprite.Play(name);
	}

	// 이미 같은 단발 애니가 재생 중이면 무시(매 프레임 호출돼도 재시작 안 되게), 다른 단발이면 끊고 갈아탄다.
	private void PlayOneShot(string name)
	{
		if (_sprite == null || !HasAnimation(name))
			return;
		if (_locked && _sprite.Animation == name)
			return;

		_locked = true;
		_sprite.Play(name);
	}

	private void OnAnimationFinished()
	{
		string finished = _sprite.Animation;
		if (finished == DIE)
			return;   // 사망 애니는 MonsterAnimator 쪽이 QueueFree까지 담당하니 여기선 손 안 댐

		_locked = false;
		PlayLoop(IDLE);
	}

	private bool HasAnimation(string name)
	{
		return _sprite.SpriteFrames != null && _sprite.SpriteFrames.HasAnimation(name);
	}
}
