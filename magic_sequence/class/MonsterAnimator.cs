using System;

// 몬스터 애니메이션 재생 담당. Monster는 "무슨 일이 일어났는지"만 알려주고
// 어떤 애니 이름을 트는지는 여기서 결정한다(애니 이름 문자열이 Monster에 흩어지지 않게).
// 걷기/공격은 반복(loop) 상태 애니라 매 프레임 호출해도 되고,
// 피격/사망은 단발(one-shot)이라 끝날 때까지 다른 애니가 못 끊게 락을 건다.
// ※ SpriteFrames에서 hit/die는 Loop OFF여야 함(안 그러면 종료 신호가 안 와 die가 영영 안 끝남).
public class MonsterAnimator
{
	private const string WALK = "walk";
	private const string ATTACK = "attack";
	private const string HIT = "hit";
	private const string DIE = "die";

	private readonly AnimatedSprite2D _sprite;
	private bool _locked;               // 단발 애니 재생 중 — walk/attack 무시
	private bool _selfDestructing;      // 자폭 시퀀스(공격→사망) 진행 중
	private Action _onDeathFinished;    // 사망/자폭 애니가 끝나면 실행(=QueueFree)

	public MonsterAnimator(AnimatedSprite2D sprite)
	{
		_sprite = sprite;
		if (_sprite != null)
		{
			_sprite.AnimationFinished += OnAnimationFinished;
		}
	}

	public void PlayWalk()
	{
		PlayLoop(WALK);
	}

	public void PlayAttack()
	{
		PlayLoop(ATTACK);
	}

	// 피격. 단발로 재생하고 끝나면 락 해제 → 다음 프레임에 walk/attack가 복귀한다.
	public void PlayHit()
	{
		PlayOneShot(HIT);
	}

	// 사망. 단발 재생 후 애니가 끝나면 onFinished 실행(그때 QueueFree).
	// die 애니가 없으면 종료 신호를 기다릴 수 없으니 즉시 onFinished.
	public void PlayDeath(Action onFinished)
	{
		_onDeathFinished = onFinished;

		if (_sprite == null || HasAnimation(DIE) == false)
		{
			onFinished?.Invoke();
			return;
		}

		PlayOneShot(DIE);
	}

	// 자폭(근거리 본진 접촉). 공격 애니 → 죽는 애니 → onFinished(=QueueFree) 순서로 재생.
	// ※ attack/die가 Loop OFF여야 순서대로 넘어간다(loop면 종료 신호가 안 와서 멈춤).
	public void PlaySelfDestruct(Action onFinished)
	{
		_onDeathFinished = onFinished;

		// 공격 애니가 없으면 죽는 애니만(그것도 없으면 PlayDeath가 즉시 콜백)
		if (_sprite == null || HasAnimation(ATTACK) == false)
		{
			PlayDeath(onFinished);
			return;
		}

		_selfDestructing = true;
		_locked = true;
		_sprite.Play(ATTACK);
	}

	// 반복 상태 애니(걷기/공격). 단발 재생 중이거나 이미 그 애니면 무시.
	private void PlayLoop(string name)
	{
		if (_sprite == null || _locked)
		{
			return;
		}
		if (_sprite.Animation == name || HasAnimation(name) == false)
		{
			return;
		}
		_sprite.Play(name);
	}

	// 단발 애니(피격/사망). 끝날 때까지 락을 걸어 다른 애니가 못 끊게 한다.
	private void PlayOneShot(string name)
	{
		if (_sprite == null || HasAnimation(name) == false)
		{
			return;
		}
		_locked = true;
		_sprite.Play(name);
	}

	private void OnAnimationFinished()
	{
		string finished = _sprite.Animation;

		// 자폭 시퀀스: 공격 애니가 끝나면 죽는 애니로 넘어간다
		if (_selfDestructing && finished == ATTACK)
		{
			if (HasAnimation(DIE))
			{
				_sprite.Play(DIE);
				return;
			}
			_selfDestructing = false;
			_onDeathFinished?.Invoke();
			return;
		}

		// 사망(또는 자폭 마지막) 애니가 끝났으면 콜백(QueueFree) 실행
		if (finished == DIE)
		{
			_selfDestructing = false;
			_onDeathFinished?.Invoke();
			return;
		}

		_locked = false;   // 피격 등 단발 종료 → 다음 프레임에 상태 애니 복귀
	}

	private bool HasAnimation(string name)
	{
		if (_sprite.SpriteFrames == null)
		{
			return false;
		}
		return _sprite.SpriteFrames.HasAnimation(name);
	}
}
