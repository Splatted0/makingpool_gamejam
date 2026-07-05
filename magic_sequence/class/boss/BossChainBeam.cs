// 주사위4(속박) 연출: 고정된 시작점(화면 모서리)에서 코어까지 이어지는 사슬 한 가닥.
// 시작점은 고정이고, 끝점은 UpdateEnd로 매 프레임 갱신해 코어가 제자리에서 흔들리는 걸 따라간다.
public partial class BossChainBeam : Node2D
{
	private Line2D _line;
	private float _fadeDuration;
	private float _fadeElapsed;
	private bool _fading;

	public void Setup(Vector2 startWorldPos, Vector2 endWorldPos, Texture2D texture, float width)
	{
		GlobalPosition = startWorldPos;

		_line = new Line2D();
		_line.Texture = texture;
		_line.TextureMode = Line2D.LineTextureMode.Tile;
		_line.TextureRepeat = CanvasItem.TextureRepeatEnum.Enabled;   // Tile 모드는 이게 켜져 있어야 실제로 반복됨(안 그러면 가장자리 색으로 늘어남)
		_line.Width = width;
		_line.AddPoint(Vector2.Zero);
		_line.AddPoint(ToLocal(endWorldPos));
		AddChild(_line);
	}

	// 끝점(코어 쪽)을 매 프레임 최신 위치로 갱신한다. 시작점(화면 모서리)은 그대로 둔다.
	public void UpdateEnd(Vector2 endWorldPos)
	{
		_line?.SetPointPosition(1, ToLocal(endWorldPos));
	}

	public void Fade(float duration)
	{
		_fading = true;
		_fadeDuration = duration;
		_fadeElapsed = 0f;
	}

	public override void _Process(double delta)
	{
		if (!_fading)
			return;

		_fadeElapsed += (float)delta;
		float t = Mathf.Clamp(_fadeElapsed / _fadeDuration, 0f, 1f);

		Color color = _line.Modulate;
		color.A = Mathf.Lerp(1f, 0f, t);
		_line.Modulate = color;

		if (_fadeElapsed >= _fadeDuration)
			QueueFree();
	}
}
