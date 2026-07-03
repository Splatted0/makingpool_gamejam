// class/HitInfo.cs
// 데미지가 들어왔을 때 데미지량, 원소, 그리고 누구로부터 공격받은 건지를 표시
public struct HitInfo
{
    public int Damage;
    public Team SourceTeam;
    // 확장성: 지금은 단일 원소. 2개 조합(콤보)을 처리하려면 List<Elemental>로 바꿔야 할 수 있음.
    // 그 경우 Monster.Hit의 디버프 적용부를 리스트 순회로 확장.
    public Elemental Element;
}