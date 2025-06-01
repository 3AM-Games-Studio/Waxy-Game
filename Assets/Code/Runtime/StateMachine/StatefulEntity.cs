using UnityEngine;

namespace UnityUtils.StateMachine {
    public abstract class StatefulEntity : MonoBehaviour {
        protected StateMachine stateMachine;

        protected void At<T>(IState from, IState to, T condition) => stateMachine.AddTransition(from, to, condition);

        protected void Any<T>(IState to, T condition) => stateMachine.AddAnyTransition(to, condition);
    }
}