using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Logging;

namespace SamplePlugin {
    public class CastState {
        public readonly uint actionId;
        public readonly float castTime;

        public CastState() {
            actionId = 0;
            castTime = 0.0f;
        }

        public CastState(uint actionId, float castTime) {
            this.actionId = actionId;
            this.castTime = castTime;
        }

        // individual casts don't have any sort of id, so check if same action
        // and if casting time has increased
        public bool IsSameCast(CastState other) {
            return actionId == other.actionId && castTime < other.castTime;
        }
    }

    public class ActionWatcher : IDisposable {
        private bool _enabled;
        public CastState? lastCast { get; private set; } = null;

        public ActionWatcher(bool enabled) {
            _enabled = enabled;
        }

        public void Enable() {
            if (_enabled) {
                return;
            }

            Dalamud.Framework.Update += OnFrameworkUpdate;
            _enabled = true;
        }

        public void Disable() {
            if (!_enabled) {
                return;
            }

            Dalamud.Framework.Update -= OnFrameworkUpdate;
            _enabled = false;
        }

        public void Dispose() => Disable();

        private void CheckCast() {
            var player = Dalamud.ClientState.LocalPlayer;

            if (player == null) {
                PluginLog.Error("player data could not be loaded");
                return;
            }

            var cast = new CastState(player.CastActionId, player.CurrentCastTime);

            if (lastCast!.IsSameCast(cast)) {
                lastCast = cast;
            }
        }

        public void OnFrameworkUpdate(object _) {
            CheckCast();
        }
    }
}
