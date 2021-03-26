﻿using Ryujinx.Common.Configuration.HidNew;
using Ryujinx.Common.Memory;
using System;
using System.Runtime.CompilerServices;

namespace Ryujinx.Gamepad
{
    public interface IGamepad : IDisposable
    {
        string Id { get; }

        string Name { get; }

        bool IsConnected { get; }

        bool IsPressed(GamepadInputId inputId);

        (float, float) GetStick(StickInputId inputId);

        void SetTriggerThreshold(float triggerThreshold);

        void SetConfiguration(InputConfig configuration);

        GamepadStateSnapshot GetMappedStateSnapshot();
        GamepadStateSnapshot GetStateSnapshot();


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GamepadStateSnapshot GetStateSnapshot(IGamepad gamepad)
        {
            // NOTE: Update Array size if JoystickInputId is changed.
            Array2<Array2<float>> joysticksState = default;

            for (StickInputId inputId = 0; inputId < StickInputId.Count; inputId++)
            {
                (float state0, float state1) = gamepad.GetStick(inputId);

                Array2<float> state = default;

                state[0] = state0;
                state[1] = state1;

                joysticksState[(int)inputId] = state;
            }

            // NOTE: Update Array size if GamepadInputId is changed.
            Array17<bool> buttonsState = default;

            for (GamepadInputId inputId = 0; inputId < GamepadInputId.Count; inputId++)
            {
                buttonsState[(int)inputId] = gamepad.IsPressed(inputId);
            }

            return new GamepadStateSnapshot(joysticksState, buttonsState);
        }
    }
}