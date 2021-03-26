﻿using Ryujinx.Common.Logging;
using System;
using System.Threading;
using static SDL2.SDL;

namespace Ryujinx.Gamepad.SDL2
{
    public class SDL2Driver : IDisposable
    {
        private static SDL2Driver _instance;

        public static bool IsInitialized => _instance != null;

        public static SDL2Driver Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SDL2Driver();
                }

                return _instance;
            }
        }

        private const uint SdlInitFlags = SDL_INIT_EVENTS | SDL_INIT_GAMECONTROLLER | SDL_INIT_JOYSTICK | SDL_INIT_HAPTIC;

        private bool _isRunning;
        private Thread _worker;

        public event Action<int, int> OnJoyStickConnected;
        public event Action<int> OnJoystickDisconnected;

        private SDL2Driver() {}

        public void Init()
        {
            if (_isRunning)
            {
                return;
            }

            if (SDL_Init(SdlInitFlags) != 0)
            {
                string errorMessage = $"SDL2 initlaization failed with error \"{SDL_GetError()}\"";

                Logger.Error?.Print(LogClass.Application, errorMessage);

                throw new Exception(errorMessage);
            }

            // First ensure that we only enable joystick events (for connected/disconnected).
            SDL_GameControllerEventState(SDL_DISABLE);
            SDL_JoystickEventState(SDL_ENABLE);

            // Disable all joysticks information, we don't need them no need to flood the event queue for that.
            SDL_EventState(SDL_EventType.SDL_JOYAXISMOTION, SDL_DISABLE);
            SDL_EventState(SDL_EventType.SDL_JOYBALLMOTION, SDL_DISABLE);
            SDL_EventState(SDL_EventType.SDL_JOYHATMOTION, SDL_DISABLE);
            SDL_EventState(SDL_EventType.SDL_JOYBUTTONDOWN, SDL_DISABLE);
            SDL_EventState(SDL_EventType.SDL_JOYBUTTONUP, SDL_DISABLE);

            _worker = new Thread(EventWorker);
            _isRunning = true;
            _worker.Start();
        }

        private unsafe int FilterEvent(IntPtr userdata, IntPtr sdlevent)
        {
            HandleSDLEvent(ref *((SDL_Event*)sdlevent));

            return 0;
        }

        private void HandleSDLEvent(ref SDL_Event evnt)
        {
            Logger.Error?.Print(LogClass.Application, $"{evnt.type}");

            if (evnt.type == SDL_EventType.SDL_JOYDEVICEADDED)
            {
                int deviceId = evnt.cbutton.which;

                // SDL2 loves to be inconsistent here by providing the device id instead of the instance id (like on removed event), as such we just grab it and send it inside our system.
                int instanceId = SDL_JoystickGetDeviceInstanceID(evnt.cbutton.which);

                if (instanceId == -1)
                {
                    return;
                }

                Logger.Debug?.Print(LogClass.Application, $"Added joystick instance id {evnt.cbutton.which}");

                OnJoyStickConnected?.Invoke(deviceId, instanceId);
            }
            else if (evnt.type == SDL_EventType.SDL_JOYDEVICEREMOVED)
            {
                Logger.Debug?.Print(LogClass.Application, $"Removed joystick instance id {evnt.cbutton.which}");

                OnJoystickDisconnected?.Invoke(evnt.cbutton.which);
            }
        }

        private void EventWorker()
        {
            // Change this... maybe
            const int WaitTimeMs = 10;

            ManualResetEventSlim waitHandle = new ManualResetEventSlim(false);

            while (_isRunning)
            {
                while (SDL_PollEvent(out SDL_Event evnt) != 0)
                {
                    HandleSDLEvent(ref evnt);
                }

                waitHandle.Wait(WaitTimeMs);
            }

            waitHandle.Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _isRunning = false;

                _worker?.Join();

                SDL_Quit();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}