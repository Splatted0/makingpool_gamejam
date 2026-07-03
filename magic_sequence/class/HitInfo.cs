// class/HitInfo.cs
// 데미지가 들어왔을 때 데미지량, 원소, 그리고 누구로부터 공격받은 건지를 표시
public struct HitInfo
{
    public int Damage;
    public Team SourceTeam;
    public MagicElement Element;
}