
public interface IEntity 
{
    public Team Team { get; set; }
    public int Health { get; set; }
    public void Hit();
}
