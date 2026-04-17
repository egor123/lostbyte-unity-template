using System;
using System.Collections.Generic;
using Lostbyte.Toolkit.FactSystem;
using Lostbyte.Toolkit.CustomEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.FSM
{
    public interface IState
    {
        bool Enabled { get; set; }
        void OnEnter();
        void OnUpdate();
        void OnFixedUpdate();
        void OnExit();
    }

    public interface IStateMachine<TValue> where TValue : Enum
    {
        IFactWrapper<Enum> State { get; }
        TValue StateValue { get; set; }
        internal Dictionary<Enum, IState> States { get; set; }
        internal Dictionary<Enum, Dictionary<Enum, List<Func<bool>>>> Transitions { get; set; }
        internal Dictionary<Enum, List<Func<bool>>> AnyTransitions { get; set; }
        internal Dictionary<Enum, Enum> DefaultTransitions { get; set; }
    }

    public static class StateMachineUtils
    {
        public static void Exit(this IState state)
        {
            if (state != null && state.Enabled)
            {
                state.Enabled = false;
                state.OnExit();
            }
        }
        public static void Enter(this IState state)
        {
            if (state != null)
            {
                state.Enabled = true;
                state.OnEnter();
            }
        }
        public static void Update(this IState state)
        {
            if (state != null && state.Enabled) state.OnUpdate();
        }
        public static void FixedUpdate(this IState state)
        {
            if (state != null && state.Enabled) state.OnFixedUpdate();
        }
        public static IState GetCurrentState<TValue>(this IStateMachine<TValue> fsm) where TValue : Enum => fsm.States.GetValueOrDefault(fsm.State.Value);
        public static void HandleTransitions<TValue>(this IStateMachine<TValue> fsm) where TValue : Enum
        {
            if (fsm.States.TryGetValue(fsm.State.Value, out var state) && !state.Enabled)
                if (fsm.DefaultTransitions.TryGetValue(fsm.State.Value, out var to))
                    fsm.State.Value = to;
            if (fsm.Transitions.TryGetValue(fsm.State.Value, out var transitions))
                foreach (var transition in transitions)
                    foreach (var condition in transition.Value)
                        if (condition.Invoke())
                            fsm.State.Value = transition.Key;
            foreach (var transition in fsm.AnyTransitions)
                foreach (var condition in transition.Value)
                    if (condition.Invoke())
                        fsm.State.Value = transition.Key;
        }
        internal static void Transition<TValue>(this IStateMachine<TValue> fsm, Enum from, Enum to) where TValue : Enum//TODO check for inner submachines
        {
            if (from != null && from.Equals(to) && fsm.States.TryGetValue(from, out var state) && state.Enabled) return;
            if (from != null && fsm.States.TryGetValue(from, out state) && state.Enabled) state.Exit();
            if (to != null && fsm.States.TryGetValue(to, out state)) state.Enter();
        }
        public static void AddState<TValue>(this IStateMachine<TValue> fsm, TValue value, IState state) where TValue : Enum => fsm.States[value] = state;
        public static void RemoveState<TValue>(this IStateMachine<TValue> fsm, TValue value) where TValue : Enum => fsm.States.Remove(value);
        public static void AddTransition<TValue>(this IStateMachine<TValue> fsm, TValue from, TValue to, Func<bool> condition) where TValue : Enum => fsm.Transitions.GetValueOrAssignDefault(from, new()).GetValueOrAssignDefault(to, new()).Add(condition);
        public static void RemoveTransition<TValue>(this IStateMachine<TValue> fsm, TValue from, TValue to, Func<bool> condition) where TValue : Enum => fsm.Transitions.GetValueOrAssignDefault(from, new()).GetValueOrAssignDefault(to, new()).Remove(condition);
        public static void AddAnyTransition<TValue>(this IStateMachine<TValue> fsm, TValue to, Func<bool> condition) where TValue : Enum => fsm.AnyTransitions.GetValueOrAssignDefault(to, new()).Add(condition);
        public static void RemoveAnyTransition<TValue>(this IStateMachine<TValue> fsm, TValue to, Func<bool> condition) where TValue : Enum => fsm.AnyTransitions.GetValueOrAssignDefault(to, new()).Remove(condition);
        public static void AddDefaultTransition<TValue>(this IStateMachine<TValue> fsm, TValue from, TValue to) where TValue : Enum => fsm.DefaultTransitions[from] = to;
        public static void RemoveDefaultTransition<TValue>(this IStateMachine<TValue> fsm, TValue from) where TValue : Enum => fsm.DefaultTransitions.Remove(from);
    }
    internal static class FSMUtils
    {
        internal static Value GetValueOrAssignDefault<Key, Value>(this Dictionary<Key, Value> dictionary, Key key, Value @default = default)
        {
            return dictionary.TryGetValue(key, out Value value) ? value : dictionary[key] = @default;
        }
    }
    [RequireComponent(typeof(KeyReference))]
    public abstract class StateMachine<TValue> : MonoBehaviour, IStateMachine<TValue> where TValue : Enum
    {
        [field: SerializeField, Autowired(Autowired.Type.Parent, true), Hide] public KeyReference Key { get; protected set;  }
        [field: SerializeField, Group("Facts")] public SelfFactWrapper<Enum> State { get; private set; }
        Dictionary<Enum, IState> IStateMachine<TValue>.States { get; set; } = new();
        Dictionary<Enum, Dictionary<Enum, List<Func<bool>>>> IStateMachine<TValue>.Transitions { get; set; } = new();
        Dictionary<Enum, List<Func<bool>>> IStateMachine<TValue>.AnyTransitions { get; set; } = new();
        Dictionary<Enum, Enum> IStateMachine<TValue>.DefaultTransitions { get; set; } = new();
        IFactWrapper<Enum> IStateMachine<TValue>.State => State;
        public TValue StateValue
        {
            get => (TValue)State.Value;
            set => State.Value = value;
        }


        protected virtual void Awake()
        {
            // State.Value = State.GetDefaultValue();
            State.Subscribe(this.Transition);
        }
        protected virtual void Start()
        {
            this.Transition(null, StateValue);
        }
        protected virtual void OnDestroy() => State.Unsubscribe(this.Transition);
        protected virtual void Update()
        {
            this.HandleTransitions();
            this.GetCurrentState().Update();
        }
        protected virtual void FixedUpdate() => this.GetCurrentState().FixedUpdate();
    }

    public abstract class SubStateMachine<TValue> : MonoBehaviour, IStateMachine<TValue>, IState where TValue : Enum
    {
        [field: SerializeField] public SelfFactWrapper<Enum> State { get; private set; }
        [field: SerializeField, ReadOnly] public bool Enabled { get; set; }
        Dictionary<Enum, IState> IStateMachine<TValue>.States { get; set; } = new();
        Dictionary<Enum, Dictionary<Enum, List<Func<bool>>>> IStateMachine<TValue>.Transitions { get; set; } = new();
        Dictionary<Enum, List<Func<bool>>> IStateMachine<TValue>.AnyTransitions { get; set; } = new();
        Dictionary<Enum, Enum> IStateMachine<TValue>.DefaultTransitions { get; set; } = new();
        IFactWrapper<Enum> IStateMachine<TValue>.State => State;
        public TValue StateValue
        {
            get => (TValue)State.Value;
            set => State.Value = value;
        }
        protected virtual void Awake()
        {
            // State.Value = State.GetDefaultValue();
            State.Subscribe(this.Transition);
        }
        protected virtual void OnDestroy() => State.Unsubscribe(this.Transition);
        public void OnEnter() => this.GetCurrentState().Enter();
        public void OnUpdate()
        {
            this.HandleTransitions();
            this.GetCurrentState().Update();
        }
        public void OnFixedUpdate() => this.GetCurrentState().FixedUpdate();
        public void OnExit() => this.GetCurrentState().Exit();
    }

    public class State : IState
    {
        public bool Enabled { get; set; }
        private readonly Action<State> OnEnterCallback, OnUpdateCallback, OnFixedOpdateCallback, OnExitCallback;
        public State(Action<State> onEnter = null, Action<State> onUpdate = null, Action<State> onFixedUpdate = null, Action<State> onExit = null)
        {
            OnEnterCallback = onEnter;
            OnUpdateCallback = onUpdate;
            OnFixedOpdateCallback = onFixedUpdate;
            OnExitCallback = onExit;
        }
        public void OnEnter() => OnEnterCallback?.Invoke(this);
        public void OnExit() => OnExitCallback?.Invoke(this);
        public void OnFixedUpdate() => OnFixedOpdateCallback?.Invoke(this);
        public void OnUpdate() => OnUpdateCallback?.Invoke(this);
    }

    public class SequenceState : IState //TODO fix log term callbacks!!!
    {
        public bool Enabled { get; set; }
        private readonly IState[] _states;
        private int _idx;
        public Action OnExitCallback;
        public SequenceState(params IState[] states) => _states = states;
        public void OnEnter()
        {
            if (_states != null && _states.Length > 0)
                _states[_idx = 0].Enter();
            else
                this.Exit();
        }
        public void OnExit()
        {
            if (_states != null && _idx < _states.Length)
                _states[_idx].Exit();
            OnExitCallback?.Invoke();
        }
        public void OnUpdate()
        {
            while (_idx < _states.Length && !_states[_idx].Enabled)
            {
                if (++_idx < _states.Length)
                {
                    _states[_idx].Enter();
                }
                else
                {
                    this.Exit();
                    return;
                }
            }
            _states[_idx].Update();
        }
        public void OnFixedUpdate() => _states[_idx].FixedUpdate();
    }

    public class SingleActionState : IState
    {
        public bool Enabled { get; set; }
        private readonly Action _onEnter;
        public SingleActionState(Action onEnter) => _onEnter = onEnter;
        public void OnEnter()
        {
            _onEnter?.Invoke();
            this.Exit();
        }
        public void OnExit() { }
        public void OnFixedUpdate() { }
        public void OnUpdate() { }
    }

    public class WaitState : IState
    {
        public bool Enabled { get; set; }
        private float _time;
        private float _timer;
        public WaitState(float time) => _time = time;

        public void OnEnter() => _timer = 0;
        public void OnExit() { }
        public void OnFixedUpdate() { }
        public void OnUpdate()
        {
            if ((_timer += Time.deltaTime) > _time)
                this.Exit();
        }
    }
}
