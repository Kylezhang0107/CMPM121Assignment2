public abstract class RelicTrigger
{
    protected RelicEffect effect;
    public RelicTrigger(RelicEffect effect)
    {
        this.effect = effect;
    }

    public abstract void Register();
    public abstract void Unregister();
}