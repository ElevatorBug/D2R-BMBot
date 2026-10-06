using System;
using System.Drawing;
using System.Threading;

public partial class Form1
{
    private readonly object memoryWarningLock = new object();
    private DateTime lastMemoryWarningUtc = DateTime.MinValue;
    private PlayerStateSnapshot pendingPlayerState;
    private int playerPresentationQueued;

    private void PresentPlayerState(PlayerStateSnapshot state)
    {
        if (IsDisposed || Disposing || !IsHandleCreated) return;
        Interlocked.Exchange(ref pendingPlayerState, state);
        if (Interlocked.CompareExchange(ref playerPresentationQueued, 1, 0) != 0) return;
        // Several scans may finish before the UI paints. Render the latest one once.
        if (!QueuePresentation(() =>
        {
            Interlocked.Exchange(ref playerPresentationQueued, 0);
            RenderPlayerState(Volatile.Read(ref pendingPlayerState));
        })) Interlocked.Exchange(ref playerPresentationQueued, 0);
    }

    private void RenderPlayerState(PlayerStateSnapshot state)
    {
        Grid_SetInfos("Cords", state.X + "," + state.Y);
        Grid_SetInfos("Life", state.Life + "/" + state.MaxLife);
        Grid_SetInfos("Mana", state.Mana + "/" + state.MaxMana);
        Grid_SetInfos("Map Level", state.AreaId + " " + (Enums.Area)state.AreaId);
    }

    private void ReportMemoryReadFailure(MemoryReadFailure failure)
    {
        // Invalid pointers can occur repeatedly while entering or leaving a game.
        // Throttle diagnostics without hiding the native error or blocking the reader.
        lock (memoryWarningLock)
        {
            DateTime now = DateTime.UtcNow;
            if ((now - lastMemoryWarningUtc).TotalSeconds < 5) return;
            lastMemoryWarningUtc = now;
        }

        string message = string.Format("Memory read at 0x{0:X}: {1}/{2} bytes (Windows error {3}).",
            failure.Address.ToInt64(), failure.BytesRead, failure.RequestedBytes, failure.ErrorCode);
        QueuePresentation(() => AppendLog(message, Color.OrangeRed));
    }

    private bool QueuePresentation(Action action)
    {
        if (IsDisposed || Disposing || !IsHandleCreated) return false;
        try
        {
            BeginInvoke(new Action(() =>
            {
                if (!IsDisposed && !Disposing) action();
            }));
            return true;
        }
        catch (ObjectDisposedException) { }
        catch (InvalidOperationException) { } // The window handle was destroyed during shutdown.
        return false;
    }
}
