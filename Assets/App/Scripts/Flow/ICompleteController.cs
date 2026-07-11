using System;
using System.Collections;
using Cysharp.Threading.Tasks;

namespace MagicTile.Flow
{
    public interface ICompleteController
    {
        void AddStep(CompleteStep step, Func<IEnumerator> routine);
        void AddStep(CompleteStep step, Func<UniTask> task);
        void RemoveStep(CompleteStep step);
        void ExecuteAll(Action onComplete = null);
    }
}
