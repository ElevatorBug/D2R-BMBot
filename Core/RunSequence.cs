using System;

public sealed class RunStep
{
    private readonly Func<bool> canRun;
    private readonly Action execute;

    public RunStep(Func<bool> canRun, Action execute)
    {
        this.canRun = canRun ?? throw new ArgumentNullException(nameof(canRun));
        this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
    }

    internal bool TryExecute()
    {
        if (!canRun()) return false;
        execute();
        return true;
    }
}

/// <summary>Evaluates current eligibility and executes at most one step per tick.</summary>
public sealed class RunSequence
{
    private readonly RunStep[] steps;

    public RunSequence(params RunStep[] steps)
    {
        if (steps == null) throw new ArgumentNullException(nameof(steps));
        this.steps = (RunStep[])steps.Clone();
        foreach (RunStep step in this.steps)
            if (step == null) throw new ArgumentException("A run step cannot be null.", nameof(steps));
    }

    public bool TryExecuteNext()
    {
        foreach (RunStep step in steps)
            if (step.TryExecute()) return true;
        return false;
    }
}
