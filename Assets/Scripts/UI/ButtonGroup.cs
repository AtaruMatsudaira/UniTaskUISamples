using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

namespace UI
{
    public class ButtonGroup : IDisposable
    {
        private readonly Dictionary<string, (Button btn, Func<CancellationToken, UniTask> eventAction)> _eventMap =
            new(4);

        private readonly CancellationTokenSource _cts = new();

        private Func<CancellationToken, UniTask> _preAsyncEvent;
        private Func<CancellationToken, UniTask> _postAsyncEvent;

        public void AddButton(Button button, Func<CancellationToken, UniTask> eventAction, string key = default)
        {
            key ??= Guid.NewGuid().ToString();

            _eventMap.Add(key, (button, eventAction));
        }

        public void AddPreAsyncEvent(Func<CancellationToken, UniTask> preClickEvent)
        {
            _preAsyncEvent = preClickEvent;
        }

        public void AddPostAsyncEvent(Func<CancellationToken, UniTask> postClickEvent)
        {
            _postAsyncEvent = postClickEvent;
        }

        public void Remove(string key)
        {
            _eventMap.Remove(key);
        }

        public async UniTask RunAsync(CancellationToken ct)
        {
            var clickHandlers = _eventMap
                .Values
                .Select(x => (x.btn, x.eventAction))
                .ToList();

            while (!ct.IsCancellationRequested)
            {
                var clickCts = CancellationTokenSource.CreateLinkedTokenSource(ct, _cts.Token);

                var clickedIndex = await UniTask.WhenAny(
                    clickHandlers
                        .Select(h => h.btn.OnClickAsync(ct)));

                if (_preAsyncEvent != null)
                {
                    await _preAsyncEvent(ct);
                }

                await clickHandlers[clickedIndex].eventAction(clickCts.Token);

                if (_postAsyncEvent != null)
                {
                    await _postAsyncEvent(ct);
                }
            }
        }

        public void Dispose()
        {
            _eventMap.Clear();
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}