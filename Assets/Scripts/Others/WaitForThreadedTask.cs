using System;
using System.Threading;

/// <summary>
/// Custom yield instruction used for execute methods hard to divide in parts executable in different frames
/// </summary>
public class WaitForThreadedTask : UnityEngine.CustomYieldInstruction
{
    private bool isRunning;

    public WaitForThreadedTask(Action task, ThreadPriority priority = ThreadPriority.Normal)
    {
        isRunning = true;
        Thread thread = new Thread(() => { task(); isRunning = false; });
        thread.Priority = priority;
        thread.Start();
    }

    public override bool keepWaiting { get { return isRunning; } }
}
