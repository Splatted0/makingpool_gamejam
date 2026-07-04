// 주사위6 발 1개의 시각 노드. 차징 중엔 패턴이 SetWidth로 두께만 갱신하고,
// Fire() 호출 이후엔 스스로 페이드아웃하다 사라진다. 판정(코어 명중)은 패턴 쪽에서 처리한다.
public partial class BossLaserSprayBeam : Node2D
{
	private Line2D _line;
	private float _fadeDuration;
	private float _fadeElapsed;
	private bool _firing;

	public void Setup(Vector2 direction, float length, Color color, float chargeWidth)
	{
		_line = new Line2D();
		_line.DefaultColor = color;
		_line.Width = chargeWidth;
		_line.AddPoint(Vector2.Zero);
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
	}

	public override void _Process(double delta)
	{
		if (!_firing)
			return;

		_fadeElapsed += (float)delta;
		float t = Mathf.Clamp(_fadeElapsed / _fadeDuration, 0f, 1f);

		Color color = _line.DefaultColor;
		color.A = Mathf.Lerp(1f, 0f, t);
		_line.DefaultColor = color;

		if (_fadeElapsed >= _fadeDuration)
			QueueFree();
	}
}
