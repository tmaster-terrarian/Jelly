// Involves syntax edits to fit .NET core 8.0 standards.

#region
/*

MIT License

Copyright (c) 2017 Chevy Ray Johnston

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*/
//github: https://github.com/ChevyRay/Coroutines/tree/master
//FIX github: https://github.com/Linerichka/Coroutines
#endregion

using System.Collections;
using System.Collections.Generic;

namespace Jelly.Coroutines;

/// <summary>
/// A container for running multiple routines in parallel. Coroutines can be nested.
/// </summary>
public class CoroutineRunner
{
    /// Use it if each coroutine call should be close to the time of its expected call.  
    /// However, this may cause IEnumerator calls to be made at irregular intervals between each other.
    public bool UnscaledTime { get; set; } = true;

    /// <summary>
    /// How many coroutines are currently running.
    /// </summary>
    public int Count => _coroutines.Count;

    private readonly Dictionary<string, CoroutineHandle> _coroutines = [];

    public CoroutineHandle Run(string methodName, IEnumerator enumerator, float delay = 0f)
    {
        CoroutineHandle coroutineHandle = new(this, methodName, delay, enumerator);
        _coroutines.Add(methodName, coroutineHandle);
        return coroutineHandle;
    }

    public bool Stop(string methodName)
    {
        if (_coroutines.TryGetValue(methodName, out _))
        {
            _coroutines.Remove(methodName);
            return true;
        }
        else return false;
    }

    public CoroutineHandle GetHandle(string methodName) => _coroutines[methodName];

    /// <summary>
    /// Stop all running routines.
    /// </summary>
    public void StopAll() => _coroutines.Clear();

    /// <summary>
    /// Check if the routine is currently running.
    /// </summary>
    /// <returns>True if the routine is running.</returns>
    public bool IsRunning(string methodName) => _coroutines.TryGetValue(methodName, out _);

    /// <summary>
    /// Update all running coroutines.
    /// </summary>
    /// <param name="deltaTime">How many seconds have passed sinced the last update.</param>
    public void Update(float deltaTime)
    {
        Queue<string> corountineForRemoval = [];

        foreach(var coroutine in _coroutines.Values)
        {
            if(coroutine.Delay > 0)
            {
                coroutine.Delay -= deltaTime;

                if(coroutine.Delay > 0) continue;
            }

            bool moveNext = MoveNext(coroutine, deltaTime);

            if(!moveNext)
            {
                corountineForRemoval.Enqueue(coroutine.MethodName);
            }
        }

        while(corountineForRemoval.TryDequeue(out string methodName))
        {
            _coroutines.Remove(methodName);
        }
    }

    private bool MoveNext(CoroutineHandle coroutine, float deltaTime)
    {
        bool result = coroutine.Enumerator.MoveNext();

        if(!result)
            return false;
        else if(coroutine.Enumerator.Current is null)
            coroutine.Delay += deltaTime;
        else if(coroutine.Enumerator.Current is float current)
        {
            if(UnscaledTime)
                coroutine.Delay += current;
            else
                coroutine.Delay = current;
        }

        return result;
    }
}

public class CoroutineHandle(CoroutineRunner runner, string methodName, float delay, IEnumerator enumerator)
{
    public CoroutineRunner Runner => runner;
    public string MethodName => methodName;
    public float Delay { get; set; } = delay;
    public IEnumerator Enumerator => enumerator;

    /// <summary>
    /// Stop this coroutine if it is running.
    /// </summary>
    /// <returns>True if the coroutine was stopped.</returns>
    public bool Stop() => Runner.Stop(MethodName);

    /// <summary>
    /// A routine to wait until this coroutine has finished running.
    /// </summary>
    /// <returns>The wait enumerator.</returns>
    public IEnumerator Wait()
    {
        if (Enumerator != null)
        {
            while(Runner.IsRunning(MethodName))
            {
                yield return null;
            }
        }
    }

    /// <summary>
    /// True if the enumerator is currently running.
    /// </summary>
    public bool IsRunning => Runner.IsRunning(MethodName);
}
