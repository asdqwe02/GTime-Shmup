using System;
using System.Collections;
using UnityEngine;

namespace PKFramework.Runner
{
    public interface IRunner
    {
        void ScheduleUpdate(Action action);
        void UnscheduleUpdate(Action action);
        void ScheduleLateUpdate(Action action);
        void UnscheduleLateUpdate(Action action);
        void CallOnMainThread(Action action);
        Coroutine StartCoroutine(IEnumerator routine);
        void StopCoroutine(Coroutine routine);
        void StopCoroutine(IEnumerator routine);
    }
}