using System.Collections.Generic;
using UnityEngine;

namespace AI {
    public abstract class LogicBase : ScriptableObject {
        private AIResponseManager _AIResponder;
        public List<CreatureBase> _Creatures = new List<CreatureBase> ();
        int _PlayerNumber = 0;

        public abstract void OnTick (IBoardState data);
        // public abstract void OnValidateFail(IBoardState data,IResponse[] chain);

        protected AIResponseManager AIResponse {
            get {
                return _AIResponder;
            }
        }

        public void init () {
            if (this is Human) {
                (this as Human).Start ();
            }

            _PlayerNumber = TournamentManager._instance.P1 == this ? 1 : 2;

            _AIResponder = new AIResponseManager (this);
            TournamentManager._instance.OnTick.AddListener (_AIResponder.onTick);
        }
    }
}