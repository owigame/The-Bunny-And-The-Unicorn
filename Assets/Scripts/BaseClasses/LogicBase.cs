using UnityEngine;

namespace AI
{
    public abstract class LogicBase : ScriptableObject
    {
        private AIResponseManagemer _AIResponder;

        public abstract void OnTick(IBoardState data);

        protected AIResponseManagemer AIResponse
        {
            get
            {
                return _AIResponder;
            }
        }

        public void init()
        {
            _AIResponder = new AIResponseManagemer();
            TournamentManager._instance.OnTick.AddListener(_AIResponder.onTick);
        }
    }
}