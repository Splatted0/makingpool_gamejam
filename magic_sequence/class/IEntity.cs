// 진영과 hp를 표시. 그리고 피격받았다는 HitInfo 상속
public interface IEntity
{
    public Team Team { get; set; }
    public int Health { get; set; }
    public void Hit();
}