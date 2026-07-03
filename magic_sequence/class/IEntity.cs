// 진영과 hp를 표시. 그리고 피격받았다는 HitInfo 상속
public interface IEntity
{
    Team Team { get; set; }
    int Health { get; set; }

    void Hit(HitInfo hitInfo);
}