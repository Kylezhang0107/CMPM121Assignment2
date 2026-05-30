public abstract class RelicTrigger
{
    protected RuntimeRelic relic;

    protected RelicTrigger(RuntimeRelic relic)
    {
        this.relic = relic;
    }

    public abstract void Register();
    public abstract void Unregister();

    public virtual void OnEffectRemoved() { }
}