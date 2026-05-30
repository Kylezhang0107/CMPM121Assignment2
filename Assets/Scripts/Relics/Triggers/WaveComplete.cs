public class WaveCompleteTrigger : RelicTrigger
{
    public WaveCompleteTrigger(RelicEffect effect) : base(effect)
    {
    }

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
        effect.Activate();
    }
}