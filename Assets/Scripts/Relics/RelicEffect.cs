public abstract class RelicEffect
{
    public bool active;

    public virtual void Activate()
    {
        active = true;
    }

    public virtual void Remove()
    {
        active = false;
    }
}