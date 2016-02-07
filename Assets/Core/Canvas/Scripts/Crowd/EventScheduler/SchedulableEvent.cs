/// <summary>
/// A simple event that can be used in an EventCollection. Contains both
/// an EventSignature as well as a probability with which the event should
/// be executed.
/// </summary>
public class SchedulableEvent 
{
    public readonly EventSignature Signature;

    public float Probability { get; private set; }

    public SchedulableEvent(EventSignature signature, float probability)
    {
        this.Signature = signature;
        this.Probability = probability;
    }

    /// <summary>
    /// Adapts the probability by dividing it through the given factor.
    /// </summary>
    public void AdaptProbability(float factor)
    {
        Probability = Probability / factor;
    }
}
