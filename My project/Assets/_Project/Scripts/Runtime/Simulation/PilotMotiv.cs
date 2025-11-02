using System;
using UnityEngine;

namespace _Project.Scripts.Simulation
{
    /// <summary>
    /// Содержит параметры активной миссии пилота и стек исполняемых подзадач.
    /// </summary>
    public struct PilotMotiv
    {
        public MissionState Mission;
        public ExecutionState Execution;

        public static PilotMotiv Idle()
        {
            return new PilotMotiv
            {
                Mission = MissionState.CreateIdle(),
                Execution = ExecutionState.CreateEmpty()
            };
        }

        public readonly EPilotSubTasks ActiveTaskKind
        {
            get
            {
                var stack = Execution.TaskStack;
                return stack.Count > 0 ? stack.Peek().Kind : EPilotSubTasks.None;
            }
        }

        public struct MissionState
        {
            public EPilotTasks Kind;
            public MissionParameters Parameters;

            public static MissionState CreateIdle()
            {
                return new MissionState
                {
                    Kind = EPilotTasks.None,
                    Parameters = default
                };
            }

            public static MissionState CreatePatrol(Vector3 center, float radius)
            {
                return new MissionState
                {
                    Kind = EPilotTasks.Patrol,
                    Parameters = new MissionParameters
                    {
                        Patrol = new PatrolMissionParameters
                        {
                            Center = center,
                            Radius = radius
                        }
                    }
                };
            }
        }

        public struct MissionParameters
        {
            public PatrolMissionParameters Patrol;
        }

        public struct PatrolMissionParameters
        {
            public Vector3 Center;
            public float Radius;
        }

        public struct ExecutionState
        {
            public TaskStack TaskStack;

            public static ExecutionState CreateEmpty()
            {
                return new ExecutionState
                {
                    TaskStack = TaskStack.Create()
                };
            }
        }

        public struct TaskStack
        {
            private TaskFrame[] _frames;
            private int _count;

            public static TaskStack Create()
            {
                return new TaskStack
                {
                    _frames = Array.Empty<TaskFrame>(),
                    _count = 0
                };
            }

            public int Count => _count;

            public TaskFrame Peek()
            {
                if (_count == 0)
                    throw new InvalidOperationException("Task stack is empty.");

                return _frames[_count - 1];
            }

            public void Push(in TaskFrame frame)
            {
                EnsureCapacity(_count + 1);
                _frames[_count++] = frame;
            }

            public void ReplaceTop(in TaskFrame frame)
            {
                if (_count == 0)
                    throw new InvalidOperationException("Task stack is empty.");

                _frames[_count - 1] = frame;
            }

            public void Pop()
            {
                if (_count > 0)
                    _count--;
            }

            private void EnsureCapacity(int size)
            {
                if (_frames == null)
                    _frames = Array.Empty<TaskFrame>();

                if (_frames.Length >= size)
                    return;

                var newCapacity = _frames.Length == 0 ? 4 : _frames.Length * 2;
                while (newCapacity < size)
                    newCapacity *= 2;

                Array.Resize(ref _frames, newCapacity);
            }
        }

        public struct TaskFrame
        {
            public EPilotSubTasks Kind;
            public TaskPayload Payload;

            public static TaskFrame CreatePatrolMove(float desiredSpeed, uint randomState)
            {
                return new TaskFrame
                {
                    Kind = EPilotSubTasks.PatrolMove,
                    Payload = new TaskPayload
                    {
                        Patrol = new PatrolTaskPayload
                        {
                            DesiredSpeed = desiredSpeed,
                            RandomState = randomState,
                            HasTarget = false,
                            CurrentTarget = Vector3.zero
                        }
                    }
                };
            }
        }

        public struct TaskPayload
        {
            public PatrolTaskPayload Patrol;
        }

        public struct PatrolTaskPayload
        {
            public Vector3 CurrentTarget;
            public bool HasTarget;
            public uint RandomState;
            public float DesiredSpeed;
        }
    }
}
