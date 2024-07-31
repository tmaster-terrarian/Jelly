using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework;

namespace Jelly;

public abstract class GameServer : Game
{
    public bool Server { get; set; }

    bool _disposed;
    bool _initialized;
    bool _shouldExit;

    Stopwatch _gameTimer;
    TimeSpan _accumulatedElapsedTime;
    readonly GameTime _gameTime = new();
    long _previousTicks;
    int _updateFrameLag;

    public GameServer()
    {
        Disposed += SetDisposed;
    }

    private void SetDisposed(object sender, EventArgs e) => _disposed = true;

    public new void Run()
    {
        if(!Server)
        {
            base.Run();
            return;
        }

        Run(GameRunBehavior.Synchronous);
    }

    public new void Run(GameRunBehavior runBehavior)
    {
        if(!Server)
        {
            base.Run(runBehavior);
            return;
        }

        ObjectDisposedException.ThrowIf(_disposed, this);

        if(!_initialized)
        {
            DoInitialize();
            _initialized = true;
        }

        BeginRun();
        switch(runBehavior)
        {
            case GameRunBehavior.Synchronous:
                DoUpdate(new GameTime());
                RunLoop();
                break;
            default:
                throw new ArgumentException($"Handling for the run behavior {runBehavior} is not implemented.");
        }
    }

    private void DoInitialize()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        Initialize();
    }

    public new void RunOneFrame()
    {
        if(!Server)
        {
            base.RunOneFrame();
            return;
        }

        ObjectDisposedException.ThrowIf(_disposed, this);

        if(!_initialized)
        {
            DoInitialize();
            _initialized = true;
        }

        BeginRun();

        Tick();

        EndRun();
    }

    private void RunLoop()
    {
        while(!_shouldExit)
        {
            Tick();
        }

        EndRun();
    }

    public new void Exit()
    {
        if(!Server)
        {
            base.Exit();
            return;
        }

        _shouldExit = true;
    }

    public new void Tick()
    {
        if(!Server)
        {
            base.Tick();
            return;
        }

        while(true)
        {
            if (!IsActive && InactiveSleepTime.TotalMilliseconds >= 1.0)
            {
                Thread.Sleep((int)InactiveSleepTime.TotalMilliseconds);
            }

            if (_gameTimer == null)
            {
                _gameTimer = new Stopwatch();
                _gameTimer.Start();
            }

            long ticks = _gameTimer.Elapsed.Ticks;
            _accumulatedElapsedTime += TimeSpan.FromTicks(ticks - _previousTicks);
            _previousTicks = ticks;
            if (!IsFixedTimeStep || !(_accumulatedElapsedTime < TargetElapsedTime))
            {
                break;
            }

            if ((TargetElapsedTime - _accumulatedElapsedTime).TotalMilliseconds >= 2.0)
            {
                Thread.Sleep(1);
            }
        }

        if (_accumulatedElapsedTime > MaxElapsedTime)
        {
            _accumulatedElapsedTime = MaxElapsedTime;
        }

        if (IsFixedTimeStep)
        {
            _gameTime.ElapsedGameTime = TargetElapsedTime;
            int num = 0;
            while (_accumulatedElapsedTime >= TargetElapsedTime && !_shouldExit)
            {
                _gameTime.TotalGameTime += TargetElapsedTime;
                _accumulatedElapsedTime -= TargetElapsedTime;
                num++;
                DoUpdate(_gameTime);
            }

            _updateFrameLag += Math.Max(0, num - 1);
            if (_gameTime.IsRunningSlowly)
            {
                if (_updateFrameLag == 0)
                {
                    _gameTime.IsRunningSlowly = false;
                }
            }
            else if (_updateFrameLag >= 5)
            {
                _gameTime.IsRunningSlowly = true;
            }

            if (num == 1 && _updateFrameLag > 0)
            {
                _updateFrameLag--;
            }

            _gameTime.ElapsedGameTime = TimeSpan.FromTicks(TargetElapsedTime.Ticks * num);
        }
        else
        {
            _gameTime.ElapsedGameTime = _accumulatedElapsedTime;
            _gameTime.TotalGameTime += _accumulatedElapsedTime;
            _accumulatedElapsedTime = TimeSpan.Zero;
            DoUpdate(_gameTime);
        }

        if (_shouldExit)
        {
            ExitingEventArgs exitingEventArgs = new ExitingEventArgs();
            OnExiting(this, exitingEventArgs);
            if (!exitingEventArgs.Cancel)
            {
                EndRun();
                UnloadContent();
            }

            _shouldExit = false;
        }
    }

    public new void ResetElapsedTime()
    {
        if(!Server)
        {
            base.ResetElapsedTime();
            return;
        }

        if (_gameTimer != null)
        {
            _gameTimer.Reset();
            _gameTimer.Start();
        }

        _accumulatedElapsedTime = TimeSpan.Zero;
        _gameTime.ElapsedGameTime = TimeSpan.Zero;
        _previousTicks = 0L;
    }

    private void DoUpdate(GameTime gameTime)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        Update(gameTime);
        SuppressDraw();
    }

    protected override void Draw(GameTime gameTime)
    {
        if(!Server)
            base.Draw(gameTime);
    }
}
