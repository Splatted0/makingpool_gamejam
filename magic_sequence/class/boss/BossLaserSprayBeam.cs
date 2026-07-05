// 주사위6 발 1개의 시각 노드. 차징 중엔 패턴이 SetWidth로 두께만 갱신하고,
// Fire() 호출 이후엔 스스로 페이드아웃하다 사라진다. 판정(코어 명중)은 패턴 쪽에서 처리한다.
public partial class BossLaserSprayBeam : Node2D
{
	// 임팩트 순간 텍스처를 확 태우는 HDR 색(마법진 임팩트와 동일 기법 — 채널 1 초과로 밝기를 태움)
	private static readonly Color ImpactColor = new Color(2.5f, 2.5f, 2.5f, 1f);

	private Line2D _line;
	private float _fadeDuration;
	private float _fadeElapsed;
	private bool _firing;

	// texture는 Boss.LaserTexture(길이 전체에 늘어나는 한 장짜리 스프라이트)를 그대로 받는다.
	// backExtension만큼 시작점 뒤로도 이어 그려서 시작점이 화면 밖으로 나가게 한다(시작점이 보이면 어색해서).
	public void Setup(Vector2 direction, float length, float backExtension, Texture2D texture, float chargeWidth)
	{
		_line = new Line2D();
		_line.Texture = texture;
		_line.TextureMode = Line2D.LineTextureMode.Stretch;
		_line.Width = chargeWidth;
		_line.AddPoint(-direction * backExtension);
		_line.AddPoint(direction * length);
		AddChild(_line);
	}

	public void SetWidth(float width)
	{
		if (_line != null)
			_line.Width = width;
	}

	public void Fire(float fadeDuration)
	{
		_firing = true;
		_fadeDuration = fadeDuration;
		_fadeElapsed = 0f;
		if (_line != null)
			_line.Modulate = ImpactColor;   // 임팩트 순간 텍스처를 쨍하게 태운다
	}

	public override void _Process(double delta)
	{
		if (!_firing)
			return;

		_fadeElapsed += (float)delta;
		float t = Mathf.Clamp(_fadeElapsed / _fadeDuration, 0f, 1f);

		Color modulate = _line.Modulate;
		modulate.A = Mathf.Lerp(1f, 0f, t);
		_line.Modulate = modulate;

		if (_fadeElapsed >= _fadeDuration)
			QueueFree();
	}
}
