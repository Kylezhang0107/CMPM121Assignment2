using UnityEngine;

public class StandStillTrigger : RelicTrigger
{
    private float requiredTime;
    private float lastMoveTime;
    private bool isArmed; 

    public StandStillTrigger(RuntimeRelic relic) : base(relic)
    {
        float.TryParse(relic.data.trigger.amount, out requiredTime);
        if (requiredTime <= 0f)
            requiredTime = 3f;
    }

    public override void Register()
    {
        lastMoveTime = Time.time;
        isArmed = true;
        EventBus.Instance.OnPlayerMove += OnMove;
    }

    public override void Unregister()
    {
        EventBus.Instance.OnPlayerMove -= OnMove;
    }

    public override void OnEffectRemoved()
    {
        isArmed = true;
        lastMoveTime = Time.time;
    }

    private void OnMove(Vector2 movement)
    {
        bool moving = movement.sqrMagnitude > 0.01f;

        if (moving)
        {
            lastMoveTime = Time.time;

            RelicManager.Instance.RemoveUntil("move");

            isArmed = true;
            return;
        }

        if (!isArmed)
            return;

        if (Time.time - lastMoveTime >= requiredTime)
        {
            isArmed = false;

            RelicManager.Instance.ActivateTemporary(relic);
        }
    }
}