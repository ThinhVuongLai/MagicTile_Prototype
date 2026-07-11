using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MagicTile.Flow
{
    public class CompleteController : MonoBehaviour, ICompleteController
    {
        private abstract class StepEntry
        {
            public CompleteStep Step;
            public abstract IEnumerator AsCoroutine();
            public abstract UniTask AsTask();
        }

        private class CoroutineStepEntry : StepEntry
        {
            public Func<IEnumerator> Routine;
            public override IEnumerator AsCoroutine() => Routine();
            public override UniTask AsTask() => UniTask.CompletedTask;
        }

        private class UniTaskStepEntry : StepEntry
        {
            public Func<UniTask> Task;
            public override IEnumerator AsCoroutine() => null;
            public override UniTask AsTask() => Task();
        }

        private readonly List<StepEntry> _steps = new();

        public void AddStep(CompleteStep step, Func<IEnumerator> routine)
        {
            _steps.Add(new CoroutineStepEntry { Step = step, Routine = routine });
        }

        public void AddStep(CompleteStep step, Func<UniTask> task)
        {
            _steps.Add(new UniTaskStepEntry { Step = step, Task = task });
        }

        public void RemoveStep(CompleteStep step)
        {
            _steps.RemoveAll(s => s.Step == step);
        }

        public void ExecuteAll(Action onComplete = null)
        {
            ExecuteAllAsync(onComplete).Forget();
        }

        private async UniTask ExecuteAllAsync(Action onComplete)
        {
            for (int i = 0; i < _steps.Count; i++)
            {
                var entry = _steps[i];

                if (entry is UniTaskStepEntry)
                    await entry.AsTask();
                else
                    await RunCoroutineAsUniTask(entry.AsCoroutine());
            }

            _steps.Clear();
            onComplete?.Invoke();
        }

        private UniTask RunCoroutineAsUniTask(IEnumerator routine)
        {
            var tcs = new UniTaskCompletionSource();
            StartCoroutine(RunCoroutineAndComplete(routine, tcs));
            return tcs.Task;
        }

        private IEnumerator RunCoroutineAndComplete(IEnumerator routine, UniTaskCompletionSource tcs)
        {
            yield return StartCoroutine(routine);
            tcs.TrySetResult();
        }
    }
}
