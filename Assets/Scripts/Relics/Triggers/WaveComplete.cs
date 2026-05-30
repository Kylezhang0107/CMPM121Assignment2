public class WaveCompleteTrigger : RelicTrigger
{
    public WaveCompleteTrigger(RuntimeRelic relic) : base(relic) { }

    public override void Register()
    {
        EventBus.Instance.OnWaveComplete += OnWaveComplete;
    }

    public override void Unregister()
    {
        EventBus.Instance.OnWaveComplete -= OnWaveComplete;
    }

    private void OnWaveComplete(int wave)
    {
        if (relic.IsTemporary())
            RelicManager.Instance.ActivateTemporary(relic);
        else
            relic.effect.Activate();
    }
}