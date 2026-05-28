public abstract class RelicEffect
{
    public bool active;
    public abstract void Activate();
    public virtual void Remove()
    {
        active = false;
    }
}