// 탄막 부채꼴 발사 유틸. 상시 패턴이 아니라 주사위3(BossDiceBarragePattern)에서만 쓴다.
public static class BossBarragePattern
{
	// 코어 방향 기준 부채꼴로 탄을 즉시 뿌린다.
	public static int FireFan(Boss boss, Vector2 baseDirection)
	{
		BossData data = boss.Config;
		int count = Mathf.Max(data.BarrageBulletCount, 1);
		float halfSpread = data.BarrageSpreadDegrees * 0.5f;
		float speed = data.BarrageBulletSpeed;

		for (int i = 0; i < count; i++)
		{
			float t = count == 1 ? 0.5f : (float)i / (count - 1);
			float angleDeg = Mathf.Lerp(-halfSpread, halfSpread, t);
			Vector2 direction = baseDirection.Rotated(Mathf.DegToRad(angleDeg));
			boss.SpawnBullet(direction, speed, data.BarrageDamage);
		}

		boss.PlayAttackAnim();
		return count;
	}
}
