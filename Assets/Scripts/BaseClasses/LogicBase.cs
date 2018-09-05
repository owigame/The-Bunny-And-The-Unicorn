using UnityEngine;

namespace AI
{
    public abstract class LogicBase : ScriptableObject
    {
        private AIResponseManager _AIResponder;

        public abstract void OnTick(IBoardState data);

        protected AIResponseManager AIResponse
        {
            get
            {
                return _AIResponder;
            }
        }

        public void init()
        {
            _AIResponder = new AIResponseManager();
            TournamentManager._instance.OnTick.AddListener(_AIResponder.onTick);
        }
    }
}